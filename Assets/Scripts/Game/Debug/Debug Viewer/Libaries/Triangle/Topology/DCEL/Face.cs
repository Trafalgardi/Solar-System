// -----------------------------------------------------------------------
// <copyright file="Face.cs">
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using TriangleNet.Geometry;

namespace TriangleNet.Topology.DCEL
{
    /// <summary>
    /// A face of DCEL mesh.
    /// </summary>
    public class Face
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Face" /> class.
        /// </summary>
        /// <param name="generator">The generator of this face (for Voronoi diagram)</param>
        public Face(Point generator)
            : this(generator, edge: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Face" /> class.
        /// </summary>
        /// <param name="generator">The generator of this face (for Voronoi diagram)</param>
        /// <param name="edge">The half-edge connected to this face.</param>
        public Face(Point generator, HalfEdge edge)
        {
            this.generator = generator;
            this.edge = edge;
            bounded = true;

            if (generator != null)
            {
                id = generator.ID;
            }
        }

        /// <summary>
        /// Gets or sets the face id.
        /// </summary>
        public int ID
        {
            get => id;
            set => id = value;
        }

        /// <summary>
        /// Gets or sets a half-edge connected to the face.
        /// </summary>
        public HalfEdge Edge
        {
            get => edge;
            set => edge = value;
        }

        /// <summary>
        /// Gets or sets a value, indicating if the face is bounded (for Voronoi diagram).
        /// </summary>
        public bool Bounded
        {
            get => bounded;
            set => bounded = value;
        }

        /// <summary>
        /// Enumerates all half-edges of the face boundary.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<HalfEdge> EnumerateEdges()
        {
            var edge = Edge;
            var first = edge.ID;

            do
            {
                yield return edge;

                edge = edge.Next;
            } while (edge.ID != first);
        }

        public override string ToString() => string.Format(format: "F-ID {0}", id);

        internal int id;

        internal Point generator;

        internal HalfEdge edge;
        internal bool bounded;

        #region Static initialization of "Outer Space" face

        public static readonly Face Empty;

        static Face()
        {
            Empty = new Face(generator: null);
            Empty.id = -1;
        }

        #endregion
    }
}