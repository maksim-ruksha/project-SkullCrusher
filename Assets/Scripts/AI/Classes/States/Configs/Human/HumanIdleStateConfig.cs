using System;
using AI.Enums;
using UnityEngine;

namespace AI.Classes.States.Configs.Human
{
    [Serializable]
    public class HumanIdleStateConfig: AiStateConfig
    {
        public IdleStateType type;

        // if patrolling
        public Transform patrolSetRootTransform;
    }
}