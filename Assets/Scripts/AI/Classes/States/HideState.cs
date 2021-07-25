using AI.Classes.States.Configs;
using UnityEngine;

namespace AI.Classes.States
{
    public class HideState: AiState
    {

        private HideStateConfig stateConfig;

        public HideState(AiStateConfig config, AiBot bot, Transform player): base(config, bot, player)
        {
            name = HideState;
            stateConfig = (HideStateConfig) config;
        }

        public override void Transit(AiStateConfig newConfig)
        {
            stateConfig = (HideStateConfig) config;
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