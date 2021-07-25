using System;
using AI.Classes.States.Configs;
using UnityEngine;

namespace AI.Classes.States
{
    [Serializable]
    public abstract class AiState
    {
        public string name = "AiState";
        
        public AiStateConfig config;
        public AiBot bot;
        public Transform player;

        public const string KeepCurrentState = "";
        public const string IdleState = "Idle";
        public const string ContactState = "Contact";
        public const string AttackState = "Attack";
        public const string TakeCoverState = "Idle";
        public const string ChaseState = "Chase";
        public const string SearchState = "Search";
        public const string RearAttackState = "RearAttack";
        public const string HideState = "Hide";
        public const string HelpState = "Help";

        public AiState(AiStateConfig config, AiBot bot, Transform player)
        {
            this.config = config;
            this.bot = bot;
            this.player = player;
        }

        // good place to init shit
        public abstract void Transit(AiStateConfig newConfig);
        
        public abstract void Update();
        
        public abstract string TransitionCheck();
    }
}