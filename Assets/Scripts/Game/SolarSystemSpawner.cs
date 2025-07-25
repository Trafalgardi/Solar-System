using UnityEngine;

public class SolarSystemSpawner : MonoBehaviour
{
    private void Awake() => Spawn(seed: 0);

    public CelestialBodyGenerator.ResolutionSettings resolutionSettings;

    public void Spawn(int seed)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var prng = new PRNG(seed);
        var bodies = FindObjectsOfType<CelestialBody>();

        foreach (var body in bodies)
        {
            if (body.bodyType == CelestialBody.BodyType.Sun)
            {
                continue;
            }

            var placeholder = body.gameObject.GetComponentInChildren<BodyPlaceholder>();
            var template = placeholder.bodySettings;

            Destroy(placeholder.gameObject);

            var holder = new GameObject(name: "Body Generator");
            var generator = holder.AddComponent<CelestialBodyGenerator>();
            generator.transform.parent = body.transform;
            generator.gameObject.layer = body.gameObject.layer;
            generator.transform.localRotation = Quaternion.identity;
            generator.transform.localPosition = Vector3.zero;
            generator.transform.localScale = Vector3.one * body.radius;
            generator.resolutionSettings = resolutionSettings;

            generator.body = template;
        }

        Debug.Log("Generation time: " + sw.ElapsedMilliseconds + " ms.");
    }
}