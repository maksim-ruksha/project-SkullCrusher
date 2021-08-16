using AI.Classes.States.Configs;
using AI.Classes.States.Configs.Human;
using UnityEngine;

namespace AI.Classes.States.Human
{
    public class HumanRetreatState : AiState
    {
        private HumanHideStateConfig stateConfig;

        public HumanRetreatState(AiStateConfig config, AiBot bot) : base(config, bot)
        {
            name = "HideState";
            stateConfig = (HumanHideStateConfig) config;
        }

        public override void Transit(AiStateConfig newConfig)
        {
            stateConfig = (HumanHideStateConfig) newConfig;
        }

        public override void Update()
        {
            float playerVisibility = bot.controller.GetPlayerVisibility();
            if (playerVisibility > stateConfig.hidingSpotChangePlayerVisibility)
            {
                Vector3 position = bot.transform.position;
                Vector3 playerDelta = player.position - position;

                Vector3 coverPosition =
                    coverManager.GetNearestCoverPosition(position, playerDelta, stateConfig.hidingSpotAngleThreshold);

                bot.controller.GoTo(coverPosition);
                if (IsPlayerLooking())
                {
                    bot.Fire();
                }
            }

            if (bot.controller.IsArrivedAtTargetPosition())
            {
                // TODO: call for help from group
            }
        }

        public override int TransitionCheck()
        {
            if (bot.GetRangedHealth() > stateConfig.healthRestoredThreshold)
            {
                return stateManager.GetStateIdByName("AttackState");
            }

            return stateManager.GetStateIdByName("KeepCurrentState");
        }

        private bool IsPlayerLooking()
        {
            Vector3 playerDelta = player.transform.position - bot.transform.position;
            return Vector3.Angle(bot.playerController.cameraTransform.forward, playerDelta) <
                   stateConfig.playerLookAngleThreshold;
        }
    }
}