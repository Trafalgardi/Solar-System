using UnityEngine;

[ExecuteInEditMode]
public class LODHandler : MonoBehaviour
{
    private Camera cam;
    private Transform camT;
    private CelestialBody[] bodies;
    private CelestialBodyGenerator[] generators;

    private void Start()
    {
        if (Application.isPlaying)
        {
            bodies = FindObjectsOfType<CelestialBody>();
            generators = new CelestialBodyGenerator[bodies.Length];

            for (var i = 0; i < generators.Length; i++)
            {
                generators[i] = bodies[i].GetComponentInChildren<CelestialBodyGenerator>();
            }
        }
    }

    [Header(header: "LOD screen heights")]
    // LOD level is determined by body's screen height (1 = taking up entire screen, 0 = teeny weeny speck) 
    public float lod1Threshold = .5f;

    public float lod2Threshold = .2f;

    [Header(header: "Debug")] public bool debug;

    public CelestialBody debugBody;

    private void Update()
    {
        DebugLODInfo();

        if (Application.isPlaying)
        {
            HandleLODs();
        }
    }

    private void HandleLODs()
    {
        for (var i = 0; i < bodies.Length; i++)
        {
            if (generators[i] != null)
            {
                var screenHeight = CalculateScreenHeight(bodies[i]);
                var lodIndex = CalculateLODIndex(screenHeight);
                generators[i].SetLOD(lodIndex);
            }
        }
    }

    private int CalculateLODIndex(float screenHeight)
    {
        if (screenHeight > lod1Threshold)
        {
            return 0;
        }

        if (screenHeight > lod2Threshold)
        {
            return 1;
        }

        return 2;
    }

    private void DebugLODInfo()
    {
        if (debugBody && debug)
        {
            var h = CalculateScreenHeight(debugBody);
            var index = CalculateLODIndex(h);
            Debug.Log($"Screen height of {debugBody.name}: {h} (lod = {index})");
        }
    }

    private float CalculateScreenHeight(CelestialBody body)
    {
        if (cam == null)
        {
            cam = Camera.main;
            camT = cam.transform;
        }

        var originalRot = camT.rotation;
        var bodyCentre = body.transform.position;
        camT.LookAt(bodyCentre);

        var viewA = cam.WorldToViewportPoint(bodyCentre - camT.up * body.radius);
        var viewB = cam.WorldToViewportPoint(bodyCentre + camT.up * body.radius);
        var screenHeight = Mathf.Abs(viewA.y - viewB.y);
        camT.rotation = originalRot;

        return screenHeight;
    }
}