using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
public class CustomPostProcessing : MonoBehaviour
{
    private Shader defaultShader;
    private Material defaultMat;
    private readonly List<RenderTexture> temporaryTextures = new();

    public PostProcessingEffect[] effects;
    public bool debugOceanMask;

    public event System.Action<RenderTexture> onPostProcessingComplete;

    public event System.Action<RenderTexture> onPostProcessingBegin;

    // Helper function for blitting a list of materials
    public static void RenderMaterials(RenderTexture source, RenderTexture destination, List<Material> materials)
    {
        var temporaryTextures = new List<RenderTexture>();

        var currentSource = source;
        RenderTexture currentDestination = null;

        if (materials != null)
        {
            for (var i = 0; i < materials.Count; i++)
            {
                var material = materials[i];

                if (material != null)
                {
                    if (i == materials.Count - 1)
                    {
                        // last material
                        currentDestination = destination;
                    }
                    else
                    {
                        // get temporary texture to render this effect into
                        currentDestination = TemporaryRenderTexture(destination);
                        temporaryTextures.Add(currentDestination);
                    }

                    Graphics.Blit(currentSource, currentDestination, material);
                    currentSource = currentDestination;
                }
            }
        }

        // In case dest texture was not rendered into (due to being provided a null material), copy current src to dest
        if (currentDestination != destination)
        {
            Graphics.Blit(currentSource, destination, new Material(Shader.Find(name: "Unlit/Texture")));
        }

        // Release temporary textures
        for (var i = 0; i < temporaryTextures.Count; i++)
        {
            RenderTexture.ReleaseTemporary(temporaryTextures[i]);
        }
    }

    public static RenderTexture TemporaryRenderTexture(RenderTexture template)
        => RenderTexture.GetTemporary(template.descriptor);

    private void Init()
    {
        if (defaultShader == null)
        {
            defaultShader = Shader.Find(name: "Unlit/Texture");
        }

        defaultMat = new Material(defaultShader);
    }

    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture intialSource, RenderTexture finalDestination)
    {
        if (onPostProcessingBegin != null)
        {
            onPostProcessingBegin(finalDestination);
        }

        Init();

        temporaryTextures.Clear();

        var currentSource = intialSource;
        RenderTexture currentDestination = null;

        if (effects != null)
        {
            for (var i = 0; i < effects.Length; i++)
            {
                var effect = effects[i];

                if (effect != null)
                {
                    if (i == effects.Length - 1)
                    {
                        // Final effect, so render into final destination texture
                        currentDestination = finalDestination;
                    }
                    else
                    {
                        // Get temporary texture to render this effect into
                        currentDestination = TemporaryRenderTexture(finalDestination);
                        temporaryTextures.Add(currentDestination); //
                    }

                    effect.Render(currentSource, currentDestination); // render the effect
                    currentSource = currentDestination; // output texture of this effect becomes input for next effect
                }
            }
        }

        // In case dest texture was not rendered into (due to being provided a null effect), copy current src to dest
        if (currentDestination != finalDestination)
        {
            Graphics.Blit(currentSource, finalDestination, defaultMat);
        }

        // Release temporary textures
        for (var i = 0; i < temporaryTextures.Count; i++)
        {
            RenderTexture.ReleaseTemporary(temporaryTextures[i]);
        }

        if (debugOceanMask)
        {
            Graphics.Blit(FindObjectOfType<OceanMaskRenderer>().oceanMaskTexture, finalDestination, defaultMat);
        }

        // Trigger post processing complete event
        if (onPostProcessingComplete != null)
        {
            onPostProcessingComplete(finalDestination);
        }
    }
}