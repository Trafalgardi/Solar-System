using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Celestial Body/Moon/Moon Shading")]
public class MoonShading : CelestialBodyShading
{
    private ComputeBuffer craterBuffer;
    private ComputeBuffer pointBuffer;
    private MoonShape moonShape;

    public Color primaryColA = Color.white;
    public Color secondaryColA = Color.black;
    public Color primaryColB = Color.white;
    public Color secondaryColB = Color.black;
    public Color steepCol = Color.black;

    [Header(header: "Shading Data")] public int numBiomePoints = 20;

    public Vector2 radiusMinMax = new(x: 0.02f, y: 0.1f);

    public SimpleNoiseSettings biomeWarpNoise;
    public SimpleNoiseSettings detailNoise;
    public SimpleNoiseSettings detailWarpNoise;

    [Header(header: "Rays")]
    [Range(min: 0, max: 1)]
    public float candidatePoolSize = 0.2f;

    public int desiredNumCraterRays = 2;
    public int ejectaRaySeed;
    public float ejectaRaysScale = 10;

    [Header(header: "Normal maps")] public Texture2D[] normalMapsFlat;

    public Texture2D[] normalMapsSteep;

    public override void Initialize(CelestialBodyShape shape)
    {
        base.Initialize(shape);
        moonShape = shape as MoonShape;
    }

    public override void SetTerrainProperties(Material material, Vector2 heightMinMax, float bodyScale)
    {
        material.SetVector(name: "heightMinMax", heightMinMax);
        material.SetFloat(name: "bodyScale", bodyScale);

        var prng = new PRNG(seed);

        if (randomize)
        {
            SetColours(prng, material);

            material.SetFloat(name: "_SmoothnessA", Mathf.Lerp(a: 0f, b: 0.6f, prng.SmallestRandom01(n: 4)));
            material.SetFloat(name: "_SmoothnessB", Mathf.Lerp(a: 0f, b: 0.6f, prng.SmallestRandom01(n: 4)));
            material.SetFloat(name: "_Metallic", Mathf.Lerp(a: 0f, b: 0.5f, prng.SmallestRandom01(n: 3)));

            var randomNormalMapFlat = prng.RandomElement(normalMapsFlat);
            var randomNormalMapSteep = randomNormalMapFlat;
            var loopSafety = 0;

            while (randomNormalMapSteep == randomNormalMapFlat && loopSafety < 20)
            {
                randomNormalMapSteep = prng.RandomElement(normalMapsSteep);
                loopSafety++;
            }

            material.SetTexture(name: "_NormalMapFlat", randomNormalMapFlat);
            material.SetTexture(name: "_NormalMapSteep", randomNormalMapSteep);

            SetBiomeSettings(prng, material);
        }
        else
        {
            material.SetColor(name: "_PrimaryColA", primaryColA);
            material.SetColor(name: "_SecondaryColA", secondaryColA);
            material.SetColor(name: "_PrimaryColB", primaryColB);
            material.SetColor(name: "_SecondaryColB", secondaryColB);
            material.SetColor(name: "_SteepCol", steepCol);
        }

        //
        if (cachedShadingData != null)
        {
            float biomeNoiseSum = 0;

            for (var i = 0; i < cachedShadingData.Length; i++)
            {
                biomeNoiseSum += cachedShadingData[i].w;
            }

            material.SetFloat(name: "_AvgBiomeNoiseDst", biomeNoiseSum / cachedShadingData.Length);
        }
        else
        {
            Debug.LogError(message: "Cached shading noise null");
            material.SetFloat(name: "_AvgBiomeNoiseDst", value: 5);
        }
    }

    public override void ReleaseBuffers()
    {
        base.ReleaseBuffers();
        ComputeHelper.Release(craterBuffer, pointBuffer);
    }

    protected override void SetShadingDataComputeProperties()
    {
        SetCraters(moonShape);
        SetRandomPoints();
        SetShadingNoise();
    }

    protected override void OnValidate() => base.OnValidate();

    private void SetBiomeSettings(PRNG prng, Material material)
    {
        var biomeValues = new Vector4(prng.SignedValueBiasExtremes(biasStrength: 0.3f),
            prng.SignedValueBiasExtremes(biasStrength: 0.3f) * 0.4f,
            prng.SignedValueBiasExtremes(biasStrength: 0.3f) * 0.3f,
            prng.SignedValueBiasCentre(biasStrength: 0.3f) * .7f);

        material.SetVector(name: "_RandomBiomeValues", biomeValues);
        var warpStrength = prng.SignedValueBiasCentre(biasStrength: .65f) * 30;
        material.SetFloat(name: "_BiomeBlendStrength", prng.Range(min: 2f, max: 12) + Mathf.Abs(warpStrength) / 2);
        material.SetFloat(name: "_BiomeWarpStrength", warpStrength);
    }

    private void SetColours(PRNG rand, Material material)
    {
        var colChance = new Chance(rand);
        var primaryA_HSV = Vector3.zero;
        var secondaryA_HSV = Vector3.zero;
        var primaryB_HSV = Vector3.zero;
        var secondaryB_HSV = Vector3.zero;

        // One light grey, one dark grey
        if (colChance.Percent(percent: 25))
        {
            primaryA_HSV = new Vector3(x: 0, y: 0, Mathf.Lerp(a: 0.55f, b: 1, rand.ValueBiasUpper(biasStrength: .4f)));
            secondaryA_HSV = primaryA_HSV + new Vector3(x: 0, y: 0, rand.SignedValueBiasCentre(biasStrength: 0.4f));
            primaryB_HSV = new Vector3(x: 0, y: 0, Mathf.Lerp(a: 0f, b: 0.45f, rand.ValueBiasLower(biasStrength: .4f)));
            secondaryB_HSV = primaryB_HSV + new Vector3(x: 0, y: 0, rand.SignedValueBiasCentre(biasStrength: 0.4f));
        }
        // One colour, one grey
        else if (colChance.Percent(percent: 25))
        {
            // Pick grey, tending towards either very dark or very light
            var greyValue = rand.ValueBiasExtremes(biasStrength: 0.8f);
            primaryA_HSV = new Vector3(x: 0, y: 0, greyValue);
            secondaryA_HSV = new Vector3(x: 0, y: 0, greyValue + rand.SignedValueBiasCentre(biasStrength: 0.5f) * .3f);

            // If grey is dark, use bright colour, otherwise dark colour
            var colourValue = greyValue < 0.5f
                ? rand.ValueBiasUpper(biasStrength: 0.7f)
                : rand.ValueBiasLower(biasStrength: 0.7f);

            primaryB_HSV = new Vector3(rand.Value(), rand.Range(min: 0.2f, max: 0.9f),
                Mathf.Lerp(a: 0.1f, b: 0.9f, colourValue));

            secondaryB_HSV = primaryB_HSV + rand.JiggleVector3(weightX: 0.1f, weightY: 0.2f, weightZ: 0.4f);
        }

        // Two similar colours
        else if (colChance.Percent(percent: 25))
        {
            primaryA_HSV = new Vector3(rand.Range(min: 0, max: 1), rand.Range(min: 0.1f, max: 0.8f),
                rand.Range(min: 0.2f, max: 0.8f));

            secondaryA_HSV = primaryA_HSV + rand.JiggleVector3(weightX: 0.1f, weightY: 0.2f, weightZ: 0.3f);

            primaryB_HSV = new Vector3(primaryA_HSV.x + rand.Range(min: 0.05f, max: 0.1f),
                rand.Range(min: 0.1f, max: 0.8f), rand.Range(min: 0.2f, max: 0.8f));

            secondaryB_HSV = primaryB_HSV + rand.JiggleVector3(weightX: 0.1f, weightY: 0.2f, weightZ: 0.3f);
        }

        // Two distinct colours
        else if (colChance.Percent(percent: 25))
        {
            primaryA_HSV = new Vector3(rand.Value(), rand.Range(min: 0.2f, max: 0.9f),
                rand.Range(min: 0.1f, max: 0.9f));

            secondaryA_HSV = primaryA_HSV + rand.JiggleVector3(weightX: 0.1f, weightY: 0.2f, weightZ: 0.3f);

            primaryB_HSV = new Vector3((primaryA_HSV.x + rand.Range(min: 0.2f, max: 0.8f)) % 1,
                rand.Range(min: 0.2f, max: 0.9f), rand.Range(min: 0.1f, max: 0.9f));

            secondaryB_HSV = primaryB_HSV + rand.JiggleVector3(weightX: 0.1f, weightY: 0.2f, weightZ: 0.3f);
        }

        material.SetColor(name: "_PrimaryColA", HSVToRGB(primaryA_HSV));
        material.SetColor(name: "_SecondaryColA", HSVToRGB(secondaryA_HSV));
        material.SetColor(name: "_PrimaryColB", HSVToRGB(primaryB_HSV));
        material.SetColor(name: "_SecondaryColB", HSVToRGB(secondaryB_HSV));
    }

    private Color GreyscaleColor(float value) => new(value, value, value, a: 1);

    private Color HSVToRGB(Vector3 col)
        => Color.HSVToRGB(Mathf.Clamp01(col.x), Mathf.Clamp01(col.y), Mathf.Clamp01(col.z));

    private void SetShadingNoise()
    {
        const string biomeWarpNoiseSuffix = "_biomeWarp";
        const string detailWarpNoiseSuffix = "_detailWarp";
        const string detailNoiseSuffix = "_detail";

        var prng = new PRNG(seed);
        var prng2 = new PRNG(seed);

        if (randomize)
        {
            // warp 1
            var randomizedBiomeWarpNoise = new SimpleNoiseSettings();
            randomizedBiomeWarpNoise.elevation = prng.Range(min: 0.8f, max: 3f);
            randomizedBiomeWarpNoise.scale = prng.Range(min: 1f, max: 3f);
            randomizedBiomeWarpNoise.SetComputeValues(shadingDataCompute, prng2, biomeWarpNoiseSuffix);

            // warp 2
            var randomizedDetailWarpNoise = new SimpleNoiseSettings();
            randomizedDetailWarpNoise.scale = prng.Range(min: 1f, max: 3f);
            randomizedDetailWarpNoise.elevation = prng.Range(min: 1f, max: 5f);
            randomizedDetailWarpNoise.SetComputeValues(shadingDataCompute, prng2, detailWarpNoiseSuffix);

            detailNoise.SetComputeValues(shadingDataCompute, prng2, detailNoiseSuffix);
        }
        else
        {
            biomeWarpNoise.SetComputeValues(shadingDataCompute, prng2, biomeWarpNoiseSuffix);
            detailWarpNoise.SetComputeValues(shadingDataCompute, prng2, detailWarpNoiseSuffix);
            detailNoise.SetComputeValues(shadingDataCompute, prng2, detailNoiseSuffix);
        }
    }

    private void SetRandomPoints()
    {
        Random.InitState(seed);

        var randomizedNumPoints = numBiomePoints;

        if (randomize)
        {
            randomizedNumPoints = Random.Range(minInclusive: 15, maxExclusive: 50);
        }

        Random.InitState(seed);
        var randomPoints = new Vector4[randomizedNumPoints];

        for (var i = 0; i < randomPoints.Length; i++)
        {
            var point = Random.onUnitSphere;
            var radius = Mathf.Lerp(radiusMinMax.x, radiusMinMax.y, Random.value);
            randomPoints[i] = new Vector4(point.x, point.y, point.z, radius);
        }

        ComputeHelper.CreateAndSetBuffer(ref pointBuffer, randomPoints, shadingDataCompute, nameID: "points");
        shadingDataCompute.SetInt(name: "numRandomPoints", randomPoints.Length);
    }

    // Pick craters to be shaded with radial streaks emanating from them 
    private void SetCraters(MoonShape moonShape)
    {
        var random = new PRNG(ejectaRaySeed);
        //int desiredNumCraterRays = random.Range (5, 15);
        //desiredNumCraterRays = 2;

        // Sort craters from largest to smallest
        var sortedCraters = new List<CraterSettings.Crater>(moonShape.craterSettings.cachedCraters);
        sortedCraters.Sort((a, b) => b.size.CompareTo(a.size));
        var poolSize = Mathf.Clamp((int)((sortedCraters.Count - 1) * candidatePoolSize), min: 1, sortedCraters.Count);
        sortedCraters = sortedCraters.GetRange(index: 0, poolSize);
        random.Shuffle(sortedCraters);

        // Choose craters
        var chosenCraters = new List<CraterSettings.Crater>();

        for (var i = 0; i < sortedCraters.Count; i++)
        {
            var currentCrater = sortedCraters[i];

            // Reject those which are too close to already chosen craters as the textures may not overlap
            var overlapsOtherEjecta = false;

            for (var j = 0; j < chosenCraters.Count; j++)
            {
                var dst = (currentCrater.centre - chosenCraters[j].centre).magnitude;
                var ejectaRadiusSum = (currentCrater.size + chosenCraters[j].size) * ejectaRaysScale / 2;

                if (dst < ejectaRadiusSum)
                {
                    overlapsOtherEjecta = true;

                    break;
                }
            }

            //Debug.DrawRay (currentCrater.centre, currentCrater.centre * 0.2f, (overlapsOtherEjecta) ? Color.red : Color.green);
            if (!overlapsOtherEjecta)
            {
                chosenCraters.Add(currentCrater);
            }

            if (chosenCraters.Count >= desiredNumCraterRays)
            {
                break;
            }
        }

        // Set
        var ejectaCraters = new Vector4[chosenCraters.Count];

        for (var i = 0; i < chosenCraters.Count; i++)
        {
            var crater = chosenCraters[i];

            ejectaCraters[i] = new Vector4(crater.centre.x, crater.centre.y, crater.centre.z,
                crater.size * ejectaRaysScale);
            //CustomDebug.DrawSphere (crater.centre, crater.size * ejectaRaysScale / 2, Color.yellow);
        }

        ComputeHelper.CreateAndSetBuffer(ref craterBuffer, ejectaCraters, shadingDataCompute, nameID: "ejectaCraters");
        shadingDataCompute.SetInt(name: "numEjectaCraters", chosenCraters.Count);
    }
}