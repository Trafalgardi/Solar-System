using System;
using UnityEngine;

// Original script by Jasper Flick (Catlike Coding)
// https://catlikecoding.com/unity/tutorials/advanced-rendering/fxaa/
// Minor modifications by Sebastian Lague

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[CreateAssetMenu(menuName = "PostProcessing/FXAA")]
public class FXAAEffect : PostProcessingEffect
{
    private const int luminancePass = 0;
    private const int fxaaPass = 1;

    [NonSerialized] private Material fxaaMaterial;

    public LuminanceMode luminanceSource;

    [Range(min: 0.0312f, max: 0.0833f)] public float contrastThreshold = 0.0312f;

    [Range(min: 0.063f, max: 0.333f)] public float relativeThreshold = 0.063f;

    [Range(min: 0f, max: 1f)] public float subpixelBlending = 1f;

    [HideInInspector] public Shader fxaaShader;

    public bool lowQuality;

    public bool gammaBlending;

    public override void Render(RenderTexture source, RenderTexture destination)
    {
        if (fxaaMaterial == null)
        {
            fxaaMaterial = new Material(fxaaShader);
            fxaaMaterial.hideFlags = HideFlags.HideAndDontSave; //
        }

        fxaaMaterial.SetFloat(name: "_ContrastThreshold", contrastThreshold);
        fxaaMaterial.SetFloat(name: "_RelativeThreshold", relativeThreshold);
        fxaaMaterial.SetFloat(name: "_SubpixelBlending", subpixelBlending);

        if (lowQuality)
        {
            fxaaMaterial.EnableKeyword(keyword: "LOW_QUALITY");
        }
        else
        {
            fxaaMaterial.DisableKeyword(keyword: "LOW_QUALITY");
        }

        if (gammaBlending)
        {
            fxaaMaterial.EnableKeyword(keyword: "GAMMA_BLENDING");
        }
        else
        {
            fxaaMaterial.DisableKeyword(keyword: "GAMMA_BLENDING");
        }

        if (luminanceSource == LuminanceMode.Calculate)
        {
            fxaaMaterial.DisableKeyword(keyword: "LUMINANCE_GREEN");
            var luminanceTex = RenderTexture.GetTemporary(source.width, source.height, depthBuffer: 0, source.format);
            Graphics.Blit(source, luminanceTex, fxaaMaterial, luminancePass);
            Graphics.Blit(luminanceTex, destination, fxaaMaterial, fxaaPass);
            RenderTexture.ReleaseTemporary(luminanceTex);
        }
        else
        {
            if (luminanceSource == LuminanceMode.Green)
            {
                fxaaMaterial.EnableKeyword(keyword: "LUMINANCE_GREEN");
            }
            else
            {
                fxaaMaterial.DisableKeyword(keyword: "LUMINANCE_GREEN");
            }

            Graphics.Blit(source, destination, fxaaMaterial, fxaaPass);
        }
    }

    public enum LuminanceMode
    {
        Alpha,
        Green,
        Calculate,
    }
}