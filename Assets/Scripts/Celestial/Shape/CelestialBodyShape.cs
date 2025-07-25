using UnityEngine;

public abstract class CelestialBodyShape : ScriptableObject
{
    private ComputeBuffer heightBuffer;

    public bool randomize;
    public int seed;
    public ComputeShader heightMapCompute;

    public bool perturbVertices;
    public ComputeShader perturbCompute;

    [Range(min: 0, max: 1)] public float perturbStrength = 0.7f;

    public event System.Action OnSettingChanged;

    public virtual float[] CalculateHeights(ComputeBuffer vertexBuffer)
    {
        //Debug.Log (System.Environment.StackTrace);
        // Set data
        SetShapeData();
        heightMapCompute.SetInt(name: "numVertices", vertexBuffer.count);
        heightMapCompute.SetBuffer(kernelIndex: 0, name: "vertices", vertexBuffer);

        ComputeHelper.CreateAndSetBuffer<float>(ref heightBuffer, vertexBuffer.count, heightMapCompute,
            nameID: "heights");

        // Run
        ComputeHelper.Run(heightMapCompute, vertexBuffer.count);

        // Get heights
        var heights = new float[vertexBuffer.count];
        heightBuffer.GetData(heights);

        return heights;
    }

    public virtual void ReleaseBuffers() => ComputeHelper.Release(heightBuffer);

    protected virtual void SetShapeData()
    {
    }

    protected virtual void OnValidate()
    {
        if (OnSettingChanged != null)
        {
            OnSettingChanged();
        }
    }
}