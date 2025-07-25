using System.Collections.Generic;
using UnityEngine;

public static class ConvexHull
{
    public static List<Vector2> MakeHull(Vector2[] points)
    {
        var hull = new List<Vector2>();

        var leftMostIndex = 0;
        var xMin = float.MaxValue;

        for (var i = 0; i < points.Length; i++)
        {
            if (points[i].x < xMin)
            {
                xMin = points[i].x;
                leftMostIndex = i;
            }
        }

        var pointOnHull = points[leftMostIndex];
        hull.Add(pointOnHull);

        var x = 0;

        while (true)
        {
            var endpoint = points[0];
            x++;

            if (x > 1000)
            {
                Debug.Log(message: "Exitting");
                Debug.Break();

                return null;
            }

            for (var i = 1; i < points.Length; i++)
            {
                if (endpoint == pointOnHull || MathUtility.SideOfLine(pointOnHull, endpoint, points[i]) == -1)
                {
                    endpoint = points[i];
                }
            }

            pointOnHull = endpoint;

            if (pointOnHull == hull[index: 0])
            {
                break;
            }

            hull.Add(pointOnHull);
        }

        return hull;
    }
}