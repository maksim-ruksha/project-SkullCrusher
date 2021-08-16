using Preferences;
using SingleInstance;
using UnityEngine;
using Util;
using Weapons.Classes;
using Weapons.Classes.Parameters;
using Weapons.Enums;
using Weapons.Parameters;

namespace Weapons
{
    public class Weapon : MonoBehaviour
    {
        // invert x for left
        public Vector3 cameraOffsetToRight = new Vector3(0.35f, -0.47f, 0.60f);

        [Range(0, 9)] public int playerSlot = 1;

        public Transform owner;
        public bool selected;

        // дядя Боб, блять, не бей
        // макс иди нахуй, отличное название переменной
        public int ammoAmountIfPlayerAlreadyHasThisWeaponAndTriesToPickupIt;

        public WeaponParameters weaponParameters;
        public SightFollowParameters sightFollowParameters;

        public Transform bullet;
        public BulletParameters bulletParameters;

        public Transform shutter;
        public ShutterParameters shutterParameters;

        public Transform trigger;
        public TriggerParameters triggerParameters;

        public Transform ejector;
        public EjectorParameters ejectorParameters;

        [Header("Positions")] public Transform muzzle;

        public Transform shell;
        public Transform clip;

        public int currentClipAmmoAmount;
        public int currentRemainedAmmoAmount; // does not include currentClipAmmoAmount

        // global
        private BulletManager bulletManager;

        private float timePerShot;
        private float currentTimePerShot;
        private float currentReloadTime;
        
        // shutter
        private Vector3 shutterInitialPosition;
        private ShutterState currentShutterState;
        private float currentShutterTime;

        private void Start()
        {
            // looks shitty, but ok for now
            if (!muzzle)
                UnityEngine.Debug.LogError("Muzzle is not assigned", this);
            if (!bullet)
                UnityEngine.Debug.LogError("Bullet is not assigned", this);

            if (!shell)
                UnityEngine.Debug.LogWarning("Shell is not assigned", this);
            if (!ejector)
                UnityEngine.Debug.LogWarning("Ejector is not assigned", this);
            if (!clip)
                UnityEngine.Debug.LogWarning("Clip is not assigned", this);
            if (!trigger)
                UnityEngine.Debug.LogWarning("Trigger is not assigned", this);
            if (!shutter)
                UnityEngine.Debug.LogWarning("Shutter is not assigned", this);

            GameObject globalController = GameObject.Find(Settings.GameObjects.GlobalController);
            bulletManager = globalController.GetComponent<BulletManager>();

            currentRemainedAmmoAmount = weaponParameters.totalAmmoAmount;
            int clipAmmo = GetAmmoAmountForReload();
            currentClipAmmoAmount = clipAmmo;
            currentRemainedAmmoAmount -= clipAmmo;

            shutterInitialPosition = shutter.localPosition;
        }

        private void Update()
        {
            UpdateReloading();
            UpdateShooting();
            UpdateShutter();
            UpdateTrigger();
        }

        public void Fire()
        {
            if (!CanFire())
            {
                // play sound, but not now
                return;
            }


            Transform bulletTransform = Instantiate(bullet, muzzle.position, muzzle.rotation);
            bulletManager.AddBullet(bulletTransform, bulletParameters, currentClipAmmoAmount, 1.0f);

            currentClipAmmoAmount--;
            currentTimePerShot = timePerShot;
            currentShutterState = ShutterState.Sliding;
            currentShutterTime = shutterParameters.slideTime;
        }

        public void FireAtPosition(Vector3 position)
        {
            if (!CanFire())
            {
                // play sound, but not now
                return;
            }
            Vector3 delta = (position - muzzle.position).normalized;
            Transform bulletTransform = Instantiate(bullet, muzzle.position, Quaternion.LookRotation(delta));
            bulletManager.AddBullet(bulletTransform, bulletParameters, currentClipAmmoAmount, 1.0f);
            
            currentClipAmmoAmount--;
            currentTimePerShot = timePerShot;
            currentShutterState = ShutterState.Sliding;
            currentShutterTime = shutterParameters.slideTime;
        }


        public void Reload()
        {
            int availableAmmo = GetAmmoAmountForReload();

            if (availableAmmo > 0)
            {
                currentReloadTime = weaponParameters.reloadTime;
            }
        }

        private bool CanFire()
        {
            return currentClipAmmoAmount > 0 && currentTimePerShot <= 0;
        }


        private void UpdateShooting()
        {
            if (!selected)
                return;
            timePerShot = 1 / weaponParameters.shootRate;

            if (currentTimePerShot > 0)
                currentTimePerShot -= Time.deltaTime;
            if (currentReloadTime > 0)
                currentReloadTime -= Time.deltaTime;
        }

        private void UpdateReloading()
        {
            if (selected)
            {
                int ammoForReload = GetAmmoAmountForReload();
                if (currentReloadTime <= 0 && currentClipAmmoAmount <= 0 && ammoForReload > 0)
                {
                    Reload();
                }

                if (currentReloadTime > 0)
                {
                    currentReloadTime -= Time.deltaTime;
                    if (currentReloadTime <= 0)
                    {
                        currentClipAmmoAmount += ammoForReload;
                        currentRemainedAmmoAmount -= ammoForReload;
                    }
                }
            }
            else
            {
                if (currentReloadTime > 0)
                    currentReloadTime = weaponParameters.reloadTime;
            }
        }

