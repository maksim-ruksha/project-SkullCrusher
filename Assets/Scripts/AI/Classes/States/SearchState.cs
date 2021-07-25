using System;
using System.Collections.Generic;
using AI.Classes.States.Configs;
using Level.Covers;
using Level.Covers.Classes;
using UnityEngine;
using Random = System.Random;


namespace AI.Classes.States
{
    public class SearchState : AiState
    {
        private SearchStateConfig stateConfig;
        private CoverManager coverManager;
        private Random random;

        private float sqrNearestPositionLowBound;

        private List<Vector3> positionsToCheck;

        private Vector3 currentCheckingPosition;

        private float currentWaitTime;
        private float currentLookAroundTime;

        public SearchState(AiStateConfig config, AiBot bot, Transform player) : base(config, bot, player)
        {
            name = SearchState;
            stateConfig = (SearchStateConfig) config;
            random = new Random(config.GetHashCode() + bot.config.GetHashCode());
        }

        public override void Transit(AiStateConfig newConfig)
        {
            stateConfig = (SearchStateConfig) config;

            positionsToCheck = GetCoverListToCheck(bot.transform.position, bot.GetPlayerLastVelocity(),
                stateConfig.coverCountChoiceLimiter, stateConfig.maxCoverDistance);

            if (positionsToCheck.Count > 0)
                CheckCover(GetNearestPosition(positionsToCheck));
        }

        public override void Update()
        {
            CheckCoverUpdate();
            /*if (currentWaitTime <= 0)
            {
            }*/
        }

        public override string TransitionCheck()
        {
            float playerVisibility = bot.controller.GetPlayerVisibility();
            if (playerVisibility > stateConfig.detectionPlayerVisibility)
            {
                return AttackState;
            }
            
            if (positionsToCheck.Count <= 0)
            {
                return IdleState;
            }

            return KeepCurrentState;
        }


        private void CheckCover(Vector3 position)
        {
            currentCheckingPosition = position;
            bot.controller.GoTo(currentCheckingPosition);
        }

        private void CheckCoverUpdate()
        {
            if (currentWaitTime > 0)
            {
                currentWaitTime -= Time.deltaTime;
                if (currentWaitTime <= 0)
                {
                    // done with checking this position, go to next
                    if (positionsToCheck.Count > 0)
                    {
                        Vector3 point = GetNearestPosition(positionsToCheck);
                        positionsToCheck.Remove(point);
                        CheckCover(point); 
                    }
                }
            }

            if (currentLookAroundTime > 0)
            {
                currentLookAroundTime -= Time.deltaTime;
                if (currentLookAroundTime <= 0)
                {
                    Vector3 direction = bot.controller.headTransform.forward;
                    Vector3 randomizedDirection = RandomizeDirection(direction, stateConfig.lookAroundRange);
                    bot.controller.LookAtDirection(randomizedDirection);
                }
            }

            if (bot.controller.IsArrivedAtTargetPosition())
            {
                currentWaitTime = stateConfig.pointCheckWaitTime;
                currentLookAroundTime = stateConfig.pointLookAroundInterval;
            }
        }

        private Vector3 GetNearestPosition(List<Vector3> positions)
        {
            if (positions.Count == 0)
                throw new Exception("Can't get nearest distance: list is empty");
            
            Vector3 nearestPosition = positions[0];
            float nearestDistance = (nearestPosition - bot.transform.position).sqrMagnitude;
            
            if (nearestDistance < stateConfig.nearestPositionLowDistanceBound)
                return nearestPosition;
            
            for (int i = 1; i < positions.Count; i++)
            {
                Vector3 position = positions[i];
                Vector3 delta = position - bot.transform.position;
                float distance = delta.sqrMagnitude;
                if (nearestDistance < stateConfig.nearestPositionLowDistanceBound)
                    return position;
                if (distance < nearestDistance)
                {
                    nearestPosition = position;
                    nearestDistance = distance;
                }
            }

            return nearestPosition;
        }

        private List<Vector3> GetCoverListToCheck(Vector3 position, Vector3 direction, int limiter, float maxDistance)
        {
            maxDistance *= maxDistance;
            List<Vector3> result = new List<Vector3>();
            List<Cover> nearestCluster = coverManager.GetNearestClusterList(position);

            if (limiter < 0)
                limiter = nearestCluster.Count;

            // TODO: change limiter depending on difficulty
            for (int i = 0; i < Mathf.Min(nearestCluster.Count, limiter); i++)
            {
                Vector3 coverPosition = nearestCluster[i].position;
                Vector3 coverDirection = nearestCluster[i].direction;
                Vector3 directionToCover = (nearestCluster[i].position - position).normalized;
                Vector3 delta = coverPosition - position;
                if (Vector3.Angle(direction, directionToCover) <=
                    stateConfig.maxDirectionAngleDifference && delta.sqrMagnitude <= maxDistance &&
                    Vector3.Angle(coverDirection, -direction) < stateConfig.maxCoverDirectionAngleDifference)
                {
                    result.Add(coverPosition);
                }
            }

            return result;
        }


        private Vector3 RandomizeDirection(Vector3 direction, float difference)
        {
            float multiplier = ((float) random.NextDouble() * 2 - 1) * difference;

            Vector3 randomVector = GetRandomVector3() * (2 * multiplier);
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