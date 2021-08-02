using System;
using UnityEngine;

namespace AI.Classes.States.Configs.Human
{
    [Serializable]
    public class HumanChaseStateConfig: AiStateConfig
    {
        [Range(0, 1)] public float detectionPlayerVisibility = 0.15f;
    }
}