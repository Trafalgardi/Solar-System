using UnityEngine;

[CreateAssetMenu(menuName = "Celestial Body/Earth-Like/Earth Shape")]
public class EarthShape : CelestialBodyShape
{
    [Header(header: "Continent settings")] public float oceanDepthMultiplier = 5;

    public float oceanFloorDepth = 1.5f;
    public float oceanFloorSmoothing = 0.5f;

    public float mountainBlend = 1.2f; // Determines how smoothly the base of mountains blends into the terrain

    [Header(header: "Noise settings")] public SimpleNoiseSettings continentNoise;

    public SimpleNoiseSettings maskNoise;

    public RidgeNoiseSettings ridgeNoise;
    public Vector4 testParams;

    protected override void SetShapeData()
    {
        var prng = new PRNG(seed);
        continentNoise.SetComputeValues(heightMapCompute, prng, varSuffix: "_continents");
        ridgeNoise.SetComputeValues(heightMapCompute, prng, varSuffix: "_mountains");
        maskNoise.SetComputeValues(heightMapCompute, prng, varSuffix: "_mask");

        heightMapCompute.SetFloat(name: "oceanDepthMultiplier", oceanDepthMultiplier);
        heightMapCompute.SetFloat(name: "oceanFloorDepth", oceanFloorDepth);
        heightMapCompute.SetFloat(name: "oceanFloorSmoothing", oceanFloorSmoothing);
        heightMapCompute.SetFloat(name: "mountainBlend", mountainBlend);
        heightMapCompute.SetVector(name: "params", testParams);

        //
    }
}