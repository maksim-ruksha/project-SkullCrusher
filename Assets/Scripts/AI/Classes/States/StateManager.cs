using System;
using System.Collections.Generic;
using AI.Classes.States.Configs;

namespace AI.Classes.States
{
    [Serializable]
    public class StateManager
    {
        private int currentStateId = 1;
        
        private List<AiState> states;
        private List<AiStateConfig> configs;
        private Dictionary<string, int> ids;

        public StateManager()
        {
            states = new List<AiState>();
            configs = new List<AiStateConfig>();
            ids = new Dictionary<string, int>();

            states.Add(null);
            configs.Add(null); 
            ids.Add("KeepCurrentState", 0);
        }

        public void Update()
        {
            states[currentStateId].Update();
            int response = states[currentStateId].TransitionCheck();
            if (response != 0)
            {
                currentStateId = response;
                states[currentStateId].Transit(configs[currentStateId]);
            }
        }

        public void UpdateConfig(int id, AiStateConfig config)
        {
            configs[id] = config;
        }

        public int RegisterState(AiState state, AiStateConfig config)
        {
            int id = states.Count;
            states.Add(state);
            configs.Add(config);

            ids.Add(state.name, id);
            return id;
        }

        public int GetStateIdByName(string name)
        {
            return ids[name];
        }
    }
}