// -----------------------------------------------------------------------
// <copyright file="PolygonValidator.cs">
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using TriangleNet.Geometry;

namespace TriangleNet.Tools
{
    public static class PolygonValidator
    {
        /// <summary>
        /// Test the polygon for consistency.
        /// </summary>
        public static bool IsConsistent(IPolygon poly)
        {
            var logger = Log.Instance;

            var points = poly.Points;

            var horrors = 0;

            var i = 0;
            var count = points.Count;

            if (count < 3)
            {
                logger.Warning(message: "Polygon must have at least 3 vertices.",
                    info: "PolygonValidator.IsConsistent()");

                return false;
            }

            foreach (var p in points)
            {
                if (p == null)
                {
                    horrors++;

                    logger.Warning(string.Format(format: "Point {0} is null.", i),
                        info: "PolygonValidator.IsConsistent()");
                }
                else if (double.IsNaN(p.x) || double.IsNaN(p.y))
                {
                    horrors++;

                    logger.Warning(string.Format(format: "Point {0} has invalid coordinates.", i),
                        info: "PolygonValidator.IsConsistent()");
                }
                else if (double.IsInfinity(p.x) || double.IsInfinity(p.y))
                {
                    horrors++;

                    logger.Warning(string.Format(format: "Point {0} has invalid coordinates.", i),
                        info: "PolygonValidator.IsConsistent()");
                }

                i++;
            }

            i = 0;

            foreach (var seg in poly.Segments)
            {
                if (seg == null)
                {
                    horrors++;

                    logger.Warning(string.Format(format: "Segment {0} is null.", i),
                        info: "PolygonValidator.IsConsistent()");

                    // Always abort if a NULL-segment is found.
                    return false;
                }

                var p = seg.GetVertex(index: 0);
                var q = seg.GetVertex(index: 1);

                if (p.x == q.x && p.y == q.y)
                {
                    horrors++;

                    logger.Warning(
                        string.Format(format: "Endpoints of segment {0} are coincident (IDs {1} / {2}).", i, p.id,
                            q.id),
                        info: "PolygonValidator.IsConsistent()");
                }

                i++;
            }

            if (points[index: 0].id == points[index: 1].id)
            {
                horrors += CheckVertexIDs(poly, count);
            }
            else
            {
                horrors += CheckDuplicateIDs(poly);
            }

            return horrors == 0;
        }

        /// <summary>
        /// Test the polygon for duplicate vertices.
        /// </summary>
        public static bool HasDuplicateVertices(IPolygon poly)
        {
            var logger = Log.Instance;

            var horrors = 0;

            var points = poly.Points.ToArray();

            VertexSorter.Sort(points);

            for (var i = 1; i < points.Length; i++)
            {
                if (points[i - 1] == points[i])
                {
                    horrors++;

                    logger.Warning(string.Format(format: "Found duplicate point {0}.", points[i]),
                        info: "PolygonValidator.HasDuplicateVertices()");
                }
            }

            return horrors > 0;
        }

        /// <summary>
        /// Test the polygon for 360 degree angles.
        /// </summary>
        /// <param name="poly">The polygon.</param>
        /// <param name="threshold">The angle threshold.</param>
        public static bool HasBadAngles(IPolygon poly, double threshold = 2e-12)
        {
            var logger = Log.Instance;

            var horrors = 0;
            var i = 0;

            Point p0 = null, p1 = null;
            Point q0, q1;

            var count = poly.Points.Count;

            foreach (var seg in poly.Segments)
            {
                q0 = p0; // Previous segment start point.
                q1 = p1; // Previous segment end point.

                p0 = seg.GetVertex(index: 0); // Current segment start point.
                p1 = seg.GetVertex(index: 1); // Current segment end point.

                if (p0 == p1 || q0 == q1)
                {
                    // Ignore zero-length segments.
                    continue;
                }

                if (q0 != null && q1 != null)
                {
                    // The two segments are connected.
                    if (p0 == q1 && p1 != null)
                    {
                        if (IsBadAngle(q0, p0, p1, threshold))
                        {
                            horrors++;

                            logger.Warning(string.Format(format: "Bad segment angle found at index {0}.", i),
                                info: "PolygonValidator.HasBadAngles()");
                        }
                    }
                }

                i++;
            }

            return horrors > 0;
        }

        private static bool IsBadAngle(Point a, Point b, Point c, double threshold = 0.0)
        {
            var x = DotProduct(a, b, c);
            var y = CrossProductLength(a, b, c);

            return Math.Abs(Math.Atan2(y, x)) <= threshold;
        }

        //  Returns the dot product <AB, BC>.
        private static double DotProduct(Point a, Point b, Point c) =>
            //  Calculate the dot product.
            (a.x - b.x) * (c.x - b.x) + (a.y - b.y) * (c.y - b.y);

        //  Returns the length of cross product AB x BC.
        private static double CrossProductLength(Point a, Point b, Point c) =>
            //  Calculate the Z coordinate of the cross product.
            (a.x - b.x) * (c.y - b.y) - (a.y - b.y) * (c.x - b.x);

        private static int CheckVertexIDs(IPolygon poly, int count)
        {
            var logger = Log.Instance;

            var horrors = 0;

            var i = 0;

            Vertex p, q;

            foreach (var seg in poly.Segments)
            {
                p = seg.GetVertex(index: 0);
                q = seg.GetVertex(index: 1);

                if (p.id < 0 || p.id >= count)
                {
                    horrors++;

                    logger.Warning(string.Format(format: "Segment {0} has invalid startpoint.", i),
                        info: "PolygonValidator.IsConsistent()");
                }

                if (q.id < 0 || q.id >= count)
                {
                    horrors++;

                    logger.Warning(string.Format(format: "Segment {0} has invalid endpoint.", i),
                        info: "PolygonValidator.IsConsistent()");
                }

                i++;
            }

            return horrors;
        }

        private static int CheckDuplicateIDs(IPolygon poly)
        {
            var ids = new HashSet<int>();

            // Check for duplicate ids.
            foreach (var p in poly.Points)
            {
                if (!ids.Add(p.id))
                {
                    Log.Instance.Warning(message: "Found duplicate vertex ids.",
                        info: "PolygonValidator.IsConsistent()");

                    return 1;
                }
            }

            return 0;
        }
    }
}