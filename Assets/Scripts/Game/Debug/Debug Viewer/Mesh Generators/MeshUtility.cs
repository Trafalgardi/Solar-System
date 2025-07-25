using System.Collections.Generic;
using UnityEngine;

namespace Visualization.MeshGeneration
{
    public static class MeshUtility
    {
        public static void MeshFromMultipleSources(Mesh mesh, List<Vector3>[] vertexLists, List<int>[] triangleLists)
        {
            var vertices = new List<Vector3>();
            var tris = new List<int>();

            for (var i = 0; i < vertexLists.Length; i++)
            {
                var vertSkipCount = vertices.Count;
                vertices.AddRange(vertexLists[i]);

                for (var triIndex = 0; triIndex < triangleLists[i].Count; triIndex++)
                {
                    tris.Add(triangleLists[i][triIndex] + vertSkipCount);
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(tris, submesh: 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }
    }
}