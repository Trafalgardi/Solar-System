using System.Collections.Generic;
using UnityEngine;

namespace Visualization.MeshGeneration
{
    public static class SphereMesh
    {
        private const int resolution = 20;

        private static readonly Vector3[] directions =
            { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back, };

        public static void GenerateMesh(Mesh mesh)
        {
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var triangles = new List<int>();

            for (var i = 0; i < 6; i++)
            {
                var localUp = directions[i];
                var faceData = ConstructFace(localUp, resolution);

                var numVerts = vertices.Count;
                vertices.AddRange(faceData.vertices);
                normals.AddRange(faceData.normals);

                for (var j = 0; j < faceData.triangles.Length; j++)
                {
                    triangles.Add(faceData.triangles[j] + numVerts);
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, submesh: 0);
        }

        private static FaceData ConstructFace(Vector3 localUp, int resolution)
        {
            var axisA = new Vector3(localUp.y, localUp.z, localUp.x);
            var axisB = Vector3.Cross(localUp, axisA);

            var vertices = new Vector3[resolution * resolution];
            var normals = new Vector3[resolution * resolution];
            var triangles = new int[(resolution - 1) * (resolution - 1) * 6];
            var triIndex = 0;

            for (var y = 0; y < resolution; y++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    var i = x + y * resolution;
                    var percent = new Vector2(x, y) / (resolution - 1);
                    var pointOnUnitCube = localUp + (percent.x - .5f) * 2 * axisA + (percent.y - .5f) * 2 * axisB;
                    var pointOnUnitSphere = pointOnUnitCube.normalized;
                    vertices[i] = pointOnUnitSphere;
                    normals[i] = pointOnUnitSphere;

                    if (x != resolution - 1 && y != resolution - 1)
                    {
                        triangles[triIndex] = i;
                        triangles[triIndex + 1] = i + resolution + 1;
                        triangles[triIndex + 2] = i + resolution;

                        triangles[triIndex + 3] = i;
                        triangles[triIndex + 4] = i + 1;
                        triangles[triIndex + 5] = i + resolution + 1;
                        triIndex += 6;
                    }
                }
            }

            return new FaceData { triangles = triangles, vertices = vertices, normals = normals, };
        }

        public class FaceData
        {
            public int[] triangles;
            public Vector3[] vertices;
            public Vector3[] normals;
        }
    }
}