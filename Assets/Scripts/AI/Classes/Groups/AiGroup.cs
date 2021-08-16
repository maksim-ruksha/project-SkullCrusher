using System;
using System.Collections.Generic;
using AI.Enums;
using AI.Scriptables;
using Preferences;

namespace AI.Classes.Groups
{
    public class AiGroup
    {
        private List<AiBot> bots;

        private int[] roleCounts;

        //private const int SingleId = (int) AiGroupRole.Single;
        private const int CommonId = (int) AiGroupRole.Common;
        private const int TankId = (int) AiGroupRole.Tank;
        private const int TankSupporterId = (int) AiGroupRole.TankSupporter;
        private const int RearAttackerId = (int) AiGroupRole.RearAttacker;
        private const int MedicId = (int) AiGroupRole.Medic;

        public AiGroup(List<AiBot> bots)
        {
            this.bots = bots;
            for (int i = 0; i < bots.Count; i++)
            {
                bots[i].SetAiGroup(this);
            }

            UpdateRoleCounts();
        }

        public AiGroup(AiBot bot1, AiBot bot2)
        {
            bots = new List<AiBot> {bot1, bot2};
            bot1.SetAiGroup(this);
            bot2.SetAiGroup(this);
            UpdateRoleCounts();
        }

        public void Merge(AiGroup groupToMerge)
        {
            for (int i = 0; i < groupToMerge.Count(); i++)
            {
                AiBot bot = groupToMerge.GetBot(i);
                bots.Add(bot);
                bot.SetAiGroup(this);
            }

            UpdateRoleCounts();
        }

        public void TrackPlayer(AiBot whoTracked)
        {
            for (int i = 0; i < bots.Count; i++)
            {
                AiBot bot = bots[i];
                if (!bot.Equals(whoTracked))
                {
                    float distance = (bot.transform.position - whoTracked.transform.position).magnitude;
                    float shareDistance = bot.config.playerPositionShareDistance;
                    if (distance < shareDistance)
                    {
                        float time = distance / shareDistance;
                        bot.OnGroupPlayerTracked(time);
                    }
                }
            }
        }


        public void Add(AiBot bot)
        {
            bots.Add(bot);
            bot.SetAiGroup(this);
            UpdateRoleCounts();
        }

        public void Remove(AiBot bot)
        {
            bots.Remove(bot);
            bot.SetAiGroup(null);
            UpdateRoleCounts();
        }

        public void RemoveAt(int id)
        {
            AiBot bot = bots[id];
            bots.RemoveAt(id);
            bot.SetAiGroup(null);
            UpdateRoleCounts();
        }

        public void UpdateRoleCounts()
        {
            int[] newCounts = new int[Enum.GetNames(typeof(AiGroupRole)).Length];

            for (int i = 0; i < bots.Count; i++)
            {
                AiBot bot = bots[i];
                newCounts[(int) bot.groupRole]++;
            }

            roleCounts = newCounts;
        }

        public AiBot GetBot(int id)
        {
            return bots[id];
        }

        /*public List<AiBot> GetBotsWithRole(AiGroupRole role)
        {
            List<AiBot> result = new List<AiBot>();
            for (int i = 0; i < bots.Count; i++)
            {
                AiBot bot = bots[i];
                if (bot.groupRole == role)
                {
                    result.Add(bot);
                }
            }

            return result;
        }*/

        public AiGroupRole[] GetPrioritizedRoles(AiGroupFormationConfig config)
        {
            AiGroupRole[] roles =
            {
                AiGroupRole.Common,
                AiGroupRole.Tank,
                AiGroupRole.TankSupporter,
                AiGroupRole.RearAttacker,
                AiGroupRole.Medic
            };
            float[] weights =
            {
                CommonWeight(config),
                TankWeight(config),
                TankSupporterWeight(config),
                RearAttackerWeight(config),
                MedicWeight(config)
            };

            for (int i = 0; i < roles.Length; i++)
            {
                int max = i; // di nahui
                for (int j = i + 1; j < roles.Length; j++)
                {
                    if (weights[j] > weights[max])
                        max = j;
                }

                float weightBuffer = weights[max];
                AiGroupRole roleBuffer = roles[max];
                weights[max] = weights[i];
                roles[max] = roles[i];
                weights[i] = weightBuffer;
                roles[i] = roleBuffer;
            }

            return roles;
        }

        public float CommonWeight(AiGroupFormationConfig config)
        {
            if (bots.Count <= 2)
            {
                return config.commonRoleWeight;
            }

            return (bots.Count - roleCounts[CommonId]) * config.commonRoleWeight;
        }

        public float TankWeight(AiGroupFormationConfig config)
        {
            int need = config.tankMinimumAgentsCount;
            int commons = roleCounts[CommonId];

            if (commons > need)
                return (commons - roleCounts[TankId]) * config.tankRoleWeight;
            return 0;
        }

        public float TankSupporterWeight(AiGroupFormationConfig config)
        {
            int need = config.tankSupporterMinimumAgentsCount;
            int commons = roleCounts[CommonId];

            int tanks = roleCounts[TankId];
            int supporters = roleCounts[TankSupporterId];

            if (supporters < tanks * AiGroupFormationConfig.SupportersPerTank
                && commons > need)
                return (commons - roleCounts[TankSupporterId]) * config.tankSupporterRoleWeight;
            return 0;
        }

        public float RearAttackerWeight(AiGroupFormationConfig config)
        {
            int need = config.rearAttackerMinimumAgentsCount;
            int commons = roleCounts[CommonId];
            if (commons > need)
                return (commons - roleCounts[RearAttackerId]) * config.rearAttackerRoleWeight;
            return 0;
        }

        public float MedicWeight(AiGroupFormationConfig config)
        {
            int need = config.medicMinimumAgentsCount;
            int commons = roleCounts[CommonId];
            if (commons > need)
                return (commons - roleCounts[MedicId]) * config.medicRoleWeight;
            return 0;
        }

        public int Count()
        {
            return bots.Count;
        }
    }
}