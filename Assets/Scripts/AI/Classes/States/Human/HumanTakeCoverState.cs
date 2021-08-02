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
        private bool pickedCover;
        private bool startedReload;

        public HumanTakeCoverState(AiStateConfig config, AiBot bot) : base(config, bot)
        {
            name = "TakeCoverState";
            stateConfig = (HumanTakeCoverStateConfig) config;
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
                return stateManager.GetStateIdByName("AttackState");
            }

            return stateManager.GetStateIdByName("KeepCurrentState");
        }
    }
}