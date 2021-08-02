using AI.Classes.States.Configs;
using UnityEngine;

namespace AI.Classes.States
{
    public class HelpState : AiState
    {
        private HelpStateConfig stateConfig;

        public HelpState(AiStateConfig config, AiBot bot, Transform player, StateManager manager) : base(config, bot, player, manager)
        {
            name = "HelpState";
            stateConfig = (HelpStateConfig) config;
        }

        public override void Update()
        {
            throw new System.NotImplementedException();
        }

        public override int TransitionCheck()
        {
            throw new System.NotImplementedException();
        }
    }
}