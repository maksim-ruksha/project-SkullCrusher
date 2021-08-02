using AI.Classes.States.Configs;
using UnityEngine;

namespace AI.Classes.States
{
    public class ContactState : AiState
    {
        private ContactStateConfig stateConfig;

        private float contactTime;

        public ContactState(AiStateConfig config, AiBot bot, Transform player, StateManager manager) : base(config, bot, player, manager)
        {
            name = "ContactState";
            stateConfig = (ContactStateConfig) config;
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
                return manager.GetStateIdByName("AttackState");
            }

            if (playerVisibility < stateConfig.contactLosePlayerVisibility)
            {
                contactTime = 0.0f;
                return manager.GetStateIdByName("IdleState");
            }

            return manager.GetStateIdByName("KeepCurrentState");
        }
    }
}