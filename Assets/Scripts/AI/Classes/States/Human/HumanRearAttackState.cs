using AI.Classes.States.Configs;
using AI.Classes.States.Configs.Human;
using UnityEngine;

namespace AI.Classes.States.Human
{
    public class HumanRearAttackState : AiState
    {
        private HumanRearAttackStateConfig stateConfig;

        public HumanRearAttackState(AiStateConfig config, AiBot bot) : base(config, bot)
        {
            name = "RearAttackState";
            stateConfig = (HumanRearAttackStateConfig) config;
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