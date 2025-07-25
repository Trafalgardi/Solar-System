// Based on bloom effect by Keijiro Takahashi (see accompanying readme and license for details)
// The original can be found here: https://github.com/keijiro/KinoBloom/releases
// Minor modifications made

using UnityEngine;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[CreateAssetMenu(menuName = "PostProcessing/Bloom")]
public class BloomEffect : PostProcessingEffect
{
    [SerializeField]
    [Tooltip(tooltip: "Filters out pixels under this level of brightness.")]
    private float _threshold = 0.8f;

    [SerializeField]
    [Range(min: 0, max: 1)]
    [Tooltip(tooltip: "Makes transition between under/over-threshold gradual.")]
    private float _softKnee = 0.5f;

    [SerializeField]
    [Range(min: 1, max: 7)]
    [Tooltip("Changes extent of veiling effects\n" +
             "in a screen resolution-independent fashion.")]
    private float _radius = 2.5f;

    [SerializeField]
    [Tooltip(tooltip: "Blend factor of the result image.")]
    private float _intensity = 0.8f;

    [SerializeField]
    [Tooltip(tooltip: "Controls filter quality and buffer resolution.")]
    private bool _highQuality = true;

    /// Anti-flicker filter
    /// Reduces flashing noise with an additional filter.
    [SerializeField]
    [Tooltip(tooltip: "Reduces flashing noise with an additional filter.")]
    private bool _antiFlicker = true;

    [SerializeField][HideInInspector] private Shader _shader;

    private Material _material;

    private const int kMaxIterations = 16;
    private readonly RenderTexture[] _blurBuffer1 = new RenderTexture[kMaxIterations];
    private readonly RenderTexture[] _blurBuffer2 = new RenderTexture[kMaxIterations];

    /// Prefilter threshold (gamma-encoded)
    /// Filters out pixels under this level of brightness.
    public float thresholdGamma
    {
        get => Mathf.Max(_threshold, b: 0);
        set => _threshold = value;
    }

    /// Prefilter threshold (linearly-encoded)
    /// Filters out pixels under this level of brightness.
    public float thresholdLinear
    {
        get => GammaToLinear(thresholdGamma);
        set => _threshold = LinearToGamma(value);
    }

    /// Soft-knee coefficient
    /// Makes transition between under/over-threshold gradual.
    public float softKnee
    {
        get => _softKnee;
        set => _softKnee = value;
    }

    /// Bloom radius
    /// Changes extent of veiling effects in a screen
    /// resolution-independent fashion.
    public float radius
    {
        get => _radius;
        set => _radius = value;
    }

    /// Bloom intensity
    /// Blend factor of the result image.
    public float intensity
    {
        get => Mathf.Max(_intensity, b: 0);
        set => _intensity = value;
    }

    /// High quality mode
    /// Controls filter quality and buffer resolution.
    public bool highQuality
    {
        get => _highQuality;
        set => _highQuality = value;
    }

    public bool antiFlicker
    {
        get => _antiFlicker;
        set => _antiFlicker = value;
    }

    public override void Render(RenderTexture source, RenderTexture destination)
    {
        var useRGBM = Application.isMobilePlatform;

        // source texture size
        var tw = source.width;
        var th = source.height;

        // halve the texture size for the low quality mode
        if (!_highQuality)
        {
            tw /= 2;
            th /= 2;
        }

        // blur buffer format
        var rtFormat = useRGBM ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR;

        // determine the iteration count
        var logh = Mathf.Log(th, p: 2) + _radius - 8;
        var logh_i = (int)logh;
        var iterations = Mathf.Clamp(logh_i, min: 1, kMaxIterations);

        // update the shader properties
        var lthresh = thresholdLinear;
        _material.SetFloat(name: "_Threshold", lthresh);

        var knee = lthresh * _softKnee + 1e-5f;
        var curve = new Vector3(lthresh - knee, knee * 2, 0.25f / knee);
        _material.SetVector(name: "_Curve", curve);

        var pfo = !_highQuality && _antiFlicker;
        _material.SetFloat(name: "_PrefilterOffs", pfo ? -0.5f : 0.0f);

        _material.SetFloat(name: "_SampleScale", 0.5f + logh - logh_i);
        _material.SetFloat(name: "_Intensity", intensity);

        // prefilter pass
        var prefiltered = RenderTexture.GetTemporary(tw, th, depthBuffer: 0, rtFormat);
        var pass = _antiFlicker ? 1 : 0;
        Graphics.Blit(source, prefiltered, _material, pass);

        // construct a mip pyramid
        var last = prefiltered;

        for (var level = 0; level < iterations; level++)
        {
            _blurBuffer1[level] = RenderTexture.GetTemporary(last.width / 2, last.height / 2, depthBuffer: 0, rtFormat);

            pass = level == 0 ? _antiFlicker ? 3 : 2 : 4;
            Graphics.Blit(last, _blurBuffer1[level], _material, pass);

            last = _blurBuffer1[level];
        }

        // upsample and combine loop
        for (var level = iterations - 2; level >= 0; level--)
        {
            var basetex = _blurBuffer1[level];
            _material.SetTexture(name: "_BaseTex", basetex);

            _blurBuffer2[level] = RenderTexture.GetTemporary(basetex.width, basetex.height, depthBuffer: 0, rtFormat);

            pass = _highQuality ? 6 : 5;
            Graphics.Blit(last, _blurBuffer2[level], _material, pass);
            last = _blurBuffer2[level];
        }

        // finish process
        _material.SetTexture(name: "_BaseTex", source);
        pass = _highQuality ? 8 : 7;
        Graphics.Blit(last, destination, _material, pass);

        // release the temporary buffers
        for (var i = 0; i < kMaxIterations; i++)
        {
            if (_blurBuffer1[i] != null)
            {
                RenderTexture.ReleaseTemporary(_blurBuffer1[i]);
            }

            if (_blurBuffer2[i] != null)
            {
                RenderTexture.ReleaseTemporary(_blurBuffer2[i]);
            }

            _blurBuffer1[i] = null;
            _blurBuffer2[i] = null;
        }

        RenderTexture.ReleaseTemporary(prefiltered);
    }

    private float LinearToGamma(float x) => Mathf.LinearToGammaSpace(x);

    private float GammaToLinear(float x) => Mathf.GammaToLinearSpace(x);

    private void OnEnable()
    {
        //
        _shader = null;
        var shader = _shader ? _shader : Shader.Find(name: "Hidden/Kino/Bloom");
        _material = new Material(shader);
        _material.hideFlags = HideFlags.DontSave;
    }

    private void OnDisable() => DestroyImmediate(_material);
}