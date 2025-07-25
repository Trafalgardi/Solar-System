using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
public class DebugLineDrawer : MonoBehaviour
{
    [SerializeField][HideInInspector] private List<Path> paths;
    private Material material;
    private Material defaultMat;
    private int lastFrame;

    public Shader shader;
    public float thickness = 1;
    public Vector3 testParams;

    public void DrawPath(Vector3[] points, Color colour)
    {
        Init();
        var polyLine = new Path { points = points, colour = colour, };
        paths.Add(polyLine);
    }

    private void Init()
    {
        if (paths == null || Time.frameCount != lastFrame)
        {
            lastFrame = Time.frameCount;
            paths = new List<Path>();
        }
    }

    private void DrawDefault(RenderTexture src, RenderTexture dest)
    {
        if (defaultMat == null)
        {
            defaultMat = new Material(Shader.Find(name: "Unlit/Texture"));
        }

        Graphics.Blit(src, dest, defaultMat);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (paths == null || paths.Count == 0)
        {
            DrawDefault(src, dest);

            return;
        }

        if (material == null || material.shader != shader)
        {
            material = new Material(shader);
        }

        var cam = Camera.current;

        var tempTextures = new List<RenderTexture>();

        for (var pathIndex = 0; pathIndex < paths.Count; pathIndex++)
        {
            var path = paths[pathIndex];
            var points2D = new Vector2[path.points.Length];

            for (var i = 0; i < points2D.Length; i++)
            {
                points2D[i] = cam.WorldToViewportPoint(path.points[i]);
            }

            var buffer = new ComputeBuffer(points2D.Length, sizeof(float) * 2);
            buffer.SetData(points2D);
            material.SetBuffer(name: "points", buffer);
            material.SetInt(name: "numPoints", points2D.Length);
            material.SetFloat(name: "thickness", thickness / 1000f);
            material.SetVector(name: "params", testParams);
            material.SetColor(name: "colour", path.colour);

            var isFinalPass = pathIndex == paths.Count - 1;

            var currentDest = dest;

            if (!isFinalPass)
            {
                currentDest = RenderTexture.GetTemporary(src.width, src.height);
                currentDest.name = "Temp texture " + pathIndex;
                tempTextures.Add(currentDest);
            }

            Graphics.Blit(src, currentDest, material);
            src = currentDest;
            buffer.Release();
        }

        foreach (var temp in tempTextures)
        {
            temp.Release();
        }
    }

    [System.Serializable]
    public class Path
    {
        public Vector3[] points;
        public Color colour;
    }
}