using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Celestial Body/Textures/Spots")]
public class SpotsTexture : TextureGenerator
{
    [Header(header: "Spot Settings")] public float fadeDst = 1;

    public float smoothing;

    [Range(min: 0, max: 1)] public float background;

    public Layer[] layers;

    public bool blur;
    public ComputeShader blurCompute;

    [Range(min: 1, max: 20)] public int blurSize = 1;

    public bool invertAlpha;
    public float initialRadius;
    public float radiusMultiplier;

    protected override void Run()
    {
        const float dstScale = 0.001f;
        var spots = new List<Vector4>();
        Random.InitState(seed);

        var layerIndex = 0;
        var currentRadius = initialRadius * dstScale;

        foreach (var layer in layers)
        {
            var cellSize = 1f / layer.numCellsPerAxis;
            //float currentRadius = layer.radius * dstScale;
            var alpha = Mathf.Lerp(a: 0, 1 - background, layer.alpha);
            layerIndex++;
            alpha = layerIndex / (float)layers.Length;

            if (invertAlpha)
            {
                alpha = 1 - alpha;
            }

            Debug.Log(alpha + "  " + currentRadius);

            for (var x = 0; x < layer.numCellsPerAxis; x++)
            {
                for (var y = 0; y < layer.numCellsPerAxis; y++)
                {
                    var randomOffset = new Vector2(Random.value, Random.value) * cellSize;
                    var cellCorner = new Vector2(x, y) * cellSize;
                    var pos = cellCorner + randomOffset;

                    var spot = new Vector4(pos.x, pos.y, currentRadius, alpha);
                    spots.Add(spot);

                    // mirror
                    var mirrorX = pos.x < .5f ? pos.x + 1 : pos.x - 1;
                    var mirrorY = pos.y < .5f ? pos.y + 1 : pos.y - 1;
                    var mirrorDstX = Mathf.Min(Mathf.Abs(mirrorX - 1), Mathf.Abs(mirrorX));
                    var mirrorDstY = Mathf.Min(Mathf.Abs(mirrorY - 1), Mathf.Abs(mirrorY));

                    if (mirrorDstX < currentRadius)
                    {
                        spots.Add(new Vector4(mirrorX, pos.y, currentRadius, alpha));
                    }

                    if (mirrorDstY < currentRadius)
                    {
                        spots.Add(new Vector4(pos.x, mirrorY, currentRadius, alpha));
                    }

                    if (mirrorDstX < currentRadius && mirrorDstY < currentRadius)
                    {
                        spots.Add(new Vector4(mirrorX, mirrorY, currentRadius, alpha));
                    }
                }
            }

            currentRadius *= radiusMultiplier;
        }

        if (spots.Count > 0)
        {
            var spotBuffer = ComputeHelper.CreateAndSetBuffer(spots.ToArray(), compute, nameID: "spots");

            compute.SetTexture(kernelIndex: 0, name: "Result", renderTexture);
            compute.SetInt(name: "numSpots", spots.Count);
            compute.SetInt(name: "resolution", renderTexture.width);
            compute.SetFloat(name: "fadeDst", fadeDst * dstScale);
            compute.SetFloat(name: "smoothing", smoothing);
            compute.SetFloat(name: "background", background);
            ComputeHelper.Run(compute, renderTexture.width, renderTexture.height);

            ComputeHelper.Release(spotBuffer);

            if (blur && blurCompute)
            {
                var unblurredTexture = new RenderTexture(renderTexture);
                Graphics.CopyTexture(renderTexture, unblurredTexture);
                blurCompute.SetTexture(kernelIndex: 0, name: "SourceTex", unblurredTexture);
                blurCompute.SetTexture(kernelIndex: 0, name: "Result", renderTexture);
                blurCompute.SetInt(name: "blurSize", blurSize);
                blurCompute.SetInt(name: "textureSize", (int)textureSize);
                ComputeHelper.Run(blurCompute, renderTexture.width, renderTexture.height);
                unblurredTexture.Release();
            }
        }
        else
        {
            Debug.Log(message: "No points set in layers");
        }
    }

    [System.Serializable]
    public class Layer
    {
        public int numCellsPerAxis = 10;
        public float radius = 3;

        [Range(min: 0, max: 1)] public float alpha = 1;
    }
}