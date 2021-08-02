using System.Collections.Generic;
using AI.Classes.States.Configs;
using AI.Classes.States.Configs.Human;
using Level.Covers;
using Level.Covers.Classes;
using UnityEngine;
using Random = System.Random;


namespace AI.Classes.States.Human
{
    public class HumanSearchState : AiState
    {
        private HumanSearchStateConfig stateConfig;
        private Random random;

        private List<Vector3> positionsToCheck;

        private Vector3 currentPositionToCheck;

        private float currentWaitTime;

        public HumanSearchState(AiStateConfig config, AiBot bot) : base(config, bot)
        {
            name = "SearchState";
            stateConfig = (HumanSearchStateConfig) config;
            random = new Random(config.GetHashCode() + bot.config.GetHashCode());
            positionsToCheck = GetCoverListToCheck(bot.transform.position, bot.GetPlayerLastVelocity(),
                stateConfig.coverChoiceLimiter);
        }

        public override void Update()
        {
            if (currentWaitTime <= 0)
            {
                if (CanGoToNextCheckPosition())
                {
                    GoToNextCheckPosition();
                }
            }
        }

        public override int TransitionCheck()
        {
            float playerVisibility = bot.controller.GetPlayerVisibility();

            if (playerVisibility > stateConfig.detectionPlayerVisibility)
            {
                return stateManager.GetStateIdByName("AttackState");
            }

            return stateManager.GetStateIdByName("KeepCurrentState");
        }

        private void GoToNextCheckPosition()
        {
            currentPositionToCheck = positionsToCheck[0];
            positionsToCheck.RemoveAt(0);
        }

        private bool CanGoToNextCheckPosition()
        {
            return positionsToCheck.Count > 0;
        }


        private List<Vector3> GetCoverListToCheck(Vector3 position, Vector3 direction, int limiter)
        {
            List<Vector3> result = new List<Vector3>();
            List<Cover> nearestCluster = coverManager.GetNearestClusterList(position);

            if (limiter < 0)
                limiter = nearestCluster.Count;

            // TODO: change limiter depending on difficulty
            for (int i = 0; i < Mathf.Min(nearestCluster.Count, limiter); i++)
            {
                Vector3 coverPosition = nearestCluster[i].position;
                Vector3 coverDirection = (nearestCluster[i].position - position).normalized;
                if (Vector3.Dot(direction, coverDirection) <
                    stateConfig.maxDirectionDifference)
                {
                    result.Add(coverPosition);
                }
            }

            return result;
        }


        private Vector3 RandomizeDirection(Vector3 direction, float difference)
        {
            float multiplier = ((float) random.NextDouble() * 2 - 1) * difference;

            Vector3 randomVector = GetRandomVector3() * 2 * multiplier;
            return (direction + randomVector).normalized;
        }

        private Vector3 GetRandomVector3()
        {
            float x = (float) random.NextDouble() * 2 - 1;
            float y = (float) random.NextDouble() * 2 - 1;
            float z = (float) random.NextDouble() * 2 - 1;
            return new Vector3(x, y, z).normalized;
        }
    }
}