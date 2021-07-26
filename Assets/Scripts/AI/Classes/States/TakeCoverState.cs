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
        private bool startedReload;

        public TakeCoverState(AiStateConfig config, AiBot bot, Transform player): base(config, bot, player)
        {
            name = TakeCoverState;
            stateConfig = (TakeCoverStateConfig) config;
            
            GameObject globalController = GameObject.Find(Settings.GameObjects.GlobalController);
            coverManager = globalController.GetComponent<CoverManager>();
        }

        public override void Transit(AiStateConfig newConfig)
        {
            stateConfig = (TakeCoverStateConfig) config;
            takingCoverForReloading = bot.IsNeedToStartSeekingCover();
            
            Vector3 playerDirection = (player.position - bot.transform.position).normalized;
            Vector3 coverPosition = coverManager.GetNearestCoverPosition(bot.transform.position, playerDirection,
                stateConfig.angleThreshold);
            bot.controller.GoTo(coverPosition);

        }

        public override void Update()
        {

            if (bot.controller.IsArrivedAtTargetPosition())
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

        public override string TransitionCheck()
        {
            if (bot.controller.IsArrivedAtTargetPosition() && startedReload && !bot.IsNeedToReload())
            {
                return AttackState;
            }

            return KeepCurrentState;
        }
    }
}