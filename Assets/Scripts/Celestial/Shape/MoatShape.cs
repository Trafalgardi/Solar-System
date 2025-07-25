using UnityEngine;

[CreateAssetMenu(menuName = "Celestial Body/Moat/Moat Shape")]
public class MoatShape : CelestialBodyShape
{
    [Header(header: "Continent settings")] public float shoreSteepness = 4;

    public float continentLevel = -0.05f;
    public float continentFlatness = 0.27f;

    [Header(header: "Mountain settings")]
    public float mountainSmoothing = 1f; // Reduces excessive jaggedness of mountains in some regions

    public float mountainBlend = 1; // Determines how smoothly the base of mountains blends into the terrain
    public Vector2 maskMinMax = new(x: 0.35f, y: 0.5f);

    [Header(header: "Noise settings")] public SimpleNoiseSettings continentNoise;

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
        craterSettings.SetComputeValues(heightMapCompute, seed);

        heightMapCompute.SetFloat(name: "shoreSteepness", shoreSteepness);
        heightMapCompute.SetFloat(name: "continentFlatness", continentFlatness);
        heightMapCompute.SetFloat(name: "continentLevel", continentLevel);
        heightMapCompute.SetFloat(name: "mountainSmoothing", mountainSmoothing);
        heightMapCompute.SetFloat(name: "mountainBlend", mountainBlend);
        heightMapCompute.SetVector(name: "maskMinMax", maskMinMax);
        heightMapCompute.SetVector(name: "params", testParams);
    }
}