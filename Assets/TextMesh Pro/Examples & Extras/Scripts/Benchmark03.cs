using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace TMPro.Examples
{
    public class Benchmark03 : MonoBehaviour
    {
        private void Awake()
        {
        }


        private void Start()
        {
            TMP_FontAsset fontAsset = null;

            // Create Dynamic Font Asset for the given font file.
            switch (Benchmark)
            {
                case BenchmarkType.TMP_SDF_MOBILE:
                    fontAsset = TMP_FontAsset.CreateFontAsset(SourceFont, samplingPointSize: 90, atlasPadding: 9,
                        GlyphRenderMode.SDFAA, atlasWidth: 256, atlasHeight: 256); break;

                case BenchmarkType.TMP_SDF__MOBILE_SSD:
                    fontAsset = TMP_FontAsset.CreateFontAsset(SourceFont, samplingPointSize: 90, atlasPadding: 9,
                        GlyphRenderMode.SDFAA, atlasWidth: 256, atlasHeight: 256);

                    fontAsset.material.shader = Shader.Find(name: "TextMeshPro/Mobile/Distance Field SSD");

                    break;

                case BenchmarkType.TMP_SDF:
                    fontAsset = TMP_FontAsset.CreateFontAsset(SourceFont, samplingPointSize: 90, atlasPadding: 9,
                        GlyphRenderMode.SDFAA, atlasWidth: 256, atlasHeight: 256);

                    fontAsset.material.shader = Shader.Find(name: "TextMeshPro/Distance Field");

                    break;

                case BenchmarkType.TMP_BITMAP_MOBILE:
                    fontAsset = TMP_FontAsset.CreateFontAsset(SourceFont, samplingPointSize: 90, atlasPadding: 9,
                        GlyphRenderMode.SMOOTH, atlasWidth: 256, atlasHeight: 256); break;
            }

            for (var i = 0; i < NumberOfSamples; i++)
            {
                switch (Benchmark)
                {
                    case BenchmarkType.TMP_SDF_MOBILE:
                    case BenchmarkType.TMP_SDF__MOBILE_SSD:
                    case BenchmarkType.TMP_SDF:
                    case BenchmarkType.TMP_BITMAP_MOBILE:
                    {
                        var go = new GameObject();
                        go.transform.position = new Vector3(x: 0, y: 1.2f, z: 0);

                        var textComponent = go.AddComponent<TextMeshPro>();
                        textComponent.font = fontAsset;
                        textComponent.fontSize = 128;
                        textComponent.text = "@";
                        textComponent.alignment = TextAlignmentOptions.Center;
                        textComponent.color = new Color32(r: 255, g: 255, b: 0, a: 255);

                        if (Benchmark == BenchmarkType.TMP_BITMAP_MOBILE)
                        {
                            textComponent.fontSize = 132;
                        }
                    }

                        break;

                    case BenchmarkType.TEXTMESH_BITMAP:
                    {
                        var go = new GameObject();
                        go.transform.position = new Vector3(x: 0, y: 1.2f, z: 0);

                        var textMesh = go.AddComponent<TextMesh>();
                        textMesh.GetComponent<Renderer>().sharedMaterial = SourceFont.material;
                        textMesh.font = SourceFont;
                        textMesh.anchor = TextAnchor.MiddleCenter;
                        textMesh.fontSize = 130;

                        textMesh.color = new Color32(r: 255, g: 255, b: 0, a: 255);
                        textMesh.text = "@";
                    }

                        break;
                }
            }
        }

        public int NumberOfSamples = 100;
        public BenchmarkType Benchmark;

        public Font SourceFont;

        public enum BenchmarkType
        {
            TMP_SDF_MOBILE = 0,
            TMP_SDF__MOBILE_SSD = 1,
            TMP_SDF = 2,
            TMP_BITMAP_MOBILE = 3,
            TEXTMESH_BITMAP = 4,
        }
    }
}