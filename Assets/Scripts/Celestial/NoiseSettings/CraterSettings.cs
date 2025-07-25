using UnityEngine;

[System.Serializable]
public class CraterSettings
{
    // Private
    private ComputeBuffer craterBuffer;

    // Exposed settings
    public bool enabled = true;
    public int craterSeed;
    public int numCraters = 400;
    public Vector2 craterSizeMinMax = new(x: 0.01f, y: 0.1f);
    public float rimSteepness = 0.13f;
    public float rimWidth = 1.6f;
    public Vector2 smoothMinMax = new(x: 0.4f, y: 1.5f);

    [Range(min: 0, max: 1)] public float sizeDistribution = 0.6f;

    [HideInInspector] public Crater[] cachedCraters;

    // Set values using exposed settings
    public void SetComputeValues(ComputeShader computeShader, int masterSeed)
        => SetComputeValues(computeShader, masterSeed, numCraters, craterSizeMinMax, sizeDistribution);

    // Set values using custom numCraters
    public void SetComputeValues(ComputeShader computeShader, int masterSeed, int numCraters)
        => SetComputeValues(computeShader, masterSeed, numCraters, craterSizeMinMax, sizeDistribution);

    // Set values using custom numCraters and sizeMinMax
    public void SetComputeValues(ComputeShader computeShader, int masterSeed, int numCraters, Vector2 craterSizeMinMax,
        float sizeDistribution)
    {
        if (!enabled)
        {
            numCraters = 1;
            craterSizeMinMax = Vector2.zero;
        }

        Random.InitState(craterSeed + masterSeed);
        var craters = new Crater[numCraters];
        var prng = new PRNG(masterSeed);

        // Create craters
        for (var i = 0; i < numCraters; i++)
        {
            var t = prng.ValueBiasLower(sizeDistribution);

            var size = Mathf.Lerp(craterSizeMinMax.x, craterSizeMinMax.y, t);
            var floorHeight = Mathf.Lerp(a: -1.2f, b: -0.2f, t + prng.ValueBiasLower(biasStrength: 0.3f));
            var smooth = Mathf.Lerp(smoothMinMax.x, smoothMinMax.y, 1 - t);

            craters[i] = new Crater
                { centre = Random.onUnitSphere, size = size, floorHeight = floorHeight, smoothness = smooth, };
        }

        cachedCraters = craters;

        // Set shape data
        ComputeHelper.CreateAndSetBuffer(ref craterBuffer, craters, computeShader, nameID: "craters");
        computeShader.SetInt(name: "numCraters", numCraters);

        computeShader.SetFloat(nameof(rimSteepness), rimSteepness);
        computeShader.SetFloat(nameof(rimWidth), rimWidth);
        //computeShader.SetFloat (nameof (smoothFactor), smoothFactor);
    }

    public void ReleaseBuffers() => ComputeHelper.Release(craterBuffer);

    [System.Serializable]
    public struct Crater
    {
        public Vector3 centre;
        public float size;
        public float floorHeight;
        public float smoothness;
    }
}