using UnityEngine;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
public class Atmosphere : CustomImageEffect
{
    private ComputeBuffer buffer;
    private Texture2D falloffTex;

    public CelestialBodyGenerator planet;

    [Range(min: 0, max: 1)] public float atmosphereScale = 0.2f;

    public Color color;
    public Vector4 testParams;
    public Gradient falloff;
    public int gradientRes = 10;
    public int numSteps = 10;
    public Texture2D blueNoise;

    public override Material GetMaterial()
    {
        // Validate inputs
        if (material == null || material.shader != shader)
        {
            if (shader == null)
            {
                shader = Shader.Find(name: "Unlit/Texture");
            }

            material = new Material(shader);
        }

        // Set
        var sphere = new Sphere
        {
            centre = planet.transform.position,
            radius = (1 + atmosphereScale) * planet.BodyScale,
            waterRadius = planet.GetOceanRadius(),
        };

        buffer = new ComputeBuffer(count: 1, Sphere.Size);
        buffer.SetData(new[] { sphere, });
        material.SetBuffer(name: "spheres", buffer);
        material.SetVector(name: "params", testParams);
        material.SetColor(name: "_Color", color);
        material.SetFloat(name: "planetRadius", planet.BodyScale);

        CelestialBodyShading.TextureFromGradient(ref falloffTex, gradientRes, falloff);
        material.SetTexture(name: "_Falloff", falloffTex);
        material.SetTexture(name: "_BlueNoise", blueNoise);
        material.SetInt(name: "numSteps", numSteps);

        return material;
    }

    public override void Release() => buffer.Release();

    public struct Sphere
    {
        public Vector3 centre;
        public float radius;
        public float waterRadius;

        public static int Size => sizeof(float) * 5;
    }
}