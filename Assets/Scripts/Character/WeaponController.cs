using System;
using Preferences;
using SingleInstance;
using UnityEngine;
using Util;
using Weapons;
using Weapons.Classes;

namespace Character
{
    [RequireComponent(typeof(Controller))]
    public class WeaponController : MonoBehaviour
    {
        public bool useOldSightFollow;
        public bool needPush = false;

        public Controller controller;

        public float sightPositionFollowAccelerationSpeed = 10.0f;
        public float sightFollowMaxOffset = 0.5f;
        public float sightPositionFollowDecelerationSpeed = 10.0f;
        [Range(0, 2)] public float moveImpact = 1.0f;

        public float weaponCastDistance = 1000.0f;
        public LayerMask weaponCastMask;

        // how fast current weapon will go out of screen
        public float weaponHideWhileChangingTime = 0.10f;
        public float secondaryWeaponDroppingTime = 0.25f;

        //private BulletManager bulletManager;
        private InputManager inputManager;

        private Weapon[] weapons;
        private Weapon mainWeapon;
        private Weapon changingToWeapon;
        private Weapon secondaryWeapon;

        // (previousTargetPosition, currentDelta, offset)
        private (Vector3, Vector3, Vector3)[] weaponsSightFollowData;


        private float currentHideWhileChangingTime;
        private float currentDroppingTime;

        private Vector3 previousTargetPosition;
        private Vector3 currentDelta;
        private Vector3 offset;

        private Vector3 lastVelocity;

        private void Start()
        {
            controller = GetComponent<Controller>();
            GameObject globalController = GameObject.Find(Settings.GameObjects.GlobalController);
            inputManager = globalController.GetComponent<InputManager>();

            weapons = new Weapon[Settings.Config.weaponSlotsCount];
            weaponsSightFollowData = new (Vector3, Vector3, Vector3)[Settings.Config.weaponSlotsCount];
        }

        private void Update()
        {
            WeaponsFireUpdate();
            MainWeaponStateUpdate();
            SecondaryWeaponUpdate();
            if(useOldSightFollow)
                SightFollowUpdate(mainWeapon, true);
            else
                SightFollowUpdate1(mainWeapon, true);
            SightFollowUpdate(secondaryWeapon, false);
            lastVelocity = controller.GetVelocity();
        }


        private void MainWeaponStateUpdate()
        {
            if (HasMainWeapon())
            {
                if (mainWeapon.currentClipAmmoAmount <= 0)
                {
                    // reloading, animate
                }

                if (currentHideWhileChangingTime > 0)
                {
                    // hiding to change, animate
                    currentHideWhileChangingTime -= Time.deltaTime;
                    if (currentHideWhileChangingTime <= 0)
                    {
                        // pull out new weapon
                    }
                }
            }
        }

        private void SecondaryWeaponUpdate()
        {
            if (!HasSecondaryWeapon())
                return;
            if (secondaryWeapon.currentClipAmmoAmount <= 0 && currentDroppingTime <= 0)
            {
                secondaryWeapon.selected = false;
                currentDroppingTime = secondaryWeaponDroppingTime;
            }

            if (currentDroppingTime > 0)
            {
                // TODO: animate or something does not exist

                currentDroppingTime -= Time.deltaTime;

                if (currentDroppingTime <= 0)
                {
                    // TODO: drop empty weapon
                    // but for now just destroy it and make null
                    Destroy(secondaryWeapon.gameObject);
                    secondaryWeapon = null;
                }
            }
        }


        private void WeaponsFireUpdate()
        {
            if (inputManager.isFire1Pressed)
            {
                if (mainWeapon)
                {
                    if (TryGetVisionPoint(controller.cameraTransform, out Vector3 point))
                        mainWeapon.FireAtPosition(point);
                    else
                        mainWeapon.Fire();
                }

                if (secondaryWeapon)
                {
                    if (TryGetVisionPoint(controller.cameraTransform, out Vector3 point))
                        secondaryWeapon.FireAtPosition(point);
                    else
                        secondaryWeapon.Fire();
                }
            }
        }

