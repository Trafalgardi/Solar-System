using UnityEngine;
using static UnityEngine.Mathf;

[CreateAssetMenu(menuName = "Celestial Body/Atmosphere")]
public class AtmosphereSettings : ScriptableObject
{
    private RenderTexture opticalDepthTexture;
    private bool settingsUpToDate;

    public bool enabled = true;
    public Shader atmosphereShader;
    public ComputeShader opticalDepthCompute;
    public int textureSize = 256;

    public int inScatteringPoints = 10;
    public int opticalDepthPoints = 10;
    public float densityFalloff = 0.25f;

    public Vector3 wavelengths = new(x: 700, y: 530, z: 460);

    public Vector4 testParams = new(x: 7, y: 1.26f, z: 0.1f, w: 3);
    public float scatteringStrength = 20;
    public float intensity = 1;

    public float ditherStrength = 0.8f;
    public float ditherScale = 4;
    public Texture2D blueNoise;

    [Range(min: 0, max: 1)] public float atmosphereScale = 0.5f;

    [Header(header: "Test")] public float timeOfDay;

    public float sunDst = 1;

    public void SetProperties(Material material, float bodyRadius)
    {
        /*
        if (Application.isPlaying) {
            if (Time.time > 1) {
                timeOfDay += Time.deltaTime * 0.1f;
                    var sun = GameObject.Find ("Test Sun");
            sun.transform.position = new Vector3 (Mathf.Cos (timeOfDay), Mathf.Sin (timeOfDay), 0) * sunDst;
            sun.transform.LookAt (Vector3.zero);
            }
        }
        */
        if (!settingsUpToDate || !Application.isPlaying)
        {
            var sun = GameObject.Find(name: "Test Sun");

            if (sun)
            {
                sun.transform.position = new Vector3(Cos(timeOfDay), Sin(timeOfDay), z: 0) * sunDst;
                sun.transform.LookAt(Vector3.zero);
            }

            var atmosphereRadius = (1 + atmosphereScale) * bodyRadius;

            material.SetVector(name: "params", testParams);
            material.SetInt(name: "numInScatteringPoints", inScatteringPoints);
            material.SetInt(name: "numOpticalDepthPoints", opticalDepthPoints);
            material.SetFloat(name: "atmosphereRadius", atmosphereRadius);
            material.SetFloat(name: "planetRadius", bodyRadius);
            material.SetFloat(name: "densityFalloff", densityFalloff);

            // Strength of (rayleigh) scattering is inversely proportional to wavelength^4
            var scatterX = Pow(400 / wavelengths.x, p: 4);
            var scatterY = Pow(400 / wavelengths.y, p: 4);
            var scatterZ = Pow(400 / wavelengths.z, p: 4);

            material.SetVector(name: "scatteringCoefficients",
                new Vector3(scatterX, scatterY, scatterZ) * scatteringStrength);

            material.SetFloat(name: "intensity", intensity);
            material.SetFloat(name: "ditherStrength", ditherStrength);
            material.SetFloat(name: "ditherScale", ditherScale);
            material.SetTexture(name: "_BlueNoise", blueNoise);

            PrecomputeOutScattering();
            material.SetTexture(name: "_BakedOpticalDepth", opticalDepthTexture);

            settingsUpToDate = true;
        }
    }

    private void PrecomputeOutScattering()
    {
        if (!settingsUpToDate || opticalDepthTexture == null || !opticalDepthTexture.IsCreated())
        {
            ComputeHelper.CreateRenderTexture(ref opticalDepthTexture, textureSize);
            opticalDepthCompute.SetTexture(kernelIndex: 0, name: "Result", opticalDepthTexture);
            opticalDepthCompute.SetInt(name: "textureSize", textureSize);
            opticalDepthCompute.SetInt(name: "numOutScatteringSteps", opticalDepthPoints);
            opticalDepthCompute.SetFloat(name: "atmosphereRadius", 1 + atmosphereScale);
            opticalDepthCompute.SetFloat(name: "densityFalloff", densityFalloff);
            opticalDepthCompute.SetVector(name: "params", testParams);
            ComputeHelper.Run(opticalDepthCompute, textureSize, textureSize);
        }
    }

    private void OnValidate() => settingsUpToDate = false;
}