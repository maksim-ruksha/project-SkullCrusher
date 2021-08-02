using AI.Classes.States.Configs;
using UnityEngine;

namespace AI.Classes.States
{
    public class ChaseState : AiState
    {
        private ChaseStateConfig stateConfig;

        public ChaseState(AiStateConfig config, AiBot bot, Transform player, StateManager manager) : base(config, bot, player, manager)
        {
            name = "ChaseState";
            stateConfig = (ChaseStateConfig) config;
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
                return manager.GetStateIdByName("AttackState");
            }

            if (bot.controller.IsArrivedAtTargetPosition())
            {
                return manager.GetStateIdByName("SearchState");
            }

            return manager.GetStateIdByName("KeepCurrentState");
        }
    }
}