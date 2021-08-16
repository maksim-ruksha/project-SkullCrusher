using System.Runtime.InteropServices;
using AI.Classes.States.Configs;
using AI.Classes.States.Configs.Human;
using UnityEngine;

namespace AI.Classes.States.Human
{
    public class HumanChaseState : AiState
    {
        private HumanChaseStateConfig stateConfig;

        public HumanChaseState(AiStateConfig config, AiBot bot) : base(config, bot)
        {
            name = "ChaseState";
            stateConfig = (HumanChaseStateConfig) config;
        }
        public override void Transit(AiStateConfig newConfig)
        {
            stateConfig = (HumanChaseStateConfig) newConfig;
        }
        public override void Update()
        {
            bot.controller.GoTo(bot.GetPlayerLastPosition() + bot.GetPlayerLastVelocity());
        }

        public override int TransitionCheck()
        {
            float playerVisibility = bot.controller.GetPlayerVisibility();
            if (playerVisibility > stateConfig.detectionPlayerVisibility)
            {
                return stateManager.GetStateIdByName("AttackState");
            }

            if (bot.controller.IsArrivedAtTargetPosition())
            {
                return stateManager.GetStateIdByName("SearchState");
            }

            return stateManager.GetStateIdByName("KeepCurrentState");
        }
    }
}