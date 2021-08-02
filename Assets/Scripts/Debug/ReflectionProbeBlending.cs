using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Debug
{
    public class ReflectionProbeBlending : MonoBehaviour
    {
        private MeshRenderer meshRenderer;
        private ReflectionProbe reflectionProbe;
        
        private void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            reflectionProbe = GetComponent<ReflectionProbe>();
        }

        private void Update()
        {
            List<ReflectionProbeBlendInfo> result = null;
            meshRenderer.GetClosestReflectionProbes(result);
            //meshRenderer.reflectionProbeUsage= 
        }
    }
}