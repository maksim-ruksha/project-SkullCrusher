using AI.Classes.States.Configs;
using UnityEngine;

namespace AI.Classes.States
{
    public class HelpState: AiState
    {
        private HelpStateConfig stateConfig;

        public HelpState(AiStateConfig config, AiBot bot, Transform player): base(config, bot, player)
        {
            name = HelpState;
            stateConfig = (HelpStateConfig) config;
        }

        public override void Transit(AiStateConfig newConfig)
        {
            stateConfig = (HelpStateConfig) config;
        }

        public override void Update()
        {
            throw new System.NotImplementedException();
        }

        public override string TransitionCheck()
        {
            throw new System.NotImplementedException();
        }
    }
}