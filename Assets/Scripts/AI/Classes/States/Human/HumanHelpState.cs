using AI.Classes.States.Configs;
using AI.Classes.States.Configs.Human;
using UnityEngine;

namespace AI.Classes.States.Human
{
    public class HumanHelpState : AiState
    {
        private HumanHelpStateConfig stateConfig;

        public HumanHelpState(AiStateConfig config, AiBot bot) : base(config, bot)
        {
            name = "HelpState";
            stateConfig = (HumanHelpStateConfig) config;
        }


        public override void Transit(AiStateConfig newConfig)
        {
            throw new System.NotImplementedException();
        }

        public override void Update()
        {
            // TODO: implement this
            throw new System.NotImplementedException();
        }

        public override int TransitionCheck()
        {
            throw new System.NotImplementedException();
        }
    }
}