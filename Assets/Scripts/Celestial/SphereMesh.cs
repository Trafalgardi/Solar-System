using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Vector3;

public class SphereMesh
{
    // Internal:
    private readonly FixedSizeList<Vector3> vertices;
    private readonly FixedSizeList<int> triangles;
    private readonly int numDivisions;
    private readonly int numVertsPerFace;

    // Indices of the vertex pairs that make up each of the initial 12 edges
    private static readonly int[] vertexPairs =
        { 0, 1, 0, 2, 0, 3, 0, 4, 1, 2, 2, 3, 3, 4, 4, 1, 5, 1, 5, 2, 5, 3, 5, 4, };

    // Indices of the edge triplets that make up the initial 8 faces
    private static readonly int[] edgeTriplets =
        { 0, 1, 4, 1, 2, 5, 2, 3, 6, 3, 0, 7, 8, 9, 4, 9, 10, 5, 10, 11, 6, 11, 8, 7, };

    // The six initial vertices
    private static readonly Vector3[] baseVertices = { up, left, back, right, forward, down, };

    public SphereMesh(int resolution)
    {
        Resolution = resolution;
        numDivisions = Mathf.Max(a: 0, resolution);
        numVertsPerFace = ((numDivisions + 3) * (numDivisions + 3) - (numDivisions + 3)) / 2;
        var numVerts = numVertsPerFace * 8 - (numDivisions + 2) * 12 + 6;
        var numTrisPerFace = (numDivisions + 1) * (numDivisions + 1);

        vertices = new FixedSizeList<Vector3>(numVerts);
        triangles = new FixedSizeList<int>(numTrisPerFace * 8 * 3);

        vertices.AddRange(baseVertices);

        // Create 12 edges, with n vertices added along them (n = numDivisions)
        var edges = new Edge[12];

        for (var i = 0; i < vertexPairs.Length; i += 2)
        {
            var startVertex = vertices.items[vertexPairs[i]];
            var endVertex = vertices.items[vertexPairs[i + 1]];

            var edgeVertexIndices = new int[numDivisions + 2];
            edgeVertexIndices[0] = vertexPairs[i];

            // Add vertices along edge
            for (var divisionIndex = 0; divisionIndex < numDivisions; divisionIndex++)
            {
                var t = (divisionIndex + 1f) / (numDivisions + 1f);
                edgeVertexIndices[divisionIndex + 1] = vertices.nextIndex;
                vertices.Add(Slerp(startVertex, endVertex, t));
            }

            edgeVertexIndices[numDivisions + 1] = vertexPairs[i + 1];
            var edgeIndex = i / 2;
            edges[edgeIndex] = new Edge(edgeVertexIndices);
        }

        // Create faces
        for (var i = 0; i < edgeTriplets.Length; i += 3)
        {
            var faceIndex = i / 3;
            var reverse = faceIndex >= 4;
            CreateFace(edges[edgeTriplets[i]], edges[edgeTriplets[i + 1]], edges[edgeTriplets[i + 2]], reverse);
        }

        Vertices = vertices.items;
        Triangles = triangles.items;
    }

    public readonly Vector3[] Vertices;
    public readonly int[] Triangles;
    public readonly int Resolution;

    private void CreateFace(Edge sideA, Edge sideB, Edge bottom, bool reverse)
    {
        var numPointsInEdge = sideA.vertexIndices.Length;
        var vertexMap = new FixedSizeList<int>(numVertsPerFace);
        vertexMap.Add(sideA.vertexIndices[0]); // top of triangle

        for (var i = 1; i < numPointsInEdge - 1; i++)
        {
            // Side A vertex
            vertexMap.Add(sideA.vertexIndices[i]);

            // Add vertices between sideA and sideB
            var sideAVertex = vertices.items[sideA.vertexIndices[i]];
            var sideBVertex = vertices.items[sideB.vertexIndices[i]];
            var numInnerPoints = i - 1;

            for (var j = 0; j < numInnerPoints; j++)
            {
                var t = (j + 1f) / (numInnerPoints + 1f);
                vertexMap.Add(vertices.nextIndex);
                vertices.Add(Slerp(sideAVertex, sideBVertex, t));
            }

            // Side B vertex
            vertexMap.Add(sideB.vertexIndices[i]);
        }

        // Add bottom edge vertices
        for (var i = 0; i < numPointsInEdge; i++)
        {
            vertexMap.Add(bottom.vertexIndices[i]);
        }

        // Triangulate
        var numRows = numDivisions + 1;

        for (var row = 0; row < numRows; row++)
        {
            // vertices down left edge follow quadratic sequence: 0, 1, 3, 6, 10, 15...
            // the nth term can be calculated with: (n^2 - n)/2
            var topVertex = ((row + 1) * (row + 1) - row - 1) / 2;
            var bottomVertex = ((row + 2) * (row + 2) - row - 2) / 2;

            var numTrianglesInRow = 1 + 2 * row;

            for (var column = 0; column < numTrianglesInRow; column++)
            {
                int v0, v1, v2;

                if (column % 2 == 0)
                {
                    v0 = topVertex;
                    v1 = bottomVertex + 1;
                    v2 = bottomVertex;
                    topVertex++;
                    bottomVertex++;
                }
                else
                {
                    v0 = topVertex;
                    v1 = bottomVertex;
                    v2 = topVertex - 1;
                }

                triangles.Add(vertexMap.items[v0]);
                triangles.Add(vertexMap.items[reverse ? v2 : v1]);
                triangles.Add(vertexMap.items[reverse ? v1 : v2]);
            }
        }
    }

    // Convenience classes:

    public class Edge
    {
        public Edge(int[] vertexIndices)
        {
            this.vertexIndices = vertexIndices;
        }

        public int[] vertexIndices;
    }

    public class FixedSizeList<T>
    {
        public FixedSizeList(int size)
        {
            items = new T[size];
        }

        public T[] items;
        public int nextIndex;

        public void Add(T item)
        {
            items[nextIndex] = item;
            nextIndex++;
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }
    }
}