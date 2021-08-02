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