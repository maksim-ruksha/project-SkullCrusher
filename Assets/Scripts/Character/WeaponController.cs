using System;
using Preferences;
using SingleInstance;
using UnityEngine;
using Weapons;

namespace Character
{
    [RequireComponent(typeof(Controller))]
    public class WeaponController : MonoBehaviour
    {
        public Controller controller;


        public float sightPositionFollowSpeed = 10.0f;
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
            
            
        }

        private void LateUpdate()
        {
            // sits here because in Update causes twitching
            /*
            SightFollowUpdate(mainWeapon, true);
            SightFollowUpdate(secondaryWeapon, false);
            */
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

        private void SightFollowUpdate(Weapon weapon, bool isMain)
        {
            if (weapon)
            {
                Vector3 targetPosition = controller.cameraTransform.position
                                         + controller.cameraTransform.forward * weapon.cameraOffsetToRight.z
                                         + controller.cameraTransform.right * weapon.cameraOffsetToRight.x
                                         + controller.cameraTransform.up * weapon.cameraOffsetToRight.y;

                Quaternion targetRotation = controller.cameraTransform.rotation;

                /*weapon.transform.position = Vector3.Slerp(weapon.transform.position, targetPosition,
                    Time.deltaTime * sightPositionFollowSpeed);*/
                //weapon.transform.position = targetPosition;
                //weapon.transform.rotation = controller.cameraTransform.rotation;
                weapon.transform.rotation = Quaternion.Slerp(weapon.transform.rotation, targetRotation,
                    Time.deltaTime * sightRotationFollowSpeed);
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