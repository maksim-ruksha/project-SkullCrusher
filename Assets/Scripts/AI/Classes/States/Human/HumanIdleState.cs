using System.Collections.Generic;
using AI.Classes.States.Configs;
using AI.Classes.States.Configs.Human;
using AI.Enums;
using UnityEngine;

namespace AI.Classes.States.Human
{
    public class HumanIdleState : AiState
    {
        private HumanIdleStateConfig stateConfig;

        private Vector3 initialPosition;

        private HumanContactStateConfig humanContactStateConfig;

        private List<Vector3> patrolPoints;
        private int currentPointIndex;

        public HumanIdleState(AiStateConfig config, AiBot bot) : base(config, bot)
        {
            name = "IdleState";
            stateConfig = (HumanIdleStateConfig) config;
            humanContactStateConfig = bot.config.humanContactStateConfig;
            initialPosition = bot.transform.position;
            if (stateConfig.type == IdleStateType.Patrol)
            {
                Transform patrolSetTransform = stateConfig.patrolSetRootTransform;
                for (int i = 0; i < patrolSetTransform.childCount; i++)
                {
                    Transform child = patrolSetTransform.GetChild(i);
                    Vector3 position = bot.controller.Sample(child.position);
                    patrolPoints.Add(position);
                }
            }
        }


        public override void Transit(AiStateConfig newConfig)
        {
            stateConfig = (HumanIdleStateConfig) newConfig;
        }

        public override void Update()
        {
            switch (stateConfig.type)
            {
                case IdleStateType.Stand:
                {
                    bot.controller.GoTo(initialPosition);
                    break;
                }

                case IdleStateType.Patrol:
                {
                    if (bot.controller.IsArrivedAtTargetPosition())
                    {
                        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Count;
                        Vector3 position = patrolPoints[currentPointIndex];
                        bot.controller.GoTo(position);
                    }

                    break;
                }

                case IdleStateType.RandomWalk:
                {
                    // do we really need this?
                    break;
                }
            }
        }

        public override int TransitionCheck()
        {
            /* TODO: make transitions of IdleState
             * Contact +
             * Help
             */
            float playerVisibility = bot.controller.GetPlayerVisibility();
            if (playerVisibility > humanContactStateConfig.contactStartPlayerVisibility)
            {
                return stateManager.GetStateIdByName("ContactState");
            }

            return stateManager.GetStateIdByName("KeepCurrentState");
        }
    }
}