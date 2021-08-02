using System;
using AI.Classes.States.Configs;
using Level.Covers;
using UnityEngine;

namespace AI.Classes.States
{
    [Serializable]
    public abstract class AiState
    {
        public string name = "AiState";
        
        protected AiStateConfig config;
        protected AiBot bot;
        
        protected Transform player;
        protected StateManager stateManager;
        protected CoverManager coverManager;


        public AiState(AiStateConfig config, AiBot bot)
        {
            this.config = config;
            this.bot = bot;
            
            player = bot.playerTransform;
            stateManager = bot.stateManager;
        }
        
        public abstract void Update();
        
        public abstract int TransitionCheck();
    }
}