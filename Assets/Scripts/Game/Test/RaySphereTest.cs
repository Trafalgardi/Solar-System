using UnityEngine;

public class RaySphereTest : MonoBehaviour
{
    public Transform sphere;

    // Returns dstToSphere, dstThroughSphere
    private Vector2 raySphere(Vector3 sphereCentre, float sphereRadius, Vector3 rayOrigin, Vector3 rayDir)
    {
        var offset = rayOrigin - sphereCentre;
        const float a = 1; // set to dot(rayDir, rayDir) if rayDir might be unnormalized
        var b = 2 * Vector3.Dot(offset, rayDir);
        var c = Vector3.Dot(offset, offset) - sphereRadius * sphereRadius;

        var discriminant = b * b - 4 * a * c;

        // No intersections: discriminant < 0
        // 1 intersection: discriminant == 0
        // 2 intersections: discriminant > 0
        if (discriminant > 0)
        {
            var s = Mathf.Sqrt(discriminant);
            var dstToSphereNear = Mathf.Max(a: 0, (-b - s) / (2 * a));
            var dstToSphereFar = (-b + s) / (2 * a);

            if (dstToSphereFar >= 0)
            {
                return new Vector2(dstToSphereNear, dstToSphereFar - dstToSphereNear);
            }
        }

        return Vector2.zero;
    }

    private void OnDrawGizmos()
    {
        var r = raySphere(sphere.position, sphere.localScale.x / 2, transform.position, transform.forward);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * r.x);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position + transform.forward * r.x, transform.forward * r.y);
        Debug.Log(r.y);
    }
}