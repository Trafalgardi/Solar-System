using UnityEngine;

public class StarDome : MonoBehaviour
{
    private const float calibrationDst = 2000;

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;

        //var sw = System.Diagnostics.Stopwatch.StartNew ();
        if (cam)
        {
            var starDst = cam.farClipPlane - radiusMinMax.y;
            var scale = starDst / calibrationDst;

            for (var i = 0; i < count; i++)
            {
                var star = Instantiate(starPrefab, Random.onUnitSphere * starDst, Quaternion.identity, transform);
                var t = SmallestRandomValue(iterations: 6);
                star.transform.localScale = Vector3.one * Mathf.Lerp(radiusMinMax.x, radiusMinMax.y, t) * scale;

                star.material.color = Color.Lerp(Color.black, star.material.color,
                    Mathf.Lerp(brightnessMinMax.x, brightnessMinMax.y, t));
            }
        }
        //Debug.Log (sw.ElapsedMilliseconds);
    }

    public MeshRenderer starPrefab;
    public Vector2 radiusMinMax;
    public int count = 1000;
    public Vector2 brightnessMinMax;

    private float SmallestRandomValue(int iterations)
    {
        float r = 1;

        for (var i = 0; i < iterations; i++)
        {
            r = Mathf.Min(r, Random.value);
        }

        return r;
    }

    private void LateUpdate()
    {
        if (cam != null)
        {
            transform.position = cam.transform.position;
        }
    }
}