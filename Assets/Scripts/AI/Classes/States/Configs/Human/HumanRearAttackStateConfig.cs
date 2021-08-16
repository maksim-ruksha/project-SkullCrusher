using System;
using UnityEngine;

namespace AI.Classes.States.Configs.Human
{
    [Serializable]
    public class HumanRearAttackStateConfig: AiStateConfig
    {
        public float maximumBypassRadius = 15.0f;

        public float attackPlayerVisibility = 0.5f;
        // probably should be replaced by coverManager value
        public LayerMask obstaclesMask;
        [Range(1, 50)]
        public int bypassPathSegments = 6;
    }
}