        // some juicy weapon animations
        // пиздец какой-то
        private void SightFollowUpdate(Weapon weapon, bool isMain)
        {
            if (!weapon)
                return;

            int id = weapon.playerSlot;
            (Vector3, Vector3, Vector3) sightFollowInfo = weaponsSightFollowData[id];
            
            Vector3 weaponPreviousTargetPosition = sightFollowInfo.Item1;
            Vector3 weaponCurrentDelta = sightFollowInfo.Item2;
            Vector3 weaponOffset = sightFollowInfo.Item3;
            
            SightFollowParameters sightFollowParameters = weapon.sightFollowParameters;

            Vector3 targetPosition = weapon.cameraOffsetToRight;
            if (!isMain)
            {
                Vector3 cameraOffset = weapon.cameraOffsetToRight;
                cameraOffset.x *= -1;
                
                targetPosition = controller.cameraTransform.TransformPoint(cameraOffset) -
                                 controller.cameraTransform.position;
            }

            Vector3 moveDelta = lastVelocity / controller.moveSpeed;
            Vector3 localMoveDelta = controller.cameraTransform.InverseTransformDirection(moveDelta);
            Vector3 offset = targetPosition - sightFollowInfo.Item1 + localMoveDelta * sightFollowParameters.moveImpact;
            offset = offset.normalized * Mathf.Min(sightFollowParameters.sightFollowMaxOffset, offset.magnitude);


            weaponCurrentDelta = Vector3.Lerp(sightFollowInfo.Item2, offset,
                Time.deltaTime * sightFollowParameters.sightPositionFollowDecelerationSpeed);

            weaponOffset = Vector3.Lerp(sightFollowInfo.Item3, weapon.cameraOffsetToRight - sightFollowInfo.Item2,
                Time.deltaTime * sightFollowParameters.sightPositionFollowAccelerationSpeed);

            Quaternion targetRotation = controller.cameraTransform.rotation;
            weapon.transform.rotation = targetRotation;
            weapon.transform.localPosition = sightFollowInfo.Item3;

            weaponPreviousTargetPosition = targetPosition;

            sightFollowInfo.Item1 = weaponPreviousTargetPosition;
            sightFollowInfo.Item2 = weaponCurrentDelta;
            sightFollowInfo.Item3 = weaponOffset;

            weaponsSightFollowData[id] = sightFollowInfo;
            
            //previousTargetPosition = targetPosition;
        }
        
        private void SightFollowUpdate1(Weapon weapon, bool isMain)
        {
            if (weapon)
            {
                
                Vector3 targetPosition = controller.cameraTransform.TransformPoint(weapon.cameraOffsetToRight) - controller.cameraTransform.position;
                Vector3 moveDelta = controller.GetVelocity() / controller.moveSpeed;
                
                Vector3 delta = targetPosition - previousTargetPosition + moveDelta * moveImpact;
                delta = delta.normalized * Mathf.Min(sightFollowMaxOffset, delta.magnitude);


                Vector3 localDelta = controller.cameraTransform.InverseTransformDirection(delta);
                localDelta = localDelta.normalized * Mathf.Min(sightFollowMaxOffset, localDelta.magnitude);

                currentDelta = Vector3.Lerp(currentDelta, localDelta,
                    Time.deltaTime * sightPositionFollowDecelerationSpeed);
                
                offset = Vector3.Lerp(offset, weapon.cameraOffsetToRight - currentDelta, Time.deltaTime * sightPositionFollowAccelerationSpeed);
                
                
                Quaternion targetRotation = controller.cameraTransform.rotation;
                weapon.transform.rotation = targetRotation;
                weapon.transform.localPosition = offset;

                previousTargetPosition = targetPosition;
            }
        }




        private bool TryGetVisionPoint(Transform head, out Vector3 visionPoint)
        {
            Ray ray = new Ray(head.position, head.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, weaponCastDistance, weaponCastMask))
            {
                visionPoint = hit.point;
                return true;
            }

            visionPoint = new Vector3();
            return false;
        }


        public void ChangeWeapon(int slot)
        {
            if (slot < 0 || slot > 9)
                throw new Exception($"Invalid slot: {slot}");
            if (slot == mainWeapon.playerSlot)
                return;
            if (currentHideWhileChangingTime <= 0)
                currentHideWhileChangingTime = weaponHideWhileChangingTime;
            mainWeapon.selected = false;
        }

        public Weapon GetMainWeapon()
        {
            return mainWeapon;
        }

        public Weapon GetSecondaryWeapon()
        {
            return secondaryWeapon;
        }


        private void PickupWeapon(Weapon weapon)
        {
            if (IsThisMine(weapon))
                return;

            if (!HasWeaponAtSlot(weapon.playerSlot))
            {
                // picking up, can start animation here
                Transform weaponTransform = weapon.transform;
                weaponTransform.parent = controller.cameraTransform;
                weaponTransform.forward = controller.cameraTransform.forward;

                weaponTransform.localPosition = weapon.cameraOffsetToRight;

                weapons[weapon.playerSlot] = weapon;

                mainWeapon = weapon;
                mainWeapon.owner = transform;
                mainWeapon.selected = true;

                weaponTransform.GetComponent<Rigidbody>().isKinematic = true;
            }
            else
            {
                if (!HasSecondaryWeapon())
                {
                    Transform weaponTransform = weapon.transform;

                    weaponTransform.parent = controller.cameraTransform;
                    weaponTransform.forward = controller.cameraTransform.forward;

                    Vector3 offset = weapon.cameraOffsetToRight;
                    offset.x *= -1;
                    weaponTransform.localPosition = offset;

                    secondaryWeapon = weapon;
                    secondaryWeapon.owner = transform;
                    secondaryWeapon.selected = true;

                    weaponTransform.GetComponent<Rigidbody>().isKinematic = true;
                }
                else
                {
                    // has main and secondary weapons, just add ammo
                    if (weapons[weapon.playerSlot])
                    {
                        mainWeapon.currentRemainedAmmoAmount +=
                            weapon.ammoAmountIfPlayerAlreadyHasThisWeaponAndTriesToPickupIt;
                        Destroy(weapon.transform);
                    }
                }
            }
        }

        private void PickupAmmo(Ammo ammo)
        {
            if (HasWeaponAtSlot(ammo.weaponPlayerSlot))
            {
                weapons[ammo.weaponPlayerSlot].currentRemainedAmmoAmount += ammo.ammoAmount;
                // for now just destroy the object
                Destroy(ammo.gameObject);
            }
        }

        private bool HasMainWeapon()
        {
            return mainWeapon != null;
        }

        private bool HasWeaponAtSlot(int slot)
        {
            return weapons[slot] != null;
        }

        private bool HasSecondaryWeapon()
        {
            return secondaryWeapon != null;
        }

        // for unknown reason OnTriggerEnter occurs two time per entrance
        // that is why this stuff exists
        private bool IsThisMine(Weapon weapon)
        {
            if (weapon == null)
                return false;

            for (int i = 0; i < Settings.Config.weaponSlotsCount; i++)
            {
                if (weapons[i] != null && weapons[i].Equals(weapon))
                    return true;
            }

            return secondaryWeapon != null && secondaryWeapon.Equals(weapon);
        }


        public void OnTriggerEnter(Collider other)
        {
            Weapon weaponComponent = other.gameObject.GetComponent<Weapon>();
            if (weaponComponent)
            {
                PickupWeapon(weaponComponent);
                return;
            }

            Ammo ammoComponent = other.gameObject.GetComponent<Ammo>();
            if (ammoComponent)
            {
                PickupAmmo(ammoComponent);
                //return;
            }
        }
    }
}