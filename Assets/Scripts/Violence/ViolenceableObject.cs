using System;
using System.Collections.Generic;
using Damages;
using UnityEngine;
using Violence.Classes;

namespace Violence
{
    
    /*
     * original idea by Zulubo Production
     * https://youtu.be/DWuIn4uirvo
     *
     * rewritten for better understanding and
     * to suit multilayered U L T R A V I O L E N C E needs
     */
    public class ViolenceableObject : MonoBehaviour
    {
        public Material blendMaterial;
        public ViolenceLayer[] violenceLayers;
        public int violenceTextureResolution = 128;
        public int maskRenderLayer = 16;

        private RenderTexture[] masks;
        private GameObject[] violenceMasksClones;

        private Renderer renderer;

        private Camera masksCamera;
        private RenderTexture masksRenderTexture;

        private RenderTexture temporalMaskRenderTexture;

        private readonly int violenceSpreadDamagePositionId = Shader.PropertyToID("_DamageLocalPosition");
        private readonly int violenceSpreadDamageRadiusId = Shader.PropertyToID("_DamageRadius");
        private readonly int violenceSpreadDamageAmountId = Shader.PropertyToID("_DamageAmount");

        private void Start()
        {
            renderer = GetRendererComponent(gameObject);
            InitializeViolenceLayers();
            //CreateMaskClone();
            //CreateMaskCamera();
            //CreateMaskRenderTexture();
        }

        private void Update()
        {
            // TODO: sync clones if needed
        }
        


        public void AddDamage(Vector3 position, Damage damage, float radius)
        {
            for (int i = 0; i < violenceLayers.Length; i++)
            {
                ViolenceLayer layer = violenceLayers[i];
                if (damage.amount > layer.lowerBoundDamageAmount && damage.type == layer.damageType)
                {
                    Vector3 localPosition = transform.InverseTransformPoint(position);

                    //Material violenceSpreadMaterial = renderer.material;
                    Material violenceSpreadMaterial = layer.maskSpreadMaterial;
                    
                    violenceSpreadMaterial.SetVector(violenceSpreadDamagePositionId,
                        new Vector4(localPosition.x, localPosition.y, localPosition.z, 0));
                    violenceSpreadMaterial.SetFloat(violenceSpreadDamageAmountId, damage.amount);
                    violenceSpreadMaterial.SetFloat(violenceSpreadDamageRadiusId, radius);

                    // TODO: additional render to texture
                    // activate needed object
                    // render
                    // perform shader's manipulations
                    // deactivate object
                    
                    GameObject maskClone = violenceMasksClones[i];
                    
                    maskClone.SetActive(true);
                    
                    masksCamera.Render();
                    //Graphics.Blit(masksRenderTexture, temporalMaskRenderTexture, violenceSpreadMaterial);
                    Graphics.Blit(masksRenderTexture, masks[i], blendMaterial);
                    
                    maskClone.SetActive(false);
                    
                }
            }
        }


        private void InitializeViolenceLayers()
        {
            Material thisMaterial = renderer.material;

            temporalMaskRenderTexture = CreateMaskRenderTexture();
            ClearMaskRenderTexture(temporalMaskRenderTexture);
            
            int count = violenceLayers.Length;
            violenceMasksClones = new GameObject[count];
            masks = new RenderTexture[count];
            
            for (int i = 0; i < count; i++)
            {
                ViolenceLayer layer = violenceLayers[i];
                GameObject maskClone = CreateMaskClone(layer.maskSpreadMaterial);
                maskClone.SetActive(false);
                ClearMaskClone(maskClone);

                violenceMasksClones[i] = maskClone;
                masks[i] = CreateMaskRenderTexture();
                ClearMaskRenderTexture(masks[i]);
                thisMaterial.SetTexture(layer.shaderMaskName, masks[i]);
            }

            masksRenderTexture = CreateMaskRenderTexture();
            if (violenceLayers.Length > 0)
                masksCamera = CreateMasksCamera(violenceMasksClones[0]);
        }


        private GameObject CreateMaskClone(Material maskSpreadMaterial)
        {
            // copy object
            GameObject cloneGameObject = Instantiate(gameObject);
            cloneGameObject.layer = maskRenderLayer;
            // setup material
            Renderer maskRenderer = GetRendererComponent(cloneGameObject);
            maskRenderer.materials = new[] {maskSpreadMaterial};

            ClearMaskClone(cloneGameObject);
            return cloneGameObject;
        }

        private void ClearMaskClone(GameObject clone)
        {
            Component[] components = clone.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (!(component is Transform || component is Renderer || component is MeshFilter))
                    Destroy(component);
            }

            for (int i = 0; i < clone.transform.childCount; i++)
            {
                GameObject childGameObject = clone.transform.GetChild(i).gameObject;
                Renderer childRenderer = GetRendererComponent(childGameObject);
                if (!childRenderer)
                    Destroy(childGameObject);
            }
        }

        private Camera CreateMasksCamera(GameObject oneMaskCloneGameObject)
        {
            Camera result = new GameObject("Violence Mask Camera").AddComponent<Camera>();

            // setup camera's transform 
            Transform cameraTransform = result.transform;
            cameraTransform.parent = oneMaskCloneGameObject.transform;
            cameraTransform.localPosition = Vector3.forward * -10;
            cameraTransform.localRotation = Quaternion.identity;

            // setup camera
            result.orthographic = true;
            result.orthographicSize = 5;
            result.farClipPlane = 15;
            result.cullingMask = 1 << maskRenderLayer;
            result.clearFlags = CameraClearFlags.SolidColor;
            result.backgroundColor = Color.clear;
            result.enabled = false;
            result.useOcclusionCulling = false;

            result.targetTexture = masksRenderTexture;
                
            return result;
        }

        private RenderTexture CreateMaskRenderTexture()
        {
            // TODO: remake CreateMaskRenderTexture()
            return new RenderTexture(violenceTextureResolution, violenceTextureResolution, 0);
        }

        private void ClearMaskRenderTexture(RenderTexture renderTexture)
        {
            RenderTexture currentRenderTexture = RenderTexture.active;
            RenderTexture.active = renderTexture;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = currentRenderTexture;
        }
        

        private Renderer GetRendererComponent(GameObject targetGameObject)
        {
            SkinnedMeshRenderer skinnedMeshRenderer = targetGameObject.GetComponent<SkinnedMeshRenderer>();
            if (skinnedMeshRenderer)
            {
                return skinnedMeshRenderer;
            }

            MeshRenderer meshRenderer = targetGameObject.GetComponent<MeshRenderer>();
            if (meshRenderer)
            {
                return meshRenderer;
            }

            throw new Exception($"No renderer found on {name}");
        }
    }
}