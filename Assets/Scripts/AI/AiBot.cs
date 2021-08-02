using AI.Classes.Groups;
using AI.Classes.States;
using AI.Enums;
using AI.Scriptables;
using Character;
using Damages;
using Level.Covers;
using Preferences;
using UnityEngine;
using Violence;
using Weapons;

namespace AI
{
    [RequireComponent(typeof(DamageableObject))]
    [RequireComponent(typeof(ViolenceableObject))]
    [RequireComponent(typeof(AiController))]
    public class AiBot : MonoBehaviour
    {
        public AiHumanBotConfig config;
        public Weapon initialWeapon;

        public AiBotGroupRole groupRole;
        public AiController controller;

        //private AiManager aiManager;
        private AiGroup group;

        // public for states
        [HideInInspector] public Transform playerTransform;
        [HideInInspector] public Controller playerController;
        [HideInInspector] public StateManager stateManager;
        [HideInInspector] public CoverManager coverManager;
        
        private DamageableObject damageableObject;
        private ViolenceableObject violenceableObject;
        
        private Weapon equippedWeapon;

        private Vector3 lastPlayerPosition;
        private Vector3 lastPlayerVelocity;


        private void Start()
        {
            GameObject globalController = GameObject.Find(Settings.GameObjects.GlobalController);
            coverManager = globalController.GetComponent<CoverManager>();
            //aiManager = globalController.GetComponent<AiManager>();

            // TODO: take damage
            damageableObject = GetComponent<DamageableObject>();
            violenceableObject = GetComponent<ViolenceableObject>();

            controller = GetComponent<AiController>();

            playerController = playerTransform.GetComponent<Controller>();

            stateManager = new StateManager();

            if (initialWeapon)
                EquipWeapon(initialWeapon);
            /*
             * group = null
             * groupRole = AiBotGroupRole.Single
             */
        }

        private void Update()
        {
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

        public float GetRangedHealth()
        {
            return damageableObject.GetRangedHealth();
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
            return (float) equippedWeapon.currentClipAmmoAmount / equippedWeapon.weaponParameters.clipAmmoAmount <=
                   config.predictTakingCoverClipAmmoMultiplier;
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