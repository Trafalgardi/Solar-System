using UnityEngine;

public class SunShadowCaster : MonoBehaviour
{
    private Transform track;

    private void Start() => track = Camera.main?.transform;

    private void LateUpdate()
    {
        if (track)
        {
            transform.LookAt(track.position);
        }
    }
}