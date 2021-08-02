using AI.Classes.States.Configs;
using UnityEngine;

namespace AI.Classes.States
{
    public class HideState : AiState
    {
        private HideStateConfig stateConfig;

        public HideState(AiStateConfig config, AiBot bot, Transform player, StateManager manager) : base(config, bot, player, manager)
        {
            name = "HideState";
            stateConfig = (HideStateConfig) config;
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