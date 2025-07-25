using UnityEngine;

[ExecuteInEditMode]
public class ColourTest : MonoBehaviour
{
    private Material[] materials;

    public MeshRenderer[] renderers;

    public Vector2 saturationMinMax;
    public Vector2 valueMinMax;
    public int seed;

    public void Random()
    {
        seed = UnityEngine.Random.Range(minInclusive: -1000, maxExclusive: 1000);
        Process();
    }

    private void Update() => Process();

    private void Process()
    {
        if (materials == null || materials.Length != renderers.Length)
        {
            materials = new Material[renderers.Length];
        }

        var random = new PRNG(seed);

        for (var i = 0; i < renderers.Length; i++)
        {
            if (materials[i] == null)
            {
                materials[i] = new Material(Shader.Find(name: "Unlit/Color"));
            }

            var col = ColourHelper.Random(random, saturationMinMax.x, saturationMinMax.y, valueMinMax.x, valueMinMax.y);
            materials[i].color = col;
            renderers[i].sharedMaterial = materials[i];
        }
    }
}