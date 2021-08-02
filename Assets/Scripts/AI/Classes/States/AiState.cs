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
        public StateManager manager;


        public AiState(AiStateConfig config, AiBot bot, Transform player, StateManager manager)
        {
            this.config = config;
            this.bot = bot;
            this.player = player;
            this.manager = manager;
        }
        
        public abstract void Update();
        
        public abstract int TransitionCheck();
    }
}