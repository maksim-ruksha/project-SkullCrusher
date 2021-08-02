using AI.Classes.States.Configs;
using UnityEngine;

namespace AI.Classes.States
{
    public class RearAttackState : AiState
    {
        private RearAttackStateConfig stateConfig;

        public RearAttackState(AiStateConfig config, AiBot bot, Transform player, StateManager manager) : base(config, bot, player, manager)
        {
            name = "RearAttackState";
            stateConfig = (RearAttackStateConfig) config;
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