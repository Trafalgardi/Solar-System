using UnityEngine;

namespace Visualization.MeshGeneration
{
    public static class RingMesh
    {
        private const int resolution = 50;

        public static void GenerateMesh(Mesh mesh, float angle, float innerRadius, float outerRadius)
        {
            // Validate input:
            angle = Mathf.Clamp(angle, min: -360, max: 360);
            innerRadius = Mathf.Max(a: 0, innerRadius);
            outerRadius = Mathf.Max(a: 0, outerRadius);

            if (outerRadius < innerRadius)
            {
                var temp = outerRadius;
                outerRadius = innerRadius;
                innerRadius = temp;
            }

            var numIncrements = (int)Mathf.Max(a: 5, resolution * Mathf.Abs(angle) / 360);

            var angleIncrement = angle / (numIncrements - 1f);
            var verts = new Vector3[numIncrements * 2];
            var norms = new Vector3[numIncrements * 2];
            var tris = new int[(numIncrements - 1) * 2 * 3];

            for (var i = 0; i < numIncrements; i++)
            {
                var currAngle = angleIncrement * i * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Sin(currAngle), y: 0, Mathf.Cos(currAngle));
                var pos = dir * outerRadius;
                var posInner = dir * innerRadius;
                // If angle < 0 then reverse verts so that triangles still wind the right way
                verts[i * 2] = angle > 0 ? posInner : pos;
                verts[i * 2 + 1] = angle > 0 ? pos : posInner;
                norms[i * 2] = Vector3.up;
                norms[i * 2 + 1] = Vector3.up;

                if (i < numIncrements - 1)
                {
                    tris[i * 6] = i * 2;
                    tris[i * 6 + 1] = i * 2 + 1;
                    tris[i * 6 + 2] = i * 2 + 2;

                    tris[i * 6 + 3] = i * 2 + 2;
                    tris[i * 6 + 4] = i * 2 + 1;
                    tris[i * 6 + 5] = i * 2 + 3;
                }
            }

            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.normals = norms;
        }
    }
}