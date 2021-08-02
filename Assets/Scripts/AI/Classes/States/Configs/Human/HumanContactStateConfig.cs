using System;
using UnityEngine;

namespace AI.Classes.States.Configs.Human
{
    [Serializable]
    public class HumanContactStateConfig : AiStateConfig
    {
        [Range(0, 1)] public float instantContactPlayerVisibility = 0.95f;
        [Range(0, 1)] public float contactStartPlayerVisibility = 0.25f;
        [Range(0, 1)] public float contactLosePlayerVisibility = 0.15f;
        public float contactTime = 2.0f;
    }
}