        private void UpdateShutter()
        {
            
            if (!shutter || currentShutterTime <= float.Epsilon)
                return;
            
            currentShutterTime -= Time.deltaTime;
            
            switch (currentShutterState)
            {
                case ShutterState.Idle:
                {
                    // do nothing
                    break;
                }
                    
                case ShutterState.Sliding:
                {
                    float t = (shutterParameters.slideTime - currentShutterTime) / shutterParameters.slideTime;
                    if (t > 1)
                    {
                        t = 1;
                        currentShutterState = ShutterState.Delay;
                        currentShutterTime = shutterParameters.delayAtSlidedPosition;
                        SpawnShell();
                        /*
                         * TODO: spawn shell
                         */
                    }
                    
                    Vector3 farPosition = shutterInitialPosition + Vector3.back * shutterParameters.slideDistance;
                    Vector3 position = Vector3.Lerp(shutterInitialPosition, farPosition, t);
                    shutter.localPosition = position;
                    
                        
                    break;
                }
                case ShutterState.Delay:
                {
                    // waiting
                    if (currentShutterTime <= 0)
                    {
                        currentShutterTime = shutterParameters.slideBackTime;
                        currentShutterState = ShutterState.SlidingBack;
                        
                    }

                    break;
                }
                case ShutterState.SlidingBack:
                {
                    //same as sliding, but different values
                    float t = (shutterParameters.slideBackTime - currentShutterTime) / shutterParameters.slideBackTime;
                    if (t > 1)
                    {
                        t = 1;
                        currentShutterState = ShutterState.Idle;
                        currentShutterTime = shutterParameters.delayAtSlidedPosition;
                    }
                    
                    Vector3 farPosition = shutterInitialPosition + Vector3.back * shutterParameters.slideDistance;
                    Vector3 position = Vector3.Lerp(farPosition, shutterInitialPosition, t);
                    shutter.localPosition = position;
                    
                    break;
                }
                    
            }
        }

        private void SpawnShell()
        {
            Vector3 velocityDirection = ejector.forward;
            float velocityMultiplier =
                Random.value * (ejectorParameters.shellMaximumVelocity - ejectorParameters.shellMinimumVelocity) +
                ejectorParameters.shellMinimumVelocity;
            float angularVelocityMultiplier  = Random.value * (ejectorParameters.shellMaximumAngularVelocity - ejectorParameters.shellMinimumAngularVelocity) +
                                              ejectorParameters.shellMinimumAngularVelocity;

            Transform shellTransform = Instantiate(shell, ejector.position, Quaternion.LookRotation(ejector.right));
            Rigidbody shellRigidbody = shellTransform.GetComponent<Rigidbody>();
            shellRigidbody.velocity = velocityDirection * velocityMultiplier;
            // TODO: research angularVelocity
            shellRigidbody.angularVelocity = Random.insideUnitSphere * angularVelocityMultiplier;
        }

        private void UpdateTrigger()
        {
        }


        private int GetAmmoAmountForReload()
        {
            int need = weaponParameters.clipAmmoAmount - currentClipAmmoAmount;
            int available = Mathf.Min(currentRemainedAmmoAmount, weaponParameters.clipAmmoAmount);
            return Mathf.Min(available, need);
        }


        private void OnDrawGizmos()
        {
            const int gizmoShellEjectionTrajectorySamples = 20;

            // draw muzzle position and try to raycast
            // bots will fairly aim weapon to player
            if (muzzle)
            {
                Gizmos.color = Color.red;
                Vector3 position = muzzle.position;
                //Gizmos.DrawSphere(position, 0.03125f);
                Ray ray = new Ray(muzzle.position, muzzle.forward);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    Gizmos.DrawLine(position, hit.point);
                    Gizmos.DrawSphere(hit.point, 0.0625f);
                    if (hit.collider.gameObject.tag.Equals(Settings.Tags.Ricochetable))
                    {
                        Vector3 ricochetedDirection = Quaternion.AngleAxis(180, hit.normal) * muzzle.forward * -1;
                        Gizmos.DrawLine(hit.point, hit.point + ricochetedDirection);
                    }
                }
                else
                {
                    Gizmos.DrawLine(position, position + muzzle.forward);
                }
            }

            // draw ejector position and direction
            if (ejector)
            {
                for (int i = 0; i < gizmoShellEjectionTrajectorySamples; i++)
                {
                    float t = (float) (i + 1) / gizmoShellEjectionTrajectorySamples;
                    float velocity = Mathf.Lerp(ejectorParameters.shellMinimumVelocity,
                        ejectorParameters.shellMaximumVelocity, t);
                    Color color = new Color(1 - t, t, 0).Normalize();
                    Gizmos.color = color;
                    DrawTrajectoryGizmo(ejector.position, ejector.forward, velocity);
                }
            }

            if (shutter)
            {
                Gizmos.DrawMesh(shutter.GetComponent<MeshFilter>().sharedMesh,
                    shutter.position + shutter.forward * -1 * shutterParameters.slideDistance, shutter.rotation);
            }
        }

        private void DrawTrajectoryGizmo(Vector3 point, Vector3 direction, float velocityMultiplier, int samples = 50)
        {
            Vector3 position = ejector.position;
            Vector3 velocity = direction * velocityMultiplier;

            float samplesInverted = 1.0f / samples;

            while (position.y + 1.0f > point.y)
            {
                Vector3 nextPosition = position + velocity * samplesInverted;
                Gizmos.DrawLine(position, nextPosition);
                velocity += Physics.gravity * samplesInverted;
                position = nextPosition;
            }
        }
    }
}