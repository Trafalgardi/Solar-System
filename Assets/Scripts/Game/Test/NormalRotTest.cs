using UnityEngine;

[ExecuteInEditMode]
public class NormalRotTest : MonoBehaviour
{
    private void Update()
    {
        var sphereNormal = transform.up;
        var normal = transform.GetChild(index: 0).up;
        var flattenedNormal = (normal - sphereNormal * Vector3.Dot(sphereNormal, normal)).normalized;
        var axis = Vector3.Cross(sphereNormal, Vector3.up).normalized;

        Debug.DrawRay(transform.position, sphereNormal, Color.green);
        Debug.DrawRay(transform.position, normal, Color.red);
        Debug.DrawRay(transform.position, flattenedNormal, Color.yellow);
        Debug.DrawRay(transform.position, axis, Color.cyan);
        Debug.Log(Vector3.Dot(axis, flattenedNormal));
    }
}