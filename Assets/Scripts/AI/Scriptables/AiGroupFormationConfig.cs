using UnityEngine;

namespace AI.Scriptables
{
    [CreateAssetMenu(fileName = "AiGroupFormationConfig", menuName = "Skull Crusher/Ai Group Formation Config")]
    public class AiGroupFormationConfig : ScriptableObject
    {

        public float groupFormDistance = 10.0f;
        
        // public bool visualContactNeeded = true;

        // nice
        [Header("Role Weights")] [Range(0, 1)] public float commonRoleWeight = 0.419f;
        [Range(0, 1)] public float tankRoleWeight = 0.420f;
        [Range(0, 1)] public float tankSupporterRoleWeight = 0.69f;
        [Range(0, 1)] public float rearAttackerRoleWeight = 0.526f;
        [Range(0, 1)] public float medicRoleWeight = 0.0911f;

        [Header("Minimums")]
        [Range(1, 10)] public int tankMinimumAgentsCount = 2;
        [Range(1, 10)] public int tankSupporterMinimumAgentsCount = 3;
        [Range(1, 10)] public int rearAttackerMinimumAgentsCount = 3;
        [Range(1, 10)] public int medicMinimumAgentsCount = 4;


        public const int SupportersPerTank = 2;


        public void NormalizeWeights()
        {
            float sum = commonRoleWeight + tankRoleWeight + rearAttackerRoleWeight + medicRoleWeight;
            commonRoleWeight /= sum;
            tankRoleWeight /= sum;
            rearAttackerRoleWeight /= sum;
            medicRoleWeight /= sum;
        }
    }
}