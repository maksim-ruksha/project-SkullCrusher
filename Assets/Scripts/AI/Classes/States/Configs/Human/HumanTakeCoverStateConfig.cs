using System;
using UnityEngine;

namespace AI.Classes.States.Configs.Human
{
    [Serializable]
    public class HumanTakeCoverStateConfig : AiStateConfig
    {
        //public bool botCanCrouch;
        [Range(0, 90)] public float angleThreshold = 15.0f;
        public float coverUpdateInterval;
    }
}