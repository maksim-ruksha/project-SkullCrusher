using System.Collections.Generic;
using Preferences;
using UnityEngine;
using UnityEngine.AI;

namespace AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class AiController : MonoBehaviour
    {
        /*[Header("Character")] public float characterSpeed = 1.0f;

        public Transform headTransform;
        public float headRotationSpeed = 1.0f;

        public Transform bodyTransform;
        public float bodyRotationSpeed = 2.0f;

        public float characterHeight = 2.0f;*/
        
        [Header("Character")]
        public Transform headTransform;
        public Transform bodyTransform;
        
        [Header("Misc")] public float
            maxNeckAngleDelta =
                72.0f; // google didn't help, there is no information about human's max neck rotation angle 

        public float sampleRadius = 15.0f;

        private NavMeshAgent navMeshAgent;
        private Transform playerTransform;

        private Vector3 targetMovePosition;
        private Vector3 targetLookPosition;

        private List<MeshRenderer> playerParts;
        private float perPartPointMultiplier;

        private float bodyRotationSpeed;
        private float headRotationSpeed;
        private float characterHeight;
        
        private float visionDistance;
        private float visionAngle;

        private LayerMask visibleObjectsMask;


        private void Start()
        {
            AiBot aiBot = GetComponent<AiBot>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshAgent.speed = aiBot.config.characterSpeed;

            bodyRotationSpeed = aiBot.config.bodyRotationSpeed;
            headRotationSpeed = aiBot.config.headRotationSpeed;
            characterHeight = aiBot.config.characterHeight;

            visionDistance = aiBot.config.visionDistance;
            visionAngle = aiBot.config.visionAngle;

            visibleObjectsMask = aiBot.config.visibleObjectsMask;
            

            playerTransform = GameObject.FindGameObjectWithTag(Settings.Tags.Player).transform;

            playerParts = new List<MeshRenderer>();
            GetPlayerParts(playerTransform);
            perPartPointMultiplier = 1.0f / (playerParts.Count * 8);
        }

        private void Update()
        {
            LookUpdate();
            if (IsArrivedAtTargetPosition())
            {
                /*onArrivedAtTargetPosition.Invoke();*/
            }
        }


        private void LookUpdate()
        {
            float neckDelta = GetNeckDelta();
            Vector3 targetLookDirection = targetLookPosition - headTransform.position;
            if (neckDelta > maxNeckAngleDelta)
            {
                // neck is about to break
                // need to rotate body
                Vector3 horizontalTargetLookDirection = targetLookDirection;
                horizontalTargetLookDirection.y = 0;
                bodyTransform.rotation = Quaternion.Lerp(bodyTransform.rotation,
                    Quaternion.LookRotation(horizontalTargetLookDirection), bodyRotationSpeed * Time.deltaTime);
            }

            headTransform.forward =
                Vector3.Lerp(headTransform.forward, targetLookDirection, headRotationSpeed * Time.deltaTime);
        }

        private float GetNeckDelta()
        {
            Vector3 horizontalHeadForward = headTransform.forward;
            horizontalHeadForward.y = 0;
            Vector3 horizontalBodyForward = bodyTransform.forward;
            horizontalBodyForward.y = 0;

            return Vector3.Angle(horizontalHeadForward, horizontalBodyForward);
        }


        public bool IsArrivedAtTargetPosition()
        {
            return (transform.position - targetMovePosition).magnitude < characterHeight;
        }

        public Vector3 Sample(Vector3 position)
        {
            if (NavMesh.SamplePosition(position, out var hit, sampleRadius, -1))
            {
                if ((hit.position - position).magnitude > characterHeight)
                {
                    return hit.position;
                }
            }

            return position;
        }

        public Vector3 GetTargetMovePosition()
        {
            return targetMovePosition;
        }
        
        public Vector3 GetTargetLookPosition()
        {
            return targetLookPosition;
        }

        public bool IsLookingAtTarget()
        {
            return (transform.forward - (targetLookPosition - transform.position)).sqrMagnitude < 0.01f;
        }

        public void StopMoving()
        {
            GoTo(transform.position);
        }


        public Vector3 GetMovingDirection()
        {
            return navMeshAgent.velocity.normalized;
        }

        // returns possibility to move right into point
        public bool GoTo(Vector3 position)
        {
            if (NavMesh.SamplePosition(position, out var hit, sampleRadius, -1))
            {
                if ((hit.position - position).magnitude > characterHeight)
                {
                    navMeshAgent.destination = hit.position;
                    targetMovePosition = hit.position;
                    return false;
                }
            }

            navMeshAgent.destination = position;
            targetMovePosition = position;

            return true;
        }


        public void LookAt(Vector3 position)
        {
            //rotationMode = RotationMode.HeadRules;
            targetLookPosition = position;
        }

        public void LookAtDirection(Vector3 direction)
        {
            //rotationMode = RotationMode.HeadRules;
            targetLookPosition = headTransform.position + direction;
        }

        public float GetPlayerVisibility()
        {
            float result = 0.0f;
            for (int i = 0; i < playerParts.Count; i++)
            {
                Vector3[] castPoints = GetBoundsPoints(playerParts[i].bounds);
                for (int j = 0; j < castPoints.Length; j++)
                {
                    float visibility = GetVisibilityOfPoint(castPoints[j]);
                    result += visibility * perPartPointMultiplier;
                }
            }

            return result;
        }

        public float GetPlayerVisibilityDistanceMultiplier()
        {
            Vector3 playerDelta = playerTransform.position - transform.position;
            return (visionDistance - playerDelta.magnitude) / visionDistance;
        }


        public float GetPotentialVisibilityOfPoint(Vector3 point)
        {
            Vector3 pointDelta = (point - headTransform.position);
            Ray ray = new Ray(headTransform.position, pointDelta);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, visionDistance, visibleObjectsMask))
            {
                float hitDistance = (hit.point - headTransform.position).magnitude;
                float pointDistance = pointDelta.magnitude;
                if (hitDistance > pointDistance)
                    return 1;
            }

            return 0;
        }
        public float GetVisibilityOfPoint(Vector3 point)
        {
            Vector3 pointDelta = (point - headTransform.position);
            float angle = Vector3.Angle(pointDelta.normalized, headTransform.forward);
            //float vision = angle <= visionAngle ? 1 : 0;

            if (angle > visionAngle) return -1;

            Ray ray = new Ray(headTransform.position, pointDelta);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, visionDistance, visibleObjectsMask))
            {
                float hitDistance = (hit.point - headTransform.position).magnitude;
                float pointDistance = pointDelta.magnitude;
                if (hitDistance > pointDistance)
                    return 1;
            }

            return 0;
        }


        private Vector3[] GetBoundsPoints(Bounds bounds)
        {
            return new[]
            {
                bounds.max,
                bounds.min,

                new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
                new Vector3(bounds.min.x, bounds.max.y, bounds.max.z),

                new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
                new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
                new Vector3(bounds.min.x, bounds.max.y, bounds.min.z)
            };
        }

        private void GetPlayerParts(Transform currentPlayerTransform)
        {
            MeshRenderer meshRenderer = currentPlayerTransform.GetComponent<MeshRenderer>();
            if (meshRenderer)
                playerParts.Add(meshRenderer);

            for (int i = 0; i < currentPlayerTransform.childCount; i++)
            {
                GetPlayerParts(currentPlayerTransform.GetChild(i));
            }
        }


        private void OnDrawGizmos()
        {
            if (playerParts == null)
                return;
            if (!navMeshAgent)
                return;


            /*float playerVisibility = GetPlayerVisibility();
            Color visibilityColor = new Color(playerVisibility, 1 - playerVisibility, 0).Normalize();
            Gizmos.DrawSphere(transform.position);*/

            Gizmos.color = Color.blue;
            Vector3 position = transform.position;
            Gizmos.DrawSphere(position, 0.125f);
            foreach (var t in navMeshAgent.path.corners)
            {
                Gizmos.DrawLine(position, t);
                position = t;
            }
        }
    }
}