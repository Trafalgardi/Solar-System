using UnityEngine;

public class AmbientLightCaster : MonoBehaviour
{
    private SunShadowCaster sunLight;
    private Transform camT;
    private Light ambientLight;

    private void Start()
    {
        sunLight = FindObjectOfType<SunShadowCaster>();
        ambientLight = GetComponent<Light>();
        camT = Camera.main.transform;
        transform.rotation = CalculateAmbientLightRot();
    }

    public float maxIntensity = 1;

    private void LateUpdate()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, CalculateAmbientLightRot(), Time.deltaTime * .2f);
        var sunAlignment = Vector3.Dot(sunLight.transform.forward, transform.forward);
        var i = 1 - Mathf.Clamp01(sunAlignment); // sun in same dir = 0, sun perpendicular = 1
        var intensityMultiplier = Mathf.Clamp01((i - 0.5f) * 2);
        ambientLight.intensity = maxIntensity * intensityMultiplier;
    }

    private Quaternion CalculateAmbientLightRot()
    {
        var bodies = NBodySimulation.Bodies;
        var nearestPlanetToCam = Vector3.zero;
        var nearestSqrDst = float.PositiveInfinity;

        for (var i = 0; i < bodies.Length; i++)
        {
            var sqrDst = (camT.position - bodies[i].transform.position).sqrMagnitude;

            if (sqrDst < nearestSqrDst)
            {
                nearestSqrDst = sqrDst;
                nearestPlanetToCam = bodies[i].transform.position;
            }
        }

        var targetDir = (nearestPlanetToCam - camT.position).normalized;
        var targetRot = Quaternion.LookRotation(targetDir);

        return targetRot;
    }

    private void OnValidate() => GetComponent<Light>().intensity = maxIntensity;
}