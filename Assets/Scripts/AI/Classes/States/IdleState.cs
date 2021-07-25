using System;
using System.Collections.Generic;
using AI.Classes.States.Configs;
using AI.Enums;
using UnityEngine;

namespace AI.Classes.States
{
    public class IdleState : AiState
    {
        private IdleStateConfig stateConfig;

        private Vector3 initialPosition;
        
        private ContactStateConfig contactStateConfig;
        
        private List<Vector3> patrolPoints;
        private int currentPointIndex;

        public IdleState(AiStateConfig config, AiBot bot, Transform player): base(config, bot, player)
        {
            name = IdleState;
            stateConfig = (IdleStateConfig) config;
            contactStateConfig = bot.config.contactStateConfig;
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
            stateConfig = (IdleStateConfig) config;
            contactStateConfig = bot.config.contactStateConfig;
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
                        currentPointIndex = (currentPointIndex + 1)  % patrolPoints.Count;
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

        public override string TransitionCheck()
        {
            /* TODO: make transitions of IdleState
             * Contact +
             * Help
             */
            float playerVisibility = bot.controller.GetPlayerVisibility();
            if (playerVisibility > contactStateConfig.contactStartPlayerVisibility)
            {
                return ContactState;
            }
            return KeepCurrentState;
        }
    }
}