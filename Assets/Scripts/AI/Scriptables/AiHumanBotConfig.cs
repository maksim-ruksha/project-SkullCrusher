using AI.Classes.States;
using AI.Classes.States.Configs;
using AI.Classes.States.Configs.Human;
using UnityEngine;

namespace AI.Scriptables
{
    [CreateAssetMenu(fileName = "AiConfig", menuName = "Skull Crusher/Ai Human Bot Config")]
    public class AiHumanBotConfig : ScriptableObject
    {
        [Header("Character")] public float characterSpeed = 1.0f;
        public float headRotationSpeed = 1.0f;
        public float bodyRotationSpeed = 2.0f;
        public float characterHeight = 2.0f;
        
        [Header("Weapon")]
        [Range(0, 1)] public float aimingAccuracy = 0.7f;
        [Range(0, 1)] public float recoilNeutralization = 0.5f;
        [Range(0, 1)] public float predictTakingCoverClipAmmoMultiplier = 0.10f;
        public float reloadTimeMultiplier = 1.75f;
        
        /*[Header("Behaviour")]
        [Range(0, 1)] public float coverPreference = 0.95f;
        [Range(0, 1)] public float grenadePreference = 0.25f;
        [Range(0, 1)] public float weaponChangePreference = 0.25f;*/

        [Header("Vision")] public float visionAngle = 60.0f;
        public float visionDistance = 50.0f;
        public LayerMask visibleObjectsMask;
        
        /*[Header("State Settings")] public bool allowIdleState = true;
        public bool allowContactState = true;
        public bool allowAttackState = true;
        public bool allowTakeCoverState = true;
        public bool allowChaseState = true;
        public bool allowSearchState = true;
        public bool allowRearAttackState = true;
        public bool allowHideState = true;
        public bool allowHelpState = true;*/

        [Header("Group Settings")] public bool canBeCommon = true;
        public bool canBeTank = true;
        public bool canBeTankSupporter = true;
        public bool canBeRearAttacker = true;
        public bool canBeMedic = true;
        
        public float playerPositionShareTime = 10.0f;
        public float playerPositionShareDistance = 20.0f;

        /*[Header("Behaviours Settings")]
        */
        [Header("States Configs")] public HumanIdleStateConfig humanIdleStateConfig;
        public HumanContactStateConfig humanContactStateConfig;
        public HumanAttackStateConfig humanAttackStateConfig;
        public HumanTakeCoverStateConfig humanTakeCoverStateConfig;
        public HumanChaseStateConfig humanChaseStateConfig;
        public HumanSearchStateConfig humanSearchStateConfig;
        public HumanRearAttackStateConfig humanRearAttackStateConfig;
        public HumanHideStateConfig humanHideStateConfig;
        public HumanHelpStateConfig humanHelpStateConfig;


        /*private static string[][] validStateTransitions =
        {
            // new [] {"FromState", "ToState1", "?ToQuestionableState2", "ToState3" }
            new[] {"Idle", "Contact", "Help"},
            new[] {"Contact", "Idle", "Attack", "TakeCover", "?Chase", "?Search", "RearAttack", "Hide", "Help"},
            new[] {"Attack", "TakeCover", "Chase", "?Search", "RearAttack", "Hide", "?Help"},
            new[] {"TakeCover", "Attack", "Chase", "?Search", "RearAttack", "Hide", "Help"},
            new[] {"Chase", "Attack", "Search", "?Hide", "Help"},
            new[] {"Search", "Idle", "Attack", "TakeCover", "?Chase", "Help"},
            new[] {"RearAttack", "Attack", "TakeCover", "Chase", "?Search", "Hide", "Help"},
            new[] {"Hide", "Idle", "Contact", "Attack", "TakeCover", "Chase", "Search", "RearAttack", "Hide", "Help"},
            new[] {"Help", "?Idle", "Attack", "TakeCover", "Chase", "Search", "RearAttack", "Hide"}
        };*/

        public AiStateConfig[] GetStatesConfigs()
        {
            return new AiStateConfig[]
            {
                humanIdleStateConfig,
                humanContactStateConfig,
                humanAttackStateConfig,
                humanTakeCoverStateConfig,
                humanChaseStateConfig,
                humanSearchStateConfig,
                humanRearAttackStateConfig,
                humanHideStateConfig,
                humanHelpStateConfig
            };
        }


        /*public bool IsValid()
        {
            bool[] configStates =
            {
                allowIdleState,
                allowContactState,
                allowAttackState,
                allowTakeCoverState,
                allowChaseState,
                allowSearchState,
                allowRearAttackState,
                allowHideState,
                allowHelpState
            };
            
            for (int i = 0; i < configStates.Length; i++)
            {
                bool allowed = configStates[i];
                if (!allowed)
                    continue;
                int possibleTransitionsCount = 0;
                bool questionableTransitionsOnly = true;
                for (int j = 1; j < validStateTransitions[i].Length; j++)
                {
                    string transitionStateName = validStateTransitions[i][j];
                    int stateIndex = GetStateIndex(transitionStateName);
                    if (configStates[stateIndex])
                    {
                        possibleTransitionsCount++;
                        if (transitionStateName[0] != '?')
                            questionableTransitionsOnly = false;
                    }
                }

                if (possibleTransitionsCount == 0)
                    return false;
                if (questionableTransitionsOnly)
                {
                    string stateName = validStateTransitions[i][0];
                    UnityEngine.Debug.LogWarning(
                        $"AiBotConfig {name} contains state ({stateName}) with questionable only transitions." +
                        $" See \"State Fallback\" document and consider reconfiguring to prevent strange AI behaviour",
                        this);
                }
            }

            return true;
        }*/

        /*private static int GetStateIndex(string name)
        {
            for (int i = 0; i < validStateTransitions.Length; i++)
            {
                string stateName = validStateTransitions[i][0];
                if (stateName.Equals(name))
                    return i;
            }

            return -1;
        }*/
    }
}