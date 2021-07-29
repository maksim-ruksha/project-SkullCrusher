using System;
using Preferences;
using SingleInstance;
using UnityEngine;
using Util;
using Weapons;

namespace Character
{
    [RequireComponent(typeof(Controller))]
    public class WeaponController : MonoBehaviour
    {
        public bool needPush = false;

        public Controller controller;

        public float sightPositionFollowAccelerationSpeed = 10.0f;
        public float sightFollowMaxOffset = 0.5f;
        public float sightPositionFollowDecelerationSpeed = 10.0f;
        public float moveImpact = 1.0f;
        public int decPow = 15;
        public float sightPositionMaxDelta = 0.25f;
        public float sightRotationFollowSpeed = 20.0f;

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


        private float currentHideWhileChangingTime;
        private float currentDroppingTime;

        private Vector3 previousTargetPosition;
        private Vector3 currentDelta;
        private Vector3 offset;

        private void Start()
        {
            controller = GetComponent<Controller>();
            GameObject globalController = GameObject.Find(Settings.GameObjects.GlobalController);
            inputManager = globalController.GetComponent<InputManager>();

            weapons = new Weapon[Settings.Config.weaponSlotsCount];
        }

        private void Update()
        {
            WeaponsFireUpdate();
            MainWeaponStateUpdate();
            SecondaryWeaponUpdate();
            SightFollowUpdate(mainWeapon, true);
            SightFollowUpdate(secondaryWeapon, false);
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
        private void SightFollowUpdate(Weapon weapon, bool isMain)
        {
            if (weapon)
            {
                Quaternion r = controller.GetRealRotation();
                
                Vector3 targetPosition = controller.cameraTransform.TransformPoint(weapon.cameraOffsetToRight) - controller.cameraTransform.position;
                Vector3 moveDelta = controller.GetVelocity() / controller.moveSpeed /** inputManager.move.magnitude*/;
                Vector3 delta = targetPosition - previousTargetPosition + moveDelta * moveImpact;
                delta = delta.normalized * Mathf.Min(sightFollowMaxOffset, delta.magnitude);

                // Vector3 delta = inputManager.look * Time.deltaTime;

                Vector3 localDelta = controller.cameraTransform.InverseTransformDirection(delta);
                localDelta = localDelta.normalized * Mathf.Min(sightFollowMaxOffset, localDelta.magnitude);

                currentDelta = Vector3.Lerp(currentDelta, localDelta,
                    Time.deltaTime * sightPositionFollowDecelerationSpeed);
                
                offset = Vector3.Lerp(offset, weapon.cameraOffsetToRight - currentDelta, Time.deltaTime * sightPositionFollowAccelerationSpeed);
                //offset = weapon.cameraOffsetToRight - currentDelta;
                
                Quaternion targetRotation = controller.cameraTransform.rotation;
                weapon.transform.rotation = targetRotation;
                weapon.transform.localPosition = offset;

                previousTargetPosition = targetPosition;

                //weapon.transform.position += controller.GetVelocity() * Time.deltaTime + direction * (Time.deltaTime * sightPositionFollowDecelerationSpeed);
                /*offset = Vector3.Lerp(offset, controller.GetVelocity().normalized,
                    Time.deltaTime * sightPositionFollowAccelerationSpeed) + direction * Time.deltaTime;*/
                //Vector3 rotDelta = position - weapon.transform.position;
                /*offset = Vector3.Lerp(offset, direction.normalized,
                    Time.deltaTime * sightPositionFollowAccelerationSpeed);
                // + direction * Time.deltaTime;
                
                deltaOffset = Vector3.Lerp(deltaOffset, positionDirection,
                    Time.deltaTime * sightPositionFollowDecelerationSpeed);// + offset * Time.deltaTime;
                
                if (deltaOffset.magnitude > sightFollowMaxOffset)
                    deltaOffset = deltaOffset.normalized;
                
                weapon.transform.rotation = targetRotation;
                
                if (direction.magnitude > sightFollowMaxOffset)
                    direction = direction.normalized * sightFollowMaxOffset;
                
                weapon.transform.position = targetPosition - deltaOffset * sightFollowMaxOffset;*/
                /*weapon.transform.position = weapon.transform.position +
                                            positionDirection *
                                            (Time.deltaTime * sightPositionFollowDecelerationSpeed) +
                                            controller.GetVelocity() * (Time.deltaTime *
                                                                        (positionDirection.magnitude));*/

                /*weapon.transform.localPosition = Vector3.Lerp(weapon.cameraOffsetToRight - localDirection,
                    weapon.cameraOffsetToRight, Time.deltaTime * sightPositionFollowAccelerationSpeed);*/
            }
        }

        private bool FixOutOfRangeWeapon(Weapon weapon, Vector3 targetPosition)
        {
            throw new NotImplementedException();
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