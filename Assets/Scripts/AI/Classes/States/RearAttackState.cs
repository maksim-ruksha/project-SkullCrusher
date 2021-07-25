using AI.Classes.States.Configs;
using UnityEngine;

namespace AI.Classes.States
{
    public class RearAttackState: AiState
    {
        private RearAttackStateConfig stateConfig;

        public RearAttackState(AiStateConfig config, AiBot bot, Transform player): base(config, bot, player)
        {
            name = RearAttackState;
            stateConfig = (RearAttackStateConfig) config;
        }

        public override void Transit(AiStateConfig config)
        {
            stateConfig = (RearAttackStateConfig) config;
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