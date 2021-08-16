using System;
using System.Collections.Generic;
using System.Linq;
using AI.Classes;
using AI.Classes.Groups;
using AI.Enums;
using AI.Scriptables;
using UnityEditor;
using UnityEngine;
using Util;

namespace AI
{
    // must be on global controller
    public class AiGroupManager : MonoBehaviour
    {
        public AiGroupFormationConfig formationConfig;

        public bool visualizeGroups = false;

        private List<AiBot> bots;
        private List<AiGroup> groups;

        private void Start()
        {
            //bots = new List<AiBot>();
            bots = FindObjectsOfType<AiBot>().ToList();
            groups = new List<AiGroup>();
            AssignBots();
            FormGroups();
            AssignGroupRoles();
        }

        private void Update()
        {
            
            //AssignGroupRoles();
        }

        private void AssignBots()
        {
            bots = FindObjectsOfType<AiBot>().ToList();
        }

        private void FormGroups()
        {
            for (int i = 0; i < bots.Count; i++)
            {
                AiBot bot1 = bots[i];
                Vector3 position1 = bot1.transform.position;
                for (int j = i + 1; j < bots.Count; j++)
                {
                    AiBot bot2 = bots[j];
                    Vector3 position2 = bot2.transform.position;
                    if (FastDistance(position1, position2) <
                        formationConfig.groupFormDistance)
                    {
                        float distance = Vector3.Distance(position1, position2);
                        if (distance < formationConfig.groupFormDistance)
                        {
                            bool isInGroup1 = bot1.IsInGroup();
                            bool isInGroup2 = bot2.IsInGroup();

                            if (!isInGroup1 && !isInGroup2)
                            {
                                // make new group
                                AiGroup group = new AiGroup(bot1, bot2);
                                groups.Add(group);
                                continue;
                            }

                            if (!isInGroup1 && isInGroup2)
                            {
                                // merge
                                AiGroup group2 = bot2.GetAiGroup();
                                group2.Add(bot1);
                                continue;
                            }

                            if (isInGroup1 && !isInGroup2)
                            {
                                // merge
                                AiGroup group1 = bot1.GetAiGroup();
                                group1.Add(bot2);
                                continue;
                            }

                            if (isInGroup1 && isInGroup2)
                            {
                                // merge if groups are not the same
                                AiGroup group1 = bot1.GetAiGroup();
                                AiGroup group2 = bot2.GetAiGroup();
                                if (!group1.Equals(group2))
                                {
                                    if (group1.Count() < group2.Count())
                                    {
                                        bot1.SetAiGroup(group2);
                                    }
                                    else
                                    {
                                        bot2.SetAiGroup(group1);
                                    }
                                }
                                /*if (!group1.Equals(group2))
                                {
                                    i--;
                                    j--;
                                    if (group1.Count() < group2.Count())
                                    {
                                        group2.Merge(group1);
                                        groups.Remove(group1);
                                    }
                                    else
                                    {
                                        group1.Merge(group2);
                                        groups.Remove(group2);
                                    }
                                }*/
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < groups.Count; i++)
            {
                AiGroup group = groups[i];
                if (group.Count() <= 0)
                    groups.Remove(group);
            }
        }

        private void AssignGroupRoles()
        {
            for (int i = 0; i < groups.Count; i++)
            {
                AiGroup group = groups[i];
                for (int j = 0; j < group.Count(); j++)
                {
                    AiBot bot = group.GetBot(j);
                    AiGroupRole[] roles = group.GetPrioritizedRoles(formationConfig);
                    for (int r = 0; r < roles.Length; r++)
                    {
                        if (bot.IsRoleAssignable(roles[r]))
                        {
                            bot.groupRole = roles[r];
                            group.UpdateRoleCounts();
                            break;
                        }
                    }
                }
            }
        }


        private float FastDistance(Vector3 point1, Vector3 point2)
        {
            Vector3 delta = point1 - point2;
            delta.x = Mathf.Abs(delta.x);
            delta.y = Mathf.Abs(delta.y);
            delta.z = Mathf.Abs(delta.z);
            return Mathf.Min(Mathf.Min(delta.x, delta.y), delta.z);
        }

        private void OnDrawGizmos()
        {
            if (!visualizeGroups || groups == null)
                return;
            for (int i = 0; i < groups.Count; i++)
            {
                AiGroup group = groups[i];
                Gizmos.color = ColorUtil.RandomColor(i);
                
                Vector3 center = Vector3.zero;
                for (int j = 0; j < group.Count(); j++)
                {
                    AiBot bot = group.GetBot(j);
                    center += bot.transform.position;
                }

                center /= group.Count();
                for (int j = 0; j < group.Count(); j++)
                {
                    AiBot bot = group.GetBot(j);
                    Gizmos.DrawLine(center, bot.transform.position);
                    Gizmos.DrawCube(bot.GetPlayerLastPosition(), new Vector3(1, 2, 1));
                    
                    Handles.Label(transform.position, bot.groupRole.ToString());
                }
            }
        }
    }
}