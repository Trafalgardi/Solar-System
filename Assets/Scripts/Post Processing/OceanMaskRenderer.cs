using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class OceanMaskRenderer : MonoBehaviour
{
    private CelestialBodyGenerator[] oceanBodies;
    private RenderTexture prev;

    public Shader oceanMaskShader;

    [HideInInspector] public RenderTexture oceanMaskTexture;

    private void Update() => Init();

    private void Init()
    {
        if (!Application.isPlaying || oceanBodies == null)
        {
            var allBodies = FindObjectsOfType<CelestialBodyGenerator>();
            var oceanBodiesList = new List<CelestialBodyGenerator>();

            for (var i = 0; i < allBodies.Length; i++)
            {
                if (allBodies[i].body.shading.hasOcean && allBodies[i].body.shading.oceanSettings != null)
                {
                    oceanBodiesList.Add(allBodies[i]);
                }
            }

            oceanBodies = oceanBodiesList.ToArray();
            FindObjectOfType<CustomPostProcessing>().onPostProcessingBegin -= RenderOceanMask;
            FindObjectOfType<CustomPostProcessing>().onPostProcessingBegin += RenderOceanMask;
        }
    }

    private void RenderOceanMask(RenderTexture screenTex)
    {
        Init();

        if (prev != null)
        {
            prev.Release();
            prev = null;
        }

        if (oceanMaskTexture == null ||
            oceanMaskTexture.width != screenTex.width ||
            oceanMaskTexture.height != screenTex.height)
        {
            if (oceanMaskTexture != null)
            {
                prev = oceanMaskTexture;
            }

            oceanMaskTexture = new RenderTexture(screenTex);
        }

        oceanMaskTexture.Create();

        if (oceanBodies != null && oceanBodies.Length > 0)
        {
            var mat = new Material(oceanMaskShader);

            var oceanSpheres = new Vector4[oceanBodies.Length];

            for (var i = 0; i < oceanBodies.Length; i++)
            {
                var pos = oceanBodies[i].transform.position;
                var oceanRadius = oceanBodies[i].GetOceanRadius();
                oceanSpheres[i] = new Vector4(pos.x, pos.y, pos.z, oceanRadius);
            }

            mat.SetInt(name: "numSpheres", oceanSpheres.Length);
            mat.SetVectorArray(name: "spheres", oceanSpheres);
            //ComputeHelper.Run (oceanMaskCompute, width, height);

            Graphics.Blit(screenTex, oceanMaskTexture, mat);
        }
    }

    private void OnDestroy()
    {
        if (oceanMaskTexture != null)
        {
            oceanMaskTexture.Release();
        }
    }
}