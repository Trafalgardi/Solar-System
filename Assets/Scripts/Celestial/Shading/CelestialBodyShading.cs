using UnityEngine;

/*
    Responsible for the shading of a celestial body.
    This is paired with a specific CelestialBodyShape.
*/

public abstract class CelestialBodyShading : ScriptableObject
{
    private ComputeBuffer shadingBuffer;

    protected Vector4[] cachedShadingData;

    public bool randomize;
    public int seed;

    public Material terrainMaterial;
    public bool hasAtmosphere;
    public AtmosphereSettings atmosphereSettings;
    public bool hasOcean;

    [Range(min: 0, max: 1)] public float oceanLevel;

    public OceanSettings oceanSettings;

    public ComputeShader shadingDataCompute;

    public event System.Action OnSettingChanged;

    // 
    public virtual void Initialize(CelestialBodyShape shape)
    {
    }

    // Generate Vector4[] of shading data. This is stored in mesh uvs and used to help shade the body
    public Vector4[] GenerateShadingData(ComputeBuffer vertexBuffer)
    {
        var numVertices = vertexBuffer.count;
        var shadingData = new Vector4[numVertices];

        if (shadingDataCompute)
        {
            // Set data
            SetShadingDataComputeProperties();

            shadingDataCompute.SetInt(name: "numVertices", numVertices);
            shadingDataCompute.SetBuffer(kernelIndex: 0, name: "vertices", vertexBuffer);

            ComputeHelper.CreateAndSetBuffer<Vector4>(ref shadingBuffer, numVertices, shadingDataCompute,
                nameID: "shadingData");

            // Run
            ComputeHelper.Run(shadingDataCompute, numVertices);

            // Get data
            shadingBuffer.GetData(shadingData);
        }

        cachedShadingData = shadingData;

        return shadingData;
    }

    // Set shading properties on terrain
    public virtual void SetTerrainProperties(Material material, Vector2 heightMinMax, float bodyScale)
    {
    }

    public virtual void SetOceanProperties(Material oceanMaterial)
    {
        if (oceanSettings)
        {
            oceanSettings.SetProperties(oceanMaterial, seed, randomize);
        }
    }

    public virtual void ReleaseBuffers() => ComputeHelper.Release(shadingBuffer);

    public static void TextureFromGradient(ref Texture2D texture, int width, Gradient gradient,
        FilterMode filterMode = FilterMode.Bilinear)
    {
        if (texture == null)
        {
            texture = new Texture2D(width, height: 1);
        }
        else if (texture.width != width)
        {
            texture.Reinitialize(width, height: 1);
        }

        if (gradient == null)
        {
            gradient = new Gradient();

            gradient.SetKeys(
                new[] { new GradientColorKey(Color.black, time: 0), new GradientColorKey(Color.black, time: 1), },
                new[] { new GradientAlphaKey(alpha: 1, time: 0), new GradientAlphaKey(alpha: 1, time: 1), });
        }

        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = filterMode;

        var cols = new Color[width];

        for (var i = 0; i < cols.Length; i++)
        {
            var t = i / (cols.Length - 1f);
            cols[i] = gradient.Evaluate(t);
        }

        texture.SetPixels(cols);
        texture.Apply();
    }

    // Override this to set properties on the shadingDataCompute before it is run
    protected virtual void SetShadingDataComputeProperties()
    {
    }

    protected virtual void OnValidate()
    {
        /*
        Shader activeShader = (shader) ? shader : Shader.Find ("Unlit/Color");
        if (material == null || material.shader != activeShader) {
            if (material == null) {
                material = new Material (activeShader);
            } else {
                material.shader = activeShader;
            }
        }
        */
        if (OnSettingChanged != null)
        {
            OnSettingChanged();
        }
    }
}