using System;

namespace AI.Classes.States.Configs.Human
{
    [Serializable]
    public class HumanRearAttackStateConfig: AiStateConfig
    {
        public float maximumBypassRadius = 15.0f;
    }
}