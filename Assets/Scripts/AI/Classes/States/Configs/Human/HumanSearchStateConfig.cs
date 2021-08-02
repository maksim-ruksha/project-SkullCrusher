using System;
using UnityEngine;

namespace AI.Classes.States.Configs.Human
{
    [Serializable]
    public class HumanSearchStateConfig: AiStateConfig
    {
        [Range(0, 1)] public float detectionPlayerVisibility = 0.2f;
        public float maxSearchPointDistance = 15.0f;
        public float searchPointWaitTime = 2.5f;
        public int coverChoiceLimiter = 64;
        [Range(0, 1)] public float maxDirectionDifference = 0.2f;
        public float time = 60.0f;
    }
}