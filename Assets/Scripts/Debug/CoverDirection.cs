using System;
using System.Collections.Generic;
using Level.Covers;
using Level.Covers.Classes;
using UnityEngine;
using Util;

namespace Debug
{
    public class CoverDirection : MonoBehaviour
    {

        public CoverManager coverManager;
        public int limiter = 50;
        [Range(0, 90)] public float fieldOfViewToCheck = 75.0f;
        [Range(0, 90)] public float coverDirectionMaxAngleDifference = 30.0f;
        public float maxCoverDistance = 200.0f;
        
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
                    fieldOfViewToCheck && delta.sqrMagnitude <= maxDistance && Vector3.Angle(coverDirection, -direction) < coverDirectionMaxAngleDifference)
                {
                    result.Add(coverPosition);
                }
                
            }

            return result;
        }

        private void OnDrawGizmosSelected()
        {
            List<Vector3> positions = GetCoverListToCheck(transform.position, transform.forward, limiter, maxCoverDistance);
            Gizmos.color = ColorUtil.RandomColor(GetInstanceID());
            for (int i = 0; i < positions.Count; i++)
            {
                Gizmos.DrawSphere(positions[i], 0.125f);
            }
            
        }
    }
}