using UnityEngine;
using static UnityEngine.Vector3;

[ExecuteInEditMode]
public class RandomTest : MonoBehaviour
{
    public int numSamples = 10;
    public int numBuckets = 10;
    public int weightStrength = 1;
    public PRNG.Weight weight;
    public int seed;
    public float width = 0.1f;
    public float heightScale = 1;
    public bool flatBottom;
    public bool useEquationFuncs;

    [Range(min: 0, max: 1)] public float biasStrength;

    private void Update()
    {
        var buckets = new int[numBuckets];
        var random = new PRNG(seed);

        for (var i = 0; i < numSamples; i++)
        {
            float value = 0;

            if (useEquationFuncs)
            {
                switch (weight)
                {
                    case PRNG.Weight.Ends:   value = random.ValueBiasExtremes(biasStrength); break;
                    case PRNG.Weight.Lower:  value = random.ValueBiasLower(biasStrength); break;
                    case PRNG.Weight.Upper:  value = random.ValueBiasUpper(biasStrength); break;
                    case PRNG.Weight.Centre: value = random.ValueBiasCentre(biasStrength); break;
                }
            }
            else
            {
                value = random.WeightedValue(weight, weightStrength);
            }

            var bucketIndex = Mathf.Clamp((int)(value * numBuckets), min: 0, numBuckets - 1);
            buckets[bucketIndex]++;
        }

        var maxCount = 0;

        for (var i = 0; i < numBuckets; i++)
        {
            maxCount = Mathf.Max(maxCount, buckets[i]);
        }

        for (var i = 0; i < numBuckets; i++)
        {
            var height = buckets[i] / (float)maxCount * heightScale;
            var w = width / numBuckets;
            var hw = w / 2;
            float v = flatBottom ? 0 : 1;
            var mat = new Material(Shader.Find(name: "Unlit/Color"));
            mat.color = Color.white;
            var mesh = new Mesh();

            mesh.vertices = new[]
            {
                up * height / 2 + left * hw, up * height / 2 + right * hw, down * v * height / 2 + left * hw,
                down * v * height / 2 + right * hw,
            };

            mesh.triangles = new[] { 0, 1, 2, 1, 3, 2, };
            mesh.RecalculateBounds();

            Graphics.DrawMesh(mesh, right * w * i + left * w * (numBuckets - 1) / 2f, Quaternion.identity, mat,
                layer: 0);
        }
    }
}