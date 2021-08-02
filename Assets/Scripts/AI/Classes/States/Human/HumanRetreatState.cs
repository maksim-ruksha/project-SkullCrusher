using System;
using System.Collections.Generic;
using AI.Classes.States.Configs;
using AI.Classes.States.Configs.Human;
using Level.Covers.Classes;
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
    }
}