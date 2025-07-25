using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class StarTest : MonoBehaviour
{
    private Mesh mesh;
    private Camera cam;
    private Texture2D spectrum;
    private bool settingsUpdated;
    private OceanMaskRenderer oceanMaskRenderer;

    private void Start() => Init(regenerateMesh: true);

    public int seed;
    public int numStars;
    public int numVertsPerStar = 5;
    public Vector2 sizeMinMax;
    public float minBrightness;
    public float maxBrightness = 1;
    public float dst = 10;
    public float daytimeFade = 4; // higher value means it needs to be darker before stars will appear 
    public Material mat;

    public Gradient colourSpectrum;

    public void Set(RenderTexture screen)
    {
        mat.SetTexture(name: "_MainTex", screen);

        if (oceanMaskRenderer)
        {
            mat.SetTexture(name: "_OceanMask", oceanMaskRenderer.oceanMaskTexture);
        }

        if (Camera.current == cam)
        {
            // ignore in scene view
            transform.position = cam.transform.position;
        }
    }

    private void OnValidate() => settingsUpdated = true;

    private void Update()
    {
        if (!Application.isPlaying)
        {
            Init(settingsUpdated);
            settingsUpdated = false;
        }
    }

    private void Init(bool regenerateMesh)
    {
        if (regenerateMesh)
        {
            GenerateMesh();
        }

        var customPostProcessing = FindObjectOfType<CustomPostProcessing>();
        customPostProcessing.onPostProcessingComplete -= Set;
        customPostProcessing.onPostProcessingComplete += Set;
        cam = customPostProcessing.GetComponent<Camera>();
        TextureHelper.TextureFromGradient(colourSpectrum, width: 64, ref spectrum);
        mat.SetTexture(name: "_Spectrum", spectrum);
        mat.SetFloat(name: "daytimeFade", daytimeFade);

        if (!oceanMaskRenderer)
        {
            oceanMaskRenderer = FindObjectOfType<OceanMaskRenderer>();
        }
    }

    private void GenerateMesh()
    {
        if (mesh)
        {
            mesh.Clear();
        }

        mesh = new Mesh();
        var tris = new List<int>();
        var verts = new List<Vector3>();
        var uvs = new List<Vector2>();

        Random.InitState(seed);

        for (var starIndex = 0; starIndex < numStars; starIndex++)
        {
            var dir = Random.onUnitSphere;
            var (circleVerts, circleTris, circleUvs) = GenerateCircle(dir, verts.Count);
            verts.AddRange(circleVerts);
            tris.AddRange(circleTris);
            uvs.AddRange(circleUvs);
        }

        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, submesh: 0, calculateBounds: true);
        mesh.SetUVs(channel: 0, uvs);
        var meshRenderer = GetComponent<MeshRenderer>();
        GetComponent<MeshFilter>().sharedMesh = mesh;
        meshRenderer.sharedMaterial = mat;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
    }

    private (Vector3[] verts, int[] tris, Vector2[] uvs) GenerateCircle(Vector3 dir, int indexOffset)
    {
        var size = Random.Range(sizeMinMax.x, sizeMinMax.y);
        var brightness = Random.Range(minBrightness, maxBrightness);
        var spectrumT = Random.value;

        var axisA = Vector3.Cross(dir, Vector3.up).normalized;

        if (axisA == Vector3.zero)
        {
            axisA = Vector3.Cross(dir, Vector3.forward).normalized;
        }

        var axisB = Vector3.Cross(dir, axisA);
        var centre = dir * dst;

        var verts = new Vector3[numVertsPerStar + 1];
        var uvs = new Vector2[numVertsPerStar + 1];
        var tris = new int[numVertsPerStar * 3];

        verts[0] = centre;
        uvs[0] = new Vector2(brightness, spectrumT);

        for (var vertIndex = 0; vertIndex < numVertsPerStar; vertIndex++)
        {
            var currAngle = vertIndex / (float)numVertsPerStar * Mathf.PI * 2;
            var vert = centre + (axisA * Mathf.Sin(currAngle) + axisB * Mathf.Cos(currAngle)) * size;
            verts[vertIndex + 1] = vert;
            uvs[vertIndex + 1] = new Vector2(x: 0, spectrumT);

            if (vertIndex < numVertsPerStar)
            {
                tris[vertIndex * 3 + 0] = 0 + indexOffset;
                tris[vertIndex * 3 + 1] = vertIndex + 1 + indexOffset;
                tris[vertIndex * 3 + 2] = (vertIndex + 1) % numVertsPerStar + 1 + indexOffset;
            }
        }

        return (verts, tris, uvs);
    }
}