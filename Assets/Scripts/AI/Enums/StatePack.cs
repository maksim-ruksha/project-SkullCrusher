using System;
using System.Collections.Generic;
using AI.Classes.States;
using AI.Classes.States.Human;
using AI.Scriptables;
using UnityEngine;

namespace AI.Enums
{
    public static class StatePack
    {

        public static List<AiState> HumanStatePack(AiHumanBotConfig config, AiBot bot, Transform player, StateManager manager)
        {
            return new List<AiState>
            {
                new HumanIdleState(config.humanIdleStateConfig, bot),
                new HumanContactState(config.humanContactStateConfig, bot),
                new HumanAttackState(config.humanAttackStateConfig, bot),
                new HumanTakeCoverState(config.humanTakeCoverStateConfig, bot),
                new HumanChaseState(config.humanChaseStateConfig, bot),
                new HumanSearchState(config.humanSearchStateConfig, bot),
                new HumanRearAttackState(config.humanRearAttackStateConfig, bot),
                new HumanRetreatState(config.humanHideStateConfig, bot),
                new HumanHelpState(config.humanHelpStateConfig, bot)
            };
        }
        
        public static List<AiState> CombatUnit4StatePack()
        {
            throw new NotImplementedException();
        }
        
        public static List<AiState> CombatUnit4Dot1StatePack()
        {
            throw new NotImplementedException();
        }
        
        public static List<AiState> CombatUnit5Dot1StatePack()
        {
            throw new NotImplementedException();
        }
        
        public static List<AiState> InfiltrationUnit2StatePack()
        {
            throw new NotImplementedException();
        }
        
    }
}