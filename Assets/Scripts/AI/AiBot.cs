using System.Collections.Generic;
using AI.Classes.Groups;
using AI.Classes.States;
using AI.Classes.States.Configs;
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


        //private AiManager aiManager;
        public AiGroupRole groupRole = AiGroupRole.Single;
        private AiGroup group;

        // public for states
        [HideInInspector] public AiController controller;
        [HideInInspector] public Transform playerTransform;
        [HideInInspector] public Controller playerController;
        public StateManager stateManager;
        [HideInInspector] public CoverManager coverManager;

        private DamageableObject damageableObject;
        private ViolenceableObject violenceableObject;

        private Weapon equippedWeapon;

        private Vector3 lastPlayerPosition;
        private Vector3 lastPlayerVelocity;

        private float currentTrackUpdateTime;


        private void Start()
        {
            GameObject globalController = GameObject.Find(Settings.GameObjects.GlobalController);
            coverManager = globalController.GetComponent<CoverManager>();

            damageableObject = GetComponent<DamageableObject>();
            violenceableObject = GetComponent<ViolenceableObject>();

            controller = GetComponent<AiController>();
            //controller

            playerTransform = GameObject.Find(Settings.GameObjects.Player).transform;
            playerController = playerTransform.GetComponent<Controller>();

            stateManager = new StateManager();
            InitializeStates();
            if (initialWeapon)
                EquipWeapon(initialWeapon);
            
            // idk why, but bots rotate their heads at start
            // so we need this
            controller.LookAtDirection(controller.headTransform.forward);
        }


        private void Update()
        {
            // group tracking
            if (currentTrackUpdateTime > 0)
            {
                currentTrackUpdateTime -= Time.deltaTime;
                if (currentTrackUpdateTime <= 0)
                {
                    // track player
                    lastPlayerPosition = playerTransform.position;
                    lastPlayerVelocity = playerController.GetVelocity();
                }
            }

            AiStateConfig[] configs = config.GetStatesConfigs();
            for (int i = 0; i < configs.Length; i++)
            {
                stateManager.UpdateConfig(i + 1, configs[i]);
            }

            stateManager.Update();
        }

        private void InitializeStates()
        {
            AiStateConfig[] configs = config.GetStatesConfigs();
            List<AiState> states = StatePack.HumanStatePack(config, this);
            for (int i = 0; i < states.Count; i++)
            {
                stateManager.RegisterState(states[i], configs[i]);
            }
        }

        private void EquipWeapon(Weapon weapon)
        {
            // TODO: implement EquipWeapon()
        }

        public bool IsRoleAssignable(AiGroupRole role)
        {
            bool[] assignable =
            {
                true, // single
                config.canBeCommon,
                config.canBeTank,
                config.canBeTankSupporter,
                config.canBeRearAttacker,
                config.canBeMedic
            };
            return assignable[(int) role];
        }

        public void OnGroupPlayerTracked(float calculatedTime)
        {
            currentTrackUpdateTime = calculatedTime;
        }


        public void SetAiGroup(AiGroup group, AiGroupRole role = AiGroupRole.Common)
        {
            this.group = group;
            if (group == null)
                groupRole = AiGroupRole.Single;
            else
                groupRole = role;
        }

        public AiGroup GetAiGroup()
        {
            return group;
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
            return group != null;
        }

        public bool IsNeedToStartSeekingCover()
        {
            if (!equippedWeapon)
                return true;
            return (float) equippedWeapon.currentClipAmmoAmount / equippedWeapon.weaponParameters.clipAmmoAmount <=
                   config.predictTakingCoverClipAmmoMultiplier;
        }

        public bool IsNeedToReload()
        {
            return equippedWeapon && equippedWeapon.currentClipAmmoAmount <= 0;
        }

        public void TrackPlayer()
        {
            lastPlayerPosition = playerTransform.position;
            lastPlayerVelocity = playerController.GetVelocity();
            if (IsInGroup())
            {
                group.TrackPlayer(this);
            }
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