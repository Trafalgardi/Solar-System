using UnityEngine;

[CreateAssetMenu(menuName = "Celestial Body/Shapes/Shattered")]
public class ShatteredShape : CelestialBodyShape
{
    [Header(header: "Noise settings")] public float plateauHeight;

    public float plateauSmoothing;
    public SimpleNoiseSettings continentNoise;
    public SimpleNoiseSettings warpNoise;
    public RidgeNoiseSettings ridgeNoise;
    public RidgeNoiseSettings ridgeNoise2;
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
        ridgeNoise2.SetComputeValues(heightMapCompute, prng, varSuffix: "_mountains2");
        warpNoise.SetComputeValues(heightMapCompute, prng, varSuffix: "_warp");
        craterSettings.SetComputeValues(heightMapCompute, seed);

        heightMapCompute.SetFloat(name: "plateauHeight", plateauHeight);
        heightMapCompute.SetFloat(name: "plateauSmoothing", plateauSmoothing);
        heightMapCompute.SetVector(name: "params", testParams);
    }
}