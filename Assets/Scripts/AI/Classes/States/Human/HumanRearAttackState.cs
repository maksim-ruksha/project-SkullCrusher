using System.Collections.Generic;
using AI.Classes.States.Configs;
using AI.Classes.States.Configs.Human;
using UnityEngine;

namespace AI.Classes.States.Human
{
    public class HumanRearAttackState : AiState
    {
        private HumanRearAttackStateConfig stateConfig;
        

        private List<Vector3> path;
        
        public HumanRearAttackState(AiStateConfig config, AiBot bot): base(config, bot)
        {
            name = "RearAttackState";
            stateConfig = (HumanRearAttackStateConfig) config;
        }

        public override void Transit(AiStateConfig newConfig)
        {
            stateConfig = (HumanRearAttackStateConfig) newConfig;
            
            int problems1 = GetProblematicSegmentsCount(false, out List<Vector3> positions1);
            int problems2 = GetProblematicSegmentsCount(true, out List<Vector3> positions2);

            path = problems1 < problems2 ? positions1 : positions2;
            if (path.Count > 0)
            {
                Vector3 firstPosition = path[0];
                path.Remove(firstPosition);
                bot.controller.GoTo(firstPosition);
            }
        }

        public override void Update()
        {
            if (bot.controller.IsArrivedAtTargetPosition())
            {
                if (path.Count > 0)
                {
                    Vector3 position = path[0];
                    path.Remove(position);
                    bot.controller.GoTo(position);
                }
            }

            
            if (bot.controller.GetPlayerVisibility() > stateConfig.attackPlayerVisibility)
            {
                bot.controller.LookAt(player.position);
                bot.TrackPlayer();
                bot.Fire();
            }
        }

        public override int TransitionCheck()
        {
            if (path.Count <= 0)
            {
                return stateManager.GetStateIdByName("AttackState");
            }

            return stateManager.GetStateIdByName("KeepCurrentState");
        }
        

        private int GetProblematicSegmentsCount(bool invert, out List<Vector3> positions)
        {
            int result = 0;
            positions = new List<Vector3>();
            
            float sideMultiplier = invert ? 1.0f : -1.0f;

            Vector3 playerDelta = player.position - bot.transform.position;
            
            float bypassRadius = Mathf.Min(playerDelta.magnitude * 2, stateConfig.maximumBypassRadius);
            
            Vector3 playerForwardDirection = playerDelta.normalized;
            Vector3 playerSideDirection = GetHorizontalNormal(playerForwardDirection) * sideMultiplier;

            float step = 1.0f / (stateConfig.bypassPathSegments - 1); // * sideMultiplier;
            Vector3 previousPoint = bot.transform.position;
            for (int i = 0; i < stateConfig.bypassPathSegments; i++)
            {
                float t = -1 + i * step;
                Vector3 circle = GetCirclePoint(t) * bypassRadius;
                Vector3 position = bot.transform.position + playerForwardDirection * circle.z + playerSideDirection * circle.x;
                positions.Add(position);
                Vector3 directionDelta = position - previousPoint;
                
                Ray ray = new Ray(previousPoint, directionDelta.normalized);
                if (Physics.Raycast(ray, directionDelta.magnitude, coverManager.shootableMask))
                {
                    result++;
                }
            }

            return result;
        }

        private Vector3 GetCirclePoint(float t)
        {
            float x = Mathf.Sin(t * Mathf.PI);
            float z = Mathf.Cos(t * Mathf.PI) + 1;

            return new Vector3(x, 0, z);
        }

        private Vector3 GetHorizontalNormal(Vector3 direction)
        {
            return new Vector3(direction.z, direction.y, -direction.x);
        }
    }
}