using System;
using Damages.Enums;
using UnityEngine;

namespace Violence.Classes
{
    [Serializable]
    public class ViolenceLayer
    {
        public string shaderMaskName = "_ViolenceMask";
        public float lowerBoundDamageAmount = 1.0f;
        public DamageType damageType;
        public Material maskSpreadMaterial;
    }
}