using System;
using AI.Classes.States.Configs;
using Level.Covers;
using Preferences;
using UnityEngine;

namespace AI.Classes.States
{
    public class TakeCoverState : AiState
    {
        private TakeCoverStateConfig stateConfig;

        private ContactStateConfig contactStateConfig;

        private CoverManager coverManager;
        private bool takingCoverForReloading;
        private bool pickedCover;
        private bool startedReload;

        public TakeCoverState(AiStateConfig config, AiBot bot, Transform player, StateManager manager) : base(config, bot, player, manager)
        {
            name = "TakeCoverState";
            stateConfig = (TakeCoverStateConfig) config;
            GameObject globalController = GameObject.Find(Settings.GameObjects.GlobalController);
            coverManager = globalController.GetComponent<CoverManager>();
            takingCoverForReloading = bot.IsNeedToStartSeekingCover();
        }

        public override void Update()
        {
            if (!pickedCover)
            {
                Vector3 playerDirection = (player.position - bot.transform.position).normalized;
                Vector3 coverPosition = coverManager.GetNearestCoverPosition(bot.transform.position, playerDirection,
                    stateConfig.angleThreshold);
                bot.controller.GoTo(coverPosition);
                pickedCover = true;
            }

            if (pickedCover && bot.controller.IsArrivedAtTargetPosition())
            {
                if (takingCoverForReloading)
                {
                    if (bot.IsNeedToReload())
                    {
                        bot.ReloadWeapon();
                        startedReload = true;
                    }
                    else
                    {
                        bot.Fire();
                    }
                }
            }
        }

        public override int TransitionCheck()
        {
            if (pickedCover && bot.controller.IsArrivedAtTargetPosition() && startedReload && !bot.IsNeedToReload())
            {
                return manager.GetStateIdByName("AttackState");
            }

            return manager.GetStateIdByName("KeepCurrentState");
        }
    }
}