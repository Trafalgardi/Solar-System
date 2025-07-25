using UnityEngine;

[CreateAssetMenu(menuName = "Celestial Body/Ocean")]
public class OceanSettings : ScriptableObject
{
    public float depthMultiplier = 10;
    public float alphaMultiplier = 70;
    public Color colA;
    public Color colB;
    public Color specularCol = Color.white;

    [Header(header: "Waves")] public Texture2D waveNormalA;

    public Texture2D waveNormalB;

    [Range(min: 0, max: 1)] public float waveStrength = 0.15f;

    public float waveScale = 15;
    public float waveSpeed = 0.5f;

    //[Header("")]
    [Range(min: 0, max: 1)] public float smoothness = 0.92f;

    public Vector4 testParams;

    public void SetProperties(Material material, int seed, bool randomize)
    {
        material.SetFloat(name: "depthMultiplier", depthMultiplier);
        material.SetFloat(name: "alphaMultiplier", alphaMultiplier);

        material.SetTexture(name: "waveNormalA", waveNormalA);
        material.SetTexture(name: "waveNormalB", waveNormalB);
        material.SetFloat(name: "waveStrength", waveStrength);
        material.SetFloat(name: "waveNormalScale", waveScale);
        material.SetFloat(name: "waveSpeed", waveSpeed);
        material.SetFloat(name: "smoothness", smoothness);
        material.SetVector(name: "params", testParams);

        if (randomize)
        {
            var random = new PRNG(seed);

            var randomColA = Color.HSVToRGB(random.Value(), random.Range(min: 0.6f, max: 0.8f),
                random.Range(min: 0.65f, max: 1));

            var randomColB = ColourHelper.TweakHSV(randomColA,
                random.SignedValue() * 0.2f,
                random.SignedValue() * 0.2f,
                random.Range(min: -0.5f, max: -0.4f));

            material.SetColor(name: "colA", randomColA);
            material.SetColor(name: "colB", randomColB);
            material.SetColor(name: "specularCol", Color.white);
        }
        else
        {
            material.SetColor(name: "colA", colA);
            material.SetColor(name: "colB", colB);
            material.SetColor(name: "specularCol", specularCol);
        }
    }
}