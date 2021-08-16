using AI.Classes.States.Configs;
using AI.Classes.States.Configs.Human;
using UnityEngine;

namespace AI.Classes.States.Human
{
    public class HumanContactState : AiState
    {
        private HumanContactStateConfig stateConfig;

        private float contactTime;

        public HumanContactState(AiStateConfig config, AiBot bot) : base(config, bot)
        {
            name = "ContactState";
            stateConfig = (HumanContactStateConfig) config;
        }
        
        public override void Transit(AiStateConfig newConfig)
        {
            stateConfig = (HumanContactStateConfig) newConfig;
        }

        public override void Update()
        {
            bot.controller.LookAt(player.position);
            contactTime += Time.deltaTime * bot.controller.GetPlayerVisibilityDistanceMultiplier();
        }

        public override int TransitionCheck()
        {
            // do not forget to reset contactTime
            float playerVisibility = bot.controller.GetPlayerVisibility();
            if (playerVisibility > stateConfig.instantContactPlayerVisibility || contactTime > stateConfig.contactTime)
            {
                contactTime = 0.0f;
                /* TODO: group behaviour
                if (bot.IsInGroup())
                {
                    AiBotGroupRole role = bot.groupRole;
                    
                }
                */
                return stateManager.GetStateIdByName("AttackState");
            }

            if (playerVisibility < stateConfig.contactLosePlayerVisibility)
            {
                contactTime = 0.0f;
                return stateManager.GetStateIdByName("IdleState");
            }

            return stateManager.GetStateIdByName("KeepCurrentState");
        }
    }
}