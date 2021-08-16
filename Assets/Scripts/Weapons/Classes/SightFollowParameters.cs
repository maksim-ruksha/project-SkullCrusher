using System;
using UnityEngine;

namespace Weapons.Classes
{
    [Serializable]
    public class SightFollowParameters
    {
        public float sightPositionFollowAccelerationSpeed = 10.0f;
        public float sightFollowMaxOffset = 0.5f;
        public float sightPositionFollowDecelerationSpeed = 10.0f;
        [Range(0, 2)] public float moveImpact = 0.1f;
    }
}