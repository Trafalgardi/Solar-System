using UnityEngine;
using UnityEngine.Experimental.Rendering;

public abstract class TextureGenerator : ScriptableObject
{
    [SerializeField][HideInInspector] protected RenderTexture renderTexture;

    [Header(header: "Texture Settings")] public string textureName = "Unnamed";

    public TextureSize textureSize = TextureSize.u512;
    public FilterMode filterMode = FilterMode.Bilinear;
    public GraphicsFormat format = GraphicsFormat.R8G8B8A8_UNorm;
    public bool useMips = true;

    [Header(header: "Compute Settings")] public ComputeShader compute;

    public int seed;
    public Vector4 testParams;

    public RenderTexture GenerateTexture()
    {
        var resolution = (int)textureSize;
        CreateTexture(ref renderTexture, resolution, textureName);

        if (compute)
        {
            compute.SetVector(name: "params", testParams);
            Run();

            if (useMips)
            {
                renderTexture.GenerateMips();
            }
        }

        return renderTexture;
    }

    protected abstract void Run();

    private void CreateTexture(ref RenderTexture texture, int resolution, string name)
    {
        if (texture == null ||
            !texture.IsCreated() ||
            texture.width != resolution ||
            texture.height != resolution ||
            texture.useMipMap != useMips ||
            texture.graphicsFormat != format)
        {
            if (texture != null)
            {
                texture.Release();
            }

            texture = new RenderTexture(resolution, resolution, depth: 0);
            texture.graphicsFormat = format;
            texture.enableRandomWrite = true;

            texture.autoGenerateMips = false;
            texture.useMipMap = useMips;
            texture.Create();
        }

        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = filterMode;
        texture.name = name;
    }

    private void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
        }
    }

    public enum TextureSize
    {
        u128 = 128,
        u256 = 256,
        u512 = 512,
        u1024 = 1024,
        u2048 = 2048,
        u4096 = 4096,
    }
}