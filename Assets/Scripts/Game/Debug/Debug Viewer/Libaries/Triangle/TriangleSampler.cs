// -----------------------------------------------------------------------
// <copyright file="TriangleSampler.cs">
// Original Triangle code by Jonathan Richard Shewchuk, http://www.cs.cmu.edu/~quake/triangle.html
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using TriangleNet.Topology;

namespace TriangleNet
{
    /// <summary>
    /// Used for triangle sampling in the <see cref="TriangleLocator"/> class.
    /// </summary>
    internal class TriangleSampler : IEnumerable<Triangle>
    {
        private const int RANDOM_SEED = 110503;

        // Empirically chosen factor.
        private const int samplefactor = 11;

        private readonly Random random;
        private readonly Mesh mesh;

        // Number of random samples for point location (at least 1).
        private int samples = 1;

        // Number of triangles in mesh.
        private int triangleCount;

        public TriangleSampler(Mesh mesh)
            : this(mesh, new Random(RANDOM_SEED))
        {
        }

        public TriangleSampler(Mesh mesh, Random random)
        {
            this.mesh = mesh;
            this.random = random;
        }

        /// <summary>
        /// Reset the sampler.
        /// </summary>
        public void Reset()
        {
            samples = 1;
            triangleCount = 0;
        }

        /// <summary>
        /// Update sampling parameters if mesh changed.
        /// </summary>
        public void Update()
        {
            var count = mesh.triangles.Count;

            if (triangleCount != count)
            {
                triangleCount = count;

                // The number of random samples taken is proportional to the cube root
                // of the number of triangles in the mesh.  The next bit of code assumes
                // that the number of triangles increases monotonically (or at least
                // doesn't decrease enough to matter).
                while (samplefactor * samples * samples * samples < count)
                {
                    samples++;
                }
            }
        }

        public IEnumerator<Triangle> GetEnumerator() => mesh.triangles.Sample(samples, random).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}