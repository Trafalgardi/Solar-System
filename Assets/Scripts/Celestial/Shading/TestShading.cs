using UnityEngine;

[CreateAssetMenu(menuName = "Celestial Body/Shading/Test")]
public class TestShading : CelestialBodyShading
{
    public Color colorA = Color.black;
    public Color colorB = Color.white;
    public Vector2 remapMinMax = new(x: 0, y: 1);

    public override void SetTerrainProperties(Material material, Vector2 heightMinMax, float bodyScale)
    {
        material.SetColor(name: "_ColorA", colorA);
        material.SetColor(name: "_ColorB", colorB);
        material.SetVector(name: "heightMinMax", heightMinMax);
        material.SetVector(name: "remapMinMax", remapMinMax);
    }
}