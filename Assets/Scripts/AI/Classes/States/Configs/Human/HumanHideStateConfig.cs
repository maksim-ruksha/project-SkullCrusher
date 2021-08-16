using System;
using UnityEngine;

namespace AI.Classes.States.Configs.Human
{
    [Serializable]
    public class HumanHideStateConfig: AiStateConfig
    {
        [Range(0, 1)] public float healthThreshold = 0.1f;
        [Range(0, 1)] public float healthRestoredThreshold = 0.4f;
        // This is my hiding spot, and I'm not moving until the situation has drastically improved! Now go away! And don't tell anyone I'm here!
        [Range(0, 1)] public float hidingSpotChangePlayerVisibility = 0.3f;
        [Range(0, 90)] public float hidingSpotAngleThreshold = 30.0f;
        [Range(0, 90)] public float playerLookAngleThreshold = 25.0f;
    }
}