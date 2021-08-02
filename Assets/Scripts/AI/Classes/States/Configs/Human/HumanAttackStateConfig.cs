using System;
using UnityEngine;

namespace AI.Classes.States.Configs
{
    [Serializable]
    public class HumanAttackStateConfig: AiStateConfig
    {
        [Range(0, 1)] public float contactLosePlayerVisibility = 0.2f;
        [Range(0, 1)] public float playerFollowVisionDistancePart = 0.5f;
        public float retreatDistance = 4.5f;
    }
}