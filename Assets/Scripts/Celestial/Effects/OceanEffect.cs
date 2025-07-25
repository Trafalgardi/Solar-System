using UnityEngine;

public class OceanEffect
{
    private Light light;
    protected Material material;

    public void UpdateSettings(CelestialBodyGenerator generator, Shader shader)
    {
        if (material == null || material.shader != shader)
        {
            material = new Material(shader);
        }

        if (light == null)
        {
            light = GameObject.FindObjectOfType<SunShadowCaster>()?.GetComponent<Light>();
        }

        var centre = generator.transform.position;
        var radius = generator.GetOceanRadius();
        material.SetVector(name: "oceanCentre", centre);
        material.SetFloat(name: "oceanRadius", radius);

        material.SetFloat(name: "planetScale", generator.BodyScale);

        if (light)
        {
            material.SetVector(name: "dirToSun", -light.transform.forward);
        }
        else
        {
            material.SetVector(name: "dirToSun", Vector3.up);
            Debug.Log(message: "No SunShadowCaster found");
        }

        generator.body.shading.SetOceanProperties(material);
    }

    public Material GetMaterial() => material;
}