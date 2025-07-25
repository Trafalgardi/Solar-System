using UnityEngine;

public abstract class PostProcessingEffect : ScriptableObject
{
    protected Material material;

    public virtual Material GetMaterial() => null;

    public virtual void ReleaseBuffers()
    {
    }

    public abstract void Render(RenderTexture source, RenderTexture destination);
}