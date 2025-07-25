using UnityEngine;

public class CustomImageEffect : MonoBehaviour
{
    protected Material material;

    public bool active;
    public Shader shader;

    public virtual Material GetMaterial()
    {
        if (material == null || material.shader != shader)
        {
            material = new Material(shader);
        }

        return material;
    }

    public virtual void Release()
    {
    }
}