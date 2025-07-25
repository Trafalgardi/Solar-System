using UnityEngine;

[CreateAssetMenu(menuName = "Celestial Body/Shapes/Alien")]
public class AlienShape : CelestialBodyShape
{
    [Header(header: "Continent settings")] public float oceanDepthMultiplier = 5;

    public float oceanFloorDepth = 1.5f;
    public float oceanFloorSmoothing = 0.5f;

    public float mountainBlend = 1.2f; // Determines how smoothly the base of mountains blends into the terrain

    [Header(header: "Noise settings")] public SimpleNoiseSettings continentNoise;

    public SimpleNoiseSettings maskNoise;
    public SimpleNoiseSettings warpNoise;
    public RidgeNoiseSettings ridgeNoise;
    public CraterSettings craterSettings;
    public Vector4 testParams;

    public override void ReleaseBuffers()
    {
        base.ReleaseBuffers();
        craterSettings.ReleaseBuffers();
    }

    protected override void SetShapeData()
    {
        var prng = new PRNG(seed);
        continentNoise.SetComputeValues(heightMapCompute, prng, varSuffix: "_continents");
        ridgeNoise.SetComputeValues(heightMapCompute, prng, varSuffix: "_mountains");
        maskNoise.SetComputeValues(heightMapCompute, prng, varSuffix: "_mask");
        warpNoise.SetComputeValues(heightMapCompute, prng, varSuffix: "_warp");
        craterSettings.SetComputeValues(heightMapCompute, seed);

        heightMapCompute.SetFloat(name: "oceanDepthMultiplier", oceanDepthMultiplier);
        heightMapCompute.SetFloat(name: "oceanFloorDepth", oceanFloorDepth);
        heightMapCompute.SetFloat(name: "oceanFloorSmoothing", oceanFloorSmoothing);
        heightMapCompute.SetFloat(name: "mountainBlend", mountainBlend);
        heightMapCompute.SetVector(name: "params", testParams);
    }
}