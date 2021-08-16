using AI.Classes.States.Configs;
using AI.Classes.States.Configs.Human;
using Level.Covers;
using Preferences;
using UnityEngine;

namespace AI.Classes.States.Human
{
    public class HumanTakeCoverState : AiState
    {
        private HumanTakeCoverStateConfig stateConfig;

        private HumanContactStateConfig humanContactStateConfig;

        private CoverManager coverManager;
        private bool takingCoverForReloading;
        private bool startedReload;

        private float currentCoverTime = 0.0f;

        public HumanTakeCoverState(AiStateConfig config, AiBot bot) : base(config, bot)
        {
            name = "TakeCoverState";
            stateConfig = (HumanTakeCoverStateConfig) config;
            GameObject globalController = GameObject.Find(Settings.GameObjects.GlobalController);
            coverManager = globalController.GetComponent<CoverManager>();
            takingCoverForReloading = bot.IsNeedToStartSeekingCover();
        }

        public override void Transit(AiStateConfig newConfig)
        {
            stateConfig = (HumanTakeCoverStateConfig) newConfig;
        }

        public override void Update()
        {
            if (currentCoverTime <= 0)
            {
                Vector3 playerDirection = (player.position - bot.transform.position).normalized;
                Vector3 coverPosition = coverManager.GetNearestCoverPosition(bot.transform.position, playerDirection,
                    stateConfig.angleThreshold);
                bot.controller.GoTo(coverPosition);
                currentCoverTime = stateConfig.coverUpdateInterval;
            }

            if (currentCoverTime > 0 && bot.controller.IsArrivedAtTargetPosition())
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
            if (currentCoverTime > 0 && bot.controller.IsArrivedAtTargetPosition() && startedReload && !bot.IsNeedToReload())
            {
                return stateManager.GetStateIdByName("AttackState");
            }

            return stateManager.GetStateIdByName("KeepCurrentState");
        }
    }
}