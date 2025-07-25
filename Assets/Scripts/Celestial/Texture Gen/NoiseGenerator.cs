using UnityEngine;

[CreateAssetMenu(menuName = "Celestial Body/Textures/Noise")]
public class NoiseGenerator : TextureGenerator
{
    private ComputeBuffer minMaxBuffer;

    public SimpleNoiseSettings noiseSettings;
    public SimpleNoiseSettings warpNoiseSettings;

    [Range(min: 0, max: 1)] public float valueFloor;

    public bool normalize;

    protected override void Run()
    {
        var prng = new PRNG(seed);
        var offset = new Vector4(prng.Value(), prng.Value(), prng.Value(), prng.Value()) * 10;

        ComputeHelper.CreateStructuredBuffer<int>(ref minMaxBuffer, count: 2);
        minMaxBuffer.SetData(new[] { int.MaxValue, 0, });
        compute.SetBuffer(kernelIndex: 0, name: "minMax", minMaxBuffer);
        compute.SetBuffer(kernelIndex: 1, name: "minMax", minMaxBuffer);

        var threadGroupSize = ComputeHelper.GetThreadGroupSizes(compute, kernelIndex: 0).x;
        var numThreadGroups = Mathf.CeilToInt((float)textureSize / threadGroupSize);
        compute.SetVector(name: "offset", offset);
        compute.SetTexture(kernelIndex: 0, name: "Result", renderTexture);
        noiseSettings.SetComputeValues(compute, prng, varSuffix: "_simple");
        warpNoiseSettings.SetComputeValues(compute, prng, varSuffix: "_warp");

        compute.SetInt(name: "resolution", (int)textureSize);
        compute.SetFloat(name: "valueFloor", valueFloor);
        compute.Dispatch(kernelIndex: 0, numThreadGroups, numThreadGroups, threadGroupsZ: 1);

        // Normalize
        if (normalize)
        {
            compute.SetTexture(kernelIndex: 1, name: "Result", renderTexture);
            compute.Dispatch(kernelIndex: 1, numThreadGroups, numThreadGroups, threadGroupsZ: 1);
        }

        ComputeHelper.Release(minMaxBuffer);
    }
}