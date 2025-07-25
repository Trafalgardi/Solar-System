using UnityEngine;

[CreateAssetMenu(menuName = "Celestial Body/Earth-Like/Earth Shading")]
public class EarthShading : CelestialBodyShading
{
    public EarthColours customizedCols;
    public EarthColours randomizedCols;

    [Header(header: "Shading Data")] public SimpleNoiseSettings detailWarpNoise;

    public SimpleNoiseSettings detailNoise;
    public SimpleNoiseSettings largeNoise;
    public SimpleNoiseSettings smallNoise;

    public override void SetTerrainProperties(Material material, Vector2 heightMinMax, float bodyScale)
    {
        material.SetVector(name: "heightMinMax", heightMinMax);
        material.SetFloat(name: "oceanLevel", oceanLevel);
        material.SetFloat(name: "bodyScale", bodyScale);

        if (randomize)
        {
            SetRandomColours(material);
            ApplyColours(material, randomizedCols);
        }
        else
        {
            ApplyColours(material, customizedCols);
        }
    }

    protected override void SetShadingDataComputeProperties()
    {
        var random = new PRNG(seed);
        detailNoise.SetComputeValues(shadingDataCompute, random, varSuffix: "_detail");
        detailWarpNoise.SetComputeValues(shadingDataCompute, random, varSuffix: "_detailWarp");
        largeNoise.SetComputeValues(shadingDataCompute, random, varSuffix: "_large");
        smallNoise.SetComputeValues(shadingDataCompute, random, varSuffix: "_small");
    }

    private void ApplyColours(Material material, EarthColours colours)
    {
        material.SetColor(name: "_ShoreLow", colours.shoreColLow);
        material.SetColor(name: "_ShoreHigh", colours.shoreColHigh);

        material.SetColor(name: "_FlatLowA", colours.flatColLowA);
        material.SetColor(name: "_FlatHighA", colours.flatColHighA);

        material.SetColor(name: "_FlatLowB", colours.flatColLowB);
        material.SetColor(name: "_FlatHighB", colours.flatColHighB);

        material.SetColor(name: "_SteepLow", colours.steepLow);
        material.SetColor(name: "_SteepHigh", colours.steepHigh);
    }

    private void SetRandomColours(Material material)
    {
        var random = new PRNG(seed);

        //randomizedCols.shoreCol = ColourHelper.Random (random, 0.3f, 0.7f, 0.4f, 0.8f);
        randomizedCols.flatColLowA =
            ColourHelper.Random(random, satMin: 0.45f, satMax: 0.6f, valMin: 0.7f, valMax: 0.8f);

        randomizedCols.flatColHighA = ColourHelper.TweakHSV(randomizedCols.flatColLowA,
            random.SignedValue() * 0.2f,
            random.SignedValue() * 0.15f,
            random.Range(min: -0.25f, max: -0.2f));

        randomizedCols.flatColLowB =
            ColourHelper.Random(random, satMin: 0.45f, satMax: 0.6f, valMin: 0.7f, valMax: 0.8f);

        randomizedCols.flatColHighB = ColourHelper.TweakHSV(randomizedCols.flatColLowB,
            random.SignedValue() * 0.2f,
            random.SignedValue() * 0.15f,
            random.Range(min: -0.25f, max: -0.2f));

        randomizedCols.shoreColLow = ColourHelper.Random(random, satMin: 0.2f, satMax: 0.3f, valMin: 0.9f, valMax: 1);

        randomizedCols.shoreColHigh = ColourHelper.TweakHSV(randomizedCols.shoreColLow,
            random.SignedValue() * 0.2f,
            random.SignedValue() * 0.2f,
            random.Range(min: -0.3f, max: -0.2f));

        randomizedCols.steepLow = ColourHelper.Random(random, satMin: 0.3f, satMax: 0.7f, valMin: 0.4f, valMax: 0.6f);

        randomizedCols.steepHigh = ColourHelper.TweakHSV(randomizedCols.steepLow,
            random.SignedValue() * 0.2f,
            random.SignedValue() * 0.2f,
            random.Range(min: -0.35f, max: -0.2f));
    }

    [System.Serializable]
    public struct EarthColours
    {
        public Color shoreColLow;
        public Color shoreColHigh;
        public Color flatColLowA;
        public Color flatColHighA;
        public Color flatColLowB;
        public Color flatColHighB;

        public Color steepLow;
        public Color steepHigh;
    }
}