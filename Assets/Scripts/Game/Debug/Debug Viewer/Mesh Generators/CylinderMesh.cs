using System.Collections.Generic;
using UnityEngine;

namespace Visualization.MeshGeneration
{
    public static class CylinderMesh
    {
        private const int resolution = 20;

        public static void GenerateMesh(Mesh mesh)
        {
            var radius = .5f;

            var bottomVerts = new List<Vector3>();
            var bottomTris = new List<int>();

            var topVerts = new List<Vector3>();
            var topTris = new List<int>();

            var sideVerts = new List<Vector3>();
            var sideTris = new List<int>();

            // Top/bottom face
            var bottomCentre = Vector3.down * .5f;
            var topCentre = Vector3.up * .5f;

            for (var i = 0; i < resolution; i++)
            {
                var angle = i / (float)resolution * Mathf.PI * 2;
                var offset = new Vector3(Mathf.Sin(angle), y: 0, Mathf.Cos(angle)) * radius;
                bottomVerts.Add(bottomCentre + offset);
                bottomTris.AddRange(new[] { resolution, (i + 1) % resolution, i % resolution, });

                topVerts.Add(topCentre + offset);
                topTris.AddRange(new[] { resolution, i % resolution, (i + 1) % resolution, });
            }

            sideVerts.AddRange(bottomVerts);
            sideVerts.AddRange(topVerts);

            bottomVerts.Add(bottomCentre);
            topVerts.Add(topCentre);

            // Sides
            for (var i = 0; i < resolution; i++)
            {
                sideTris.Add(i);
                sideTris.Add((i + 1) % resolution + resolution);
                sideTris.Add(i + resolution);

                sideTris.Add(i);
                sideTris.Add((i + 1) % resolution);
                sideTris.Add((i + 1) % resolution + resolution);
            }

            var allVertLists = new[] { topVerts, bottomVerts, sideVerts, };
            var allTriLists = new[] { topTris, bottomTris, sideTris, };
            MeshUtility.MeshFromMultipleSources(mesh, allVertLists, allTriLists);
        }
    }
}