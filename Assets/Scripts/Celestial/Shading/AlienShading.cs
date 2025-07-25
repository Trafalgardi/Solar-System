using UnityEngine;

[CreateAssetMenu(menuName = "Celestial Body/Alein/Alien Shading")]
public class AlienShading : CelestialBodyShading
{
    public AlienColours customizedCols;
    public AlienColours randomizedCols;

    [Header(header: "Shading Data")] public SimpleNoiseSettings detailWarpNoise;

    public SimpleNoiseSettings detailNoise;
    public SimpleNoiseSettings largeNoise;
    public SimpleNoiseSettings smallNoise;

    public SimpleNoiseSettings noise2;
    public SimpleNoiseSettings warp2;

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

        noise2.SetComputeValues(shadingDataCompute, random, varSuffix: "_noise2");
        warp2.SetComputeValues(shadingDataCompute, random, varSuffix: "_warp2");
    }

    private void ApplyColours(Material material, AlienColours colours)
    {
        material.SetColor(name: "_ShoreCol", colours.shoreCol);
        material.SetColor(name: "_FlatColA", colours.flatColA1);
        material.SetColor(name: "_FlatColA2", colours.flatColA2);
        material.SetColor(name: "_FlatColB", colours.flatColB1);
        material.SetColor(name: "_FlatColB2", colours.flatColB2);
        material.SetColor(name: "_SteepColA", colours.steepColA);
        material.SetColor(name: "_SteepColB", colours.steepColB);
    }

    private void SetRandomColours(Material material)
    {
        var random = new PRNG(seed);
        randomizedCols.shoreCol = ColourHelper.Random(random, satMin: 0.3f, satMax: 0.7f, valMin: 0.4f, valMax: 0.8f);
        randomizedCols.flatColA1 = ColourHelper.RandomSimilar(random, randomizedCols.shoreCol);
        randomizedCols.flatColB1 = ColourHelper.RandomSimilar(random, randomizedCols.flatColA1);

        randomizedCols.flatColA2 = ColourHelper.RandomSimilar(random, randomizedCols.flatColA1);
        randomizedCols.flatColB2 = ColourHelper.RandomSimilar(random, randomizedCols.flatColA2);

        randomizedCols.steepColA = ColourHelper.Random(random, satMin: 0.3f, satMax: 0.7f, valMin: 0.2f, valMax: 0.85f);
        randomizedCols.steepColB = ColourHelper.RandomSimilar(random, randomizedCols.steepColA);
    }

    [System.Serializable]
    public struct AlienColours
    {
        public Color shoreCol;
        public Color flatColA1;
        public Color flatColA2;
        public Color flatColB1;
        public Color flatColB2;
        public Color steepColA;
        public Color steepColB;
    }
}