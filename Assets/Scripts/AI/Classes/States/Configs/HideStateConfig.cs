using System;
using UnityEngine;

namespace AI.Classes.States.Configs
{
    [Serializable]
    public class HideStateConfig: AiStateConfig
    {
        [Range(0, 1)] public float restoredHealthThreshold = 0.5f;
        [Range(0, 1)] public float angleThreshold = 0.1f;
    }
}