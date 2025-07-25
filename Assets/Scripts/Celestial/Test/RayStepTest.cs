using UnityEngine;

public class RayStepTest : MonoBehaviour
{
    public float skin;
    public int numSteps = 3;
    public float displayRad = 0.1f;

    private void OnDrawGizmos()
    {
        var start = Vector3.right * -5;
        var end = Vector3.up * 7 + Vector3.right * 3;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(start, end);

        var dst = (start - end).magnitude;
        var dir = (end - start).normalized;
        var stepSize = (dst - skin * 2) / (numSteps - 1f);
        var pos = start + dir * skin;

        Gizmos.color = Color.black;

        for (var i = 0; i < numSteps; i++)
        {
            Gizmos.DrawSphere(pos, displayRad);
            pos += dir * stepSize;
        }
    }
}