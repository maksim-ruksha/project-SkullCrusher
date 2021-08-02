using AI.Classes.Groups;
using AI.Classes.States;
using AI.Enums;
using AI.Scriptables;
using Character;
using Damages;
using Preferences;
using UnityEngine;
using Weapons;

namespace AI
{
    [RequireComponent(typeof(DamageableObject))]
    [RequireComponent(typeof(ViolenceableObject))]
    [RequireComponent(typeof(AiController))]
    public class AiBot: MonoBehaviour
    {
        public AiBotConfig config;
        public Weapon initialWeapon;
        
        public AiBotGroupRole groupRole;
        public AiController controller;
        
        private AiManager aiManager;
        private AiGroup group;
        
        private Transform playerTransform;
        private Controller playerController;
        
        private DamageableObject damageableObject;
        private ViolenceableObject violenceableObject;
        private Weapon equippedWeapon;

        private Vector3 lastPlayerPosition;
        private Vector3 lastPlayerVelocity;
        
        private void Awake()
        {
            controller = GetComponent<AiController>();
            
        }

        private void Start()
        {
            GameObject globalController = GameObject.Find(Settings.GameObjects.GlobalController);

            aiManager = globalController.GetComponent<AiManager>();
            controller = GetComponent<AiController>();
            
            playerController = playerTransform.GetComponent<Controller>();
            if (initialWeapon)
                EquipWeapon(initialWeapon);
            /*
             * group = null
             * groupRole = AiBotGroupRole.Single
             */
        }

        private void Update()
        {
            /*state.Update();
            string nextState = state.TransitionCheck();
            if (!string.IsNullOrWhiteSpace(nextState))
            {
                // TODO: switch state
            }*/
        }

        private void EquipWeapon(Weapon weapon)
        {
            // TODO: implement EquipWeapon()
        }
        
        /*
         * =================================================
         * ================ SHIT FOR STATES ================
         * =================================================
         */
        public void Fire()
        {
            // TODO: implement Fire()
        }

        public void ReloadWeapon()
        {
            // TODO: implement ReloadWeapon()
        }

        public bool IsInGroup()
        {
            return groupRole != AiBotGroupRole.Single;
        }

        public bool IsNeedToStartSeekingCover()
        {
            return (float)equippedWeapon.currentClipAmmoAmount / equippedWeapon.weaponParameters.clipAmmoAmount <= config.predictTakingCoverClipAmmoMultiplier;
        }

        public bool IsNeedToReload()
        {
            return equippedWeapon.currentClipAmmoAmount <= 0;
        }

        public void TrackPlayer()
        {
            lastPlayerPosition = playerTransform.position;
            lastPlayerVelocity = playerController.GetVelocity();
        }

        public Vector3 GetPlayerLastPosition()
        {
            return lastPlayerPosition;
        }

        public Vector3 GetPlayerLastVelocity()
        {
            return lastPlayerVelocity;
        }
        
    }
}