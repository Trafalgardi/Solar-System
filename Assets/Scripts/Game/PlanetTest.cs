using UnityEngine;

[ExecuteInEditMode]
public class PlanetTest : MonoBehaviour
{
    private CelestialBodyGenerator[] bodies;

    private void Awake()
    {
        if (Application.isPlaying)
        {
            skySphere.SetActive(value: false);

            if (spawnPlayer)
            {
                Destroy(FindObjectOfType<Camera>().gameObject);
                Instantiate(playerPrefab, Vector3.up * radius * 1.2f, Quaternion.identity);
            }

            if (spawnShip)
            {
                var ship = Instantiate(shipPrefab, Vector3.up * radius * 1.2f + Vector3.forward * 20 + shipOffset,
                    Quaternion.identity);

                ship.ToggleHatch();
            }

            if (sun)
            {
                sun.gameObject.SetActive(value: true);
                testLight.gameObject.SetActive(value: false);
            }

            bodies = FindObjectsOfType<CelestialBodyGenerator>();
        }
    }

    public bool useRadius;
    public float radius;

    public bool spawnPlayer = true;
    public bool spawnShip = true;
    public bool showPlanet = true;
    public PlayerController playerPrefab;
    public Ship shipPrefab;

    public Vector3 shipOffset;

    public int lodIndex;
    public GameObject sun;
    public GameObject skySphere;
    public Light testLight;
    public float timeOfDay;

    private void Update()
    {
        if (Application.isPlaying && showPlanet)
        {
            foreach (var body in bodies)
            {
                body.SetLOD(lodIndex: 0);
            }
        }

        if (!Application.isPlaying)
        {
            //sun.transform.position = new Vector3 (Mathf.Cos (timeOfDay), Mathf.Sin (timeOfDay), 0) * 1500;
            //sun.transform.LookAt (Vector3.zero);
        }
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            //sun.SetActive (false);
        }

        var body = GetComponent<CelestialBody>();
        body.radius = radius;
        body.RecalculateMass();

        if (useRadius)
        {
            FindObjectOfType<CelestialBodyGenerator>().transform.localScale = Vector3.one * radius;
        }
        else
        {
            FindObjectOfType<CelestialBodyGenerator>().transform.localScale = Vector3.one;
        }
    }
}