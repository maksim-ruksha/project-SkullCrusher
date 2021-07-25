using System;
using UnityEngine;

namespace AI.Classes.States.Configs
{
    [Serializable]
    public class SearchStateConfig : AiStateConfig
    {
        [Range(0, 1)] public float detectionPlayerVisibility = 0.2f;
        public float pointCheckWaitTime = 2.5f;
        public float pointLookAroundInterval = 0.75f;
        [Range(0, 1)] public float lookAroundRange = 0.5f;

        [Header("Advanced Settings")]
        // limits amount of covers to check
        public int coverCountChoiceLimiter = 64;

        // also limits covers, but by distance
        public float maxCoverDistance = 15.0f;

        // "field of view" for cover picking at last player's position
        [Range(0, 90)] public float maxDirectionAngleDifference = 80.0f;

        // maximum difference between cover direction and player's last velocity
        [Range(0, 90)] public float maxCoverDirectionAngleDifference = 70.0f;

        // if distance to next cover point less than this,
        // than bot will decide to check it without considering the remained positions
        public float nearestPositionLowDistanceBound = 5.0f;
    }
}