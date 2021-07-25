using AI.Classes.States.Configs;
using UnityEngine;

namespace AI.Classes.States
{
    public class ChaseState: AiState
    {

        private ChaseStateConfig stateConfig;

        public ChaseState(AiStateConfig config, AiBot bot, Transform player): base(config, bot, player)
        {
            name = ChaseState;
            stateConfig = (ChaseStateConfig) config;
        }

        public override void Transit(AiStateConfig newConfig)
        {
            stateConfig = (ChaseStateConfig) config;
        }

        public override void Update()
        {
            bot.controller.GoTo(bot.GetPlayerLastPosition() + bot.GetPlayerLastVelocity());
        }

        public override string TransitionCheck()
        {
            float playerVisibility = bot.controller.GetPlayerVisibility();
            if (playerVisibility > stateConfig.detectionPlayerVisibility)
            {
                return AttackState;
            }
            
            if (bot.controller.IsArrivedAtTargetPosition())
            {
                return SearchState;
            }

            return KeepCurrentState;
        }
    }
}