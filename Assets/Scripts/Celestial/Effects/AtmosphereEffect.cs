using UnityEngine;

public class AtmosphereEffect
{
    private Light light;
    protected Material material;

    public void UpdateSettings(CelestialBodyGenerator generator)
    {
        var shader = generator.body.shading.atmosphereSettings.atmosphereShader;

        if (material == null || material.shader != shader)
        {
            material = new Material(shader);
        }

        if (light == null)
        {
            light = GameObject.FindObjectOfType<SunShadowCaster>()?.GetComponent<Light>();
        }

        //generator.shading.SetAtmosphereProperties (material);
        generator.body.shading.atmosphereSettings.SetProperties(material, generator.BodyScale);

        material.SetVector(name: "planetCentre", generator.transform.position);
        //material.SetFloat ("atmosphereRadius", (1 + 0.5f) * generator.BodyScale);
        material.SetFloat(name: "oceanRadius", generator.GetOceanRadius());

        if (light)
        {
            var dirFromPlanetToSun = (light.transform.position - generator.transform.position).normalized;
            //Debug.Log(dirFromPlanetToSun);
            material.SetVector(name: "dirToSun", dirFromPlanetToSun);
        }
        else
        {
            material.SetVector(name: "dirToSun", Vector3.up);
            Debug.Log(message: "No SunShadowCaster found");
        }
    }

    public Material GetMaterial() => material;
}