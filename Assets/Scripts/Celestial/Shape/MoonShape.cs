using UnityEngine;

[CreateAssetMenu(menuName = "Celestial Body/Moon/Moon Shape")]
public class MoonShape : CelestialBodyShape
{
    public CraterSettings craterSettings;
    public SimpleNoiseSettings shapeNoise;
    public RidgeNoiseSettings ridgeNoise;
    public RidgeNoiseSettings ridgeNoise2;

    public Vector4 testParams;

    public override void ReleaseBuffers()
    {
        base.ReleaseBuffers();
        craterSettings.ReleaseBuffers();
    }

    protected override void SetShapeData()
    {
        var prng = new PRNG(seed);

        SetCraterSettings(prng, seed, randomize);
        SetShapeNoiseSettings(prng, randomize);
        SetRidgeNoiseSettings(prng, randomize);

        heightMapCompute.SetVector(name: "testParams", testParams);
    }

    private void SetCraterSettings(PRNG prng, int seed, bool randomizeValues)
    {
        if (randomizeValues)
        {
            var chance = new Chance(prng);

            if (chance.Percent(percent: 70))
            {
                // Medium amount of mostly small to medium craters
                craterSettings.SetComputeValues(heightMapCompute, seed, prng.Range(min: 100, max: 700),
                    new Vector2(x: 0.01f, y: 0.1f), sizeDistribution: 0.57f);
            }
            else if (chance.Percent(percent: 15))
            {
                // Many small craters
                craterSettings.SetComputeValues(heightMapCompute, seed, prng.Range(min: 800, max: 1800),
                    new Vector2(x: 0.01f, y: 0.08f), sizeDistribution: 0.74f);
            }
            else if (chance.Percent(percent: 15))
            {
                // A few large craters
                craterSettings.SetComputeValues(heightMapCompute, seed, prng.Range(min: 50, max: 150),
                    new Vector2(x: 0.01f, y: 0.2f), sizeDistribution: 0.4f);
            }
        }
        else
        {
            craterSettings.SetComputeValues(heightMapCompute, seed);
        }
    }

    private void SetShapeNoiseSettings(PRNG prng, bool randomizeValues)
    {
        const string suffix = "_shape";

        if (randomizeValues)
        {
            var chance = new Chance(prng);

            var randomizedShapeNoise = new SimpleNoiseSettings
            {
                numLayers = 4, lacunarity = 2, persistence = 0.5f,
            };

            if (chance.Percent(percent: 80))
            {
                // Minor deformation
                randomizedShapeNoise.elevation = Mathf.Lerp(a: 0.2f, b: 3, prng.ValueBiasLower(biasStrength: 0.3f));
                randomizedShapeNoise.scale = prng.Range(min: 1.5f, max: 2.5f);
            }
            else if (chance.Percent(percent: 20))
            {
                // Major deformation
                randomizedShapeNoise.elevation = Mathf.Lerp(a: 3, b: 8, prng.ValueBiasLower(biasStrength: 0.4f));
                randomizedShapeNoise.scale = prng.Range(min: 0.3f, max: 1);
            }

            // Assign settings
            randomizedShapeNoise.SetComputeValues(heightMapCompute, prng, suffix);
        }
        else
        {
            shapeNoise.SetComputeValues(heightMapCompute, prng, suffix);
        }
    }

    private void SetRidgeNoiseSettings(PRNG prng, bool randomizeValues)
    {
        const string ridgeSuffix = "_ridge";
        const string detailSuffix = "_ridge2";

        if (randomizeValues)
        {
            // Randomize ridge mask
            var randomizedMaskNoise = new SimpleNoiseSettings
            {
                numLayers = 4, lacunarity = 2, persistence = 0.6f, elevation = 1,
            };

            randomizedMaskNoise.scale = prng.Range(min: 0.5f, max: 2f);

            // Randomize ridge noise
            var chance = new Chance(prng);

            var randomizedRidgeNoise = new RidgeNoiseSettings
            {
                numLayers = 4, power = 3, gain = 1, peakSmoothing = 2,
            };

            randomizedRidgeNoise.elevation = Mathf.Lerp(a: 1, b: 7, prng.ValueBiasLower(biasStrength: 0.3f));
            randomizedRidgeNoise.scale = prng.Range(min: 1f, max: 3f);
            randomizedRidgeNoise.lacunarity = prng.Range(min: 1f, max: 5f);
            randomizedRidgeNoise.persistence = 0.42f;
            randomizedRidgeNoise.power = prng.Range(min: 1.5f, max: 3.5f);

            randomizedRidgeNoise.SetComputeValues(heightMapCompute, prng, ridgeSuffix);
            randomizedMaskNoise.SetComputeValues(heightMapCompute, prng, detailSuffix);
        }
        else
        {
            ridgeNoise.SetComputeValues(heightMapCompute, prng, ridgeSuffix);
            ridgeNoise2.SetComputeValues(heightMapCompute, prng, detailSuffix);
        }
    }
}