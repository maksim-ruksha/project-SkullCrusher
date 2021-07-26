using AI.Classes.States.Configs;
using Level.Covers;
using Preferences;
using UnityEngine;

namespace AI.Classes.States
{
    public class HideState: AiState
    {
        private HideStateConfig stateConfig;

        private CoverManager coverManager;
        
        public HideState(AiStateConfig config, AiBot bot, Transform player): base(config, bot, player)
        {
            name = HideState;
            stateConfig = (HideStateConfig) config;    GameObject globalController = GameObject.Find(Settings.GameObjects.GlobalController);
            coverManager = globalController.GetComponent<CoverManager>();
        }

        public override void Transit(AiStateConfig newConfig)
        {
            stateConfig = (HideStateConfig) config;

            Vector3 playerDirection = (player.position - bot.transform.position).normalized;
            Vector3 coverPosition = coverManager.GetNearestCoverPosition(bot.transform.position, playerDirection,
                stateConfig.angleThreshold);
            bot.controller.GoTo(coverPosition);

        }

        public override void Update()
        {
            if (bot.controller.IsArrivedAtTargetPosition())
            {
               // TODO: call group help and wait, else keep escaping while attacking
            }
        }

        public override string TransitionCheck()
        {
            if (bot.GetRangedHealth() > stateConfig.restoredHealthThreshold)
            {
                return AttackState;
            }

            return KeepCurrentState;
        }
    }
}