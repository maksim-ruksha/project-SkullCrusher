using System;
using Preferences;
using SingleInstance;
using UnityEngine;
using Util;

namespace Character
{
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class Controller : MonoBehaviour
    {
        public Transform cameraTransform;

        [Header("Movement")] public float moveSpeed = 10.0f;

        public float groundAccelerationSpeed = 1.0f;

        public float jumpForce = 1.0f;
        public Vector3 groundSphereCenter = Vector3.down;

        public float groundSphereRadius = 0.5f;
        public float groundSphereCastDistance = 0.05f;

        [Range(1, 89)] public float slideAngle = 45;

        public float friction = 10.0f;
        public float airAccelerationSpeed = 1.0f;
        public float airDecelerationSpeed = 1.5f;
        public float airControlAccuracy = 16f;
        public float airControlAdditionalForward = 1.0f;
        public LayerMask walkableLayer;

        //public LayerMask collisionableLayer;

        [Header("Gravity")] public float gravityMultiplier = 10;
        [Header("Crouch")] public float crouchColliderHeight = 0.5f;
        public float crouchMoveSpeed = 3.0f;
        public float crouchSpeedMultiplier = 1.0f;
        public Vector3 crouchCameraCenter = new Vector3(0.0f, 0.25f, 0.0f);
        public Vector3 ceilingSphereCenter;
        public float ceilingSphereRadius = 0.5f;
        public float ceilingSphereCastDistance = 0.1f;
        [Header("Look")] public float sensitivity = 10.0f;
        public bool interpolateLook = true;
        public float interpolationSpeed = 50.0f;
        public bool invertX;
        public bool invertY;
        [Range(0, 90)] public float upAngleLimit = 90;
        [Range(-90, 0)] public float downAngleLimit = -90;
        [Header("Lock Inputs")] public bool lockMove = false;
        public bool lockLook = false;
        public bool lockJump = false;
        public bool lockInteract = false;


        # region DEV

        [Header("Be careful with this shit")] public float speedThreshold = 0.00001f;
        public int maxCollisionsHandle = 10;
        public float frictionSpeedThreshold = 0.5f;
        public float inputThreshold = 0.0001f;

        #endregion


        private Vector3 velocity;

        private InputManager inputManager;

        private bool isGroundedInPreviousFrame;

        private bool isCrouching;
        //private bool wasCrouchingBeforeJump;

        private float initialColliderHeight;

        private Vector3 initialCameraPosition;

        private Rigidbody rigidbody;

        private CapsuleCollider collider;

        // in a perfect world
        // values like this would not exist
        // but this is not a perfect world
        private float lastDeltaTime = 1.0f;
        // interpolateLook = true
        private Quaternion realRotation;
        

        private void Start()
        {
            inputManager = GameObject.Find(Settings.GameObjects.GlobalController).GetComponent<InputManager>();

            rigidbody = GetComponent<Rigidbody>();
            collider = GetComponent<CapsuleCollider>();


            initialColliderHeight = collider.height;
            initialCameraPosition = cameraTransform.localPosition;

            Gravity.Set(Physics.gravity.normalized);
        }

        private void Update()
        {
            Control();
        }


        private void Control()
        {
            velocity = rigidbody.velocity;
            MoveControl(inputManager.move);
            /*velocity = inputManager.move * moveSpeed;
            rigidbody.velocity = velocity;*/
            LookControl(inputManager.look);
            CrouchControl(inputManager.isCrouchPressed);
            lastDeltaTime = Time.deltaTime;
        }

        private void MoveControl(Vector3 input)
        {
            Vector3 accelerateDirection = GetMovementDirection(input.normalized);

            Vector3 groundNormal;
            Vector3 groundPosition;
            bool isGrounded = IsGrounded(out groundNormal, out groundPosition);

            if (isGrounded) //ground movements
            {
                bool isWalkableGround = Vector3.Angle(groundNormal, Vector3.up) < slideAngle;
                if (isWalkableGround)
                {
                    // Acceleration(accelerateDirection, isCrouching ? moveSpeed * 0.25f : moveSpeed);
                    // Acceleration(accelerateDirection, moveSpeed);
                    Acceleration(accelerateDirection, isCrouching ? crouchMoveSpeed : moveSpeed);
                    velocity = Vector3.ProjectOnPlane(velocity, groundNormal);

                    if (isGroundedInPreviousFrame && !inputManager.isJumpPressed)
                    {
                        Friction();
                    }

                    if (inputManager.isJumpPressed)
                    {
                        velocity += Gravity.up * (isCrouching ? jumpForce * 0.5f : jumpForce);
                    }
                }
                else
                {
                    //Slide down
                    velocity += Gravity.down * (Time.deltaTime * gravityMultiplier);
                }
            }
            else //air movement
            {
                float speed = Vector3.Dot(velocity, accelerateDirection) > 0
                    ? airAccelerationSpeed
                    : airDecelerationSpeed;
                Acceleration(accelerateDirection, speed);
                if (Math.Abs(input.y) > inputThreshold)
                {
                    AirControl(accelerateDirection);
                }

                velocity += Gravity.down * (Time.deltaTime * gravityMultiplier);
            }

            rigidbody.velocity = velocity;

            isGroundedInPreviousFrame = isGrounded;
        }

        private void Acceleration(Vector3 accelerateDirection, float speed)
        {
            float speedProjection = Vector3.Dot(velocity, accelerateDirection);

            float deltaSpeed = speed - speedProjection;
            if (deltaSpeed < 0)
                return;

            float newAcceleration = groundAccelerationSpeed * speed * Time.deltaTime;
            if (newAcceleration > deltaSpeed)
                newAcceleration = deltaSpeed;
            velocity += accelerateDirection * newAcceleration;
        }

        private void Friction()
        {
            float speed = velocity.magnitude;
            if (speed <= speedThreshold)
            {
                return;
            }

            float downLimit = Mathf.Max(speed, frictionSpeedThreshold); // Don't drop below treshold

            var dropAmount = speed - (downLimit * friction * Time.deltaTime);
            if (dropAmount < 0)
            {
                dropAmount = 0;
            }

            velocity *= dropAmount / speed; // Reduce the velocity by a certain percent
        }

        private void AirControl(Vector3 accelerateDirection)
        {
            Vector3 horizontalVelocity = velocity.ToHorizontal().normalized;
            float horizontalVelocitySpeed = velocity.ToHorizontal().magnitude;

            float dot = Vector3.Dot(horizontalVelocity, accelerateDirection);
            if (dot > 0)
            {
                float k = airControlAccuracy * dot * dot * Time.deltaTime;
                bool isPureForward =
                    Math.Abs(inputManager.move.x) < inputThreshold && Math.Abs(inputManager.move.y) > 0;
                if (isPureForward)
                {
                    k *= airControlAdditionalForward;
                }

                horizontalVelocity =
                    horizontalVelocity * horizontalVelocitySpeed + accelerateDirection * k;
                horizontalVelocity.Normalize();


                velocity = (horizontalVelocity * horizontalVelocitySpeed).ToHorizontal() +
                           Gravity.up * velocity.VerticalComponent();
            }
        }


        private void LookControl(Vector3 input)
        {
            if (lockLook)
                return;

            if (!invertY)
                input.y *= -1;
            if (invertX)
                input.x *= -1;
            input = new Vector3(input.y, input.x) * sensitivity;

            Vector3 instantRotation = realRotation.eulerAngles;
            instantRotation += input;
            if (instantRotation.x > 180)
                instantRotation.x -= 360;

            instantRotation.x = Mathf.Clamp(instantRotation.x, downAngleLimit, upAngleLimit);

            realRotation = Quaternion.Euler(instantRotation);

            if (interpolateLook)
                cameraTransform.rotation = Quaternion.Lerp(cameraTransform.rotation, realRotation,
                    Time.deltaTime * interpolationSpeed);
            else
                cameraTransform.rotation = realRotation;
        }

        private void CrouchControl(bool input)
        {
            bool canUncrouch = CanUncrouch();
            if (IsGrounded(out _, out _))
            {
                if (input)
                {
                    isCrouching = true;
                    collider.height = Mathf.Lerp(collider.height, crouchColliderHeight,
                        Time.deltaTime * crouchSpeedMultiplier);
                }
                else
                {
                    if (canUncrouch)
                    {
                        isCrouching = false;
                        collider.height = Mathf.Lerp(collider.height, initialColliderHeight,
                            Time.deltaTime * crouchSpeedMultiplier);
                    }
                }
            }
            else
            {
                // for better gameplay experience player will crouch instantly in air
                if (canUncrouch)
                {
                    isCrouching = input;
                    collider.height = input ? crouchColliderHeight : initialColliderHeight;
                }
            }

            // camera movement
            if (isCrouching)
            {
                cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, crouchCameraCenter,
                    Time.deltaTime * crouchSpeedMultiplier);
            }
            else
            {
                if (canUncrouch)
                    cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, initialCameraPosition,
                        Time.deltaTime * crouchSpeedMultiplier);
            }
        }

        private bool CanUncrouch()
        {
            float distanceToPoint = GetCapsulePointDistance();
            Ray ray = new Ray(transform.position + Vector3.up * distanceToPoint + ceilingSphereCenter, Vector3.up);
            RaycastHit[] hits = new RaycastHit[maxCollisionsHandle];
            int size = Physics.SphereCastNonAlloc(ray, ceilingSphereRadius, hits, ceilingSphereCastDistance,
                walkableLayer);
            if (size <= 0) return true;

            foreach (RaycastHit hit in hits)
            {
                if (!hit.collider)
                    continue;
                return false;
            }

            return true;
        }


        private void InteractControl(bool input)
        {
            // TODO: implement interactions
        }

        private bool IsGrounded(out Vector3 groundNormal, out Vector3 hitPoint)
        {
            groundNormal = Vector3.down;
            hitPoint = Vector3.zero;
            RaycastHit[] hits = new RaycastHit[maxCollisionsHandle];


            // cant calculate it once at start because player can crouch
            float distanceToPoint = GetCapsulePointDistance();
            /*Physics.SphereCastNonAlloc(transform.position + Vector3.down * distanceToPoint + groundSphereCenter,
                groundSphereRadius, Vector3.down, hits,
                groundSphereCastDistance, walkableLayer);*/
            Ray ray = new Ray(transform.position + Vector3.down * distanceToPoint + groundSphereCenter, Vector3.down);
            int size = Physics.SphereCastNonAlloc(ray, groundSphereRadius, hits, groundSphereCastDistance,
                walkableLayer);
            if (size > 0)
            {
                Vector3 normal = Vector3.zero;
                Vector3 point = Vector3.zero;
                foreach (RaycastHit hit in hits)
                {
                    if (!hit.collider)
                        continue;
                    normal += hit.normal;
                    point += hit.point;
                }

                groundNormal = normal.normalized;
                hitPoint = point /= size;
                return true;
            }


            return false;
        }


        private Vector3 GetMovementDirection(Vector3 input)
        {
            return input.y * GetMovementForward() + input.x * GetMovementRight();
        }

        private Vector3 GetMovementForward()
        {
            if (!cameraTransform && transform.childCount > 0)
                cameraTransform = transform.GetChild(0);
            float headRotationX = cameraTransform.eulerAngles.x;
            Vector3 forward = cameraTransform.forward + cameraTransform.up * (headRotationX > 180 ? -1 : 1);
            forward.y = 0;
            return forward.normalized;
        }

        private Vector3 GetMovementRight()
        {
            return cameraTransform.transform.right;
        }

        private float GetCapsulePointDistance()
        {
            float v = collider.height / 2.0f - collider.radius;
            if (v < 0) return 0;
            return v;
        }

        public Vector3 GetVelocity()
        {
            return velocity;
        }

        public Quaternion GetRealRotation()
        {
            return realRotation;
        }

        public Vector3 GetLookPoint(LayerMask mask, float distance)
        {
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, distance, mask))
            {
                Vector3 point = hit.point;
                return point;
            }

            return cameraTransform.position + cameraTransform.forward * distance;
        }


        private void OnDrawGizmos()
        {
            if (!collider)
                collider = GetComponent<CapsuleCollider>();

            float distanceToPoint = GetCapsulePointDistance();

            // ground sphere visualization
            Gizmos.color = Color.green;
            if (IsGrounded(out var groundNormal, out _))
            {
                Gizmos.color = Color.blue;
                Vector3 position = transform.position;
                Gizmos.DrawLine(position + groundSphereCenter,
                    position + groundSphereCenter + groundNormal);
                Gizmos.color = Color.red;
            }


            Gizmos.DrawWireSphere(transform.position + Vector3.down * distanceToPoint + groundSphereCenter,
                groundSphereRadius);

            // ceiling sphere visualization
            Gizmos.color = Color.green;
            if (CanUncrouch())
            {
                Gizmos.color = Color.red;
            }

            Gizmos.DrawWireSphere(transform.position + Vector3.up * distanceToPoint + ceilingSphereCenter,
                ceilingSphereRadius);
        }
    }
}