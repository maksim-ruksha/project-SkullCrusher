using System;
using UnityEngine;

namespace AI.Classes.States.Configs.Human
{
    [Serializable]
    public class HumanHelpStateConfig: AiStateConfig
    {
        
        [Range(0, 1)] public float addHealthAmount = 0.15f;
        public float healingTime = 3.0f;
        public float healingAbilityRestoreTime = 10.0f;
    }
}