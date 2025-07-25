using UnityEngine;

namespace TMPro.Examples
{
    public class TMP_UiFrameRateCounter : MonoBehaviour
    {
        private float m_LastInterval;
        private int m_Frames;

        private string htmlColorTag;
        private const string fpsLabel = "{0:2}</color> <#8080ff>FPS \n<#FF8000>{1:2} <#8080ff>MS";

        private TextMeshProUGUI m_TextMeshPro;
        private RectTransform m_frameCounter_transform;

        private FpsCounterAnchorPositions last_AnchorPosition;

        private void Awake()
        {
            if (!enabled)
            {
                return;
            }

            Application.targetFrameRate = 1000;

            var frameCounter = new GameObject(name: "Frame Counter");
            m_frameCounter_transform = frameCounter.AddComponent<RectTransform>();

            m_frameCounter_transform.SetParent(transform, worldPositionStays: false);

            m_TextMeshPro = frameCounter.AddComponent<TextMeshProUGUI>();
            m_TextMeshPro.font = Resources.Load<TMP_FontAsset>(path: "Fonts & Materials/LiberationSans SDF");

            m_TextMeshPro.fontSharedMaterial =
                Resources.Load<Material>(path: "Fonts & Materials/LiberationSans SDF - Overlay");

            m_TextMeshPro.textWrappingMode = TextWrappingModes.NoWrap;
            m_TextMeshPro.fontSize = 36;

            m_TextMeshPro.isOverlay = true;

            Set_FrameCounter_Position(AnchorPosition);
            last_AnchorPosition = AnchorPosition;
        }


        private void Start()
        {
            m_LastInterval = Time.realtimeSinceStartup;
            m_Frames = 0;
        }

        public float UpdateInterval = 5.0f;

        public FpsCounterAnchorPositions AnchorPosition = FpsCounterAnchorPositions.TopRight;


        private void Update()
        {
            if (AnchorPosition != last_AnchorPosition)
            {
                Set_FrameCounter_Position(AnchorPosition);
            }

            last_AnchorPosition = AnchorPosition;

            m_Frames += 1;
            var timeNow = Time.realtimeSinceStartup;

            if (timeNow > m_LastInterval + UpdateInterval)
            {
                // display two fractional digits (f2 format)
                var fps = m_Frames / (timeNow - m_LastInterval);
                var ms = 1000.0f / Mathf.Max(fps, b: 0.00001f);

                if (fps < 30)
                {
                    htmlColorTag = "<color=yellow>";
                }
                else if (fps < 10)
                {
                    htmlColorTag = "<color=red>";
                }
                else
                {
                    htmlColorTag = "<color=green>";
                }

                m_TextMeshPro.SetText(htmlColorTag + fpsLabel, fps, ms);

                m_Frames = 0;
                m_LastInterval = timeNow;
            }
        }


        private void Set_FrameCounter_Position(FpsCounterAnchorPositions anchor_position)
        {
            switch (anchor_position)
            {
                case FpsCounterAnchorPositions.TopLeft:
                    m_TextMeshPro.alignment = TextAlignmentOptions.TopLeft;
                    m_frameCounter_transform.pivot = new Vector2(x: 0, y: 1);
                    m_frameCounter_transform.anchorMin = new Vector2(x: 0.01f, y: 0.99f);
                    m_frameCounter_transform.anchorMax = new Vector2(x: 0.01f, y: 0.99f);
                    m_frameCounter_transform.anchoredPosition = new Vector2(x: 0, y: 1);

                    break;

                case FpsCounterAnchorPositions.BottomLeft:
                    m_TextMeshPro.alignment = TextAlignmentOptions.BottomLeft;
                    m_frameCounter_transform.pivot = new Vector2(x: 0, y: 0);
                    m_frameCounter_transform.anchorMin = new Vector2(x: 0.01f, y: 0.01f);
                    m_frameCounter_transform.anchorMax = new Vector2(x: 0.01f, y: 0.01f);
                    m_frameCounter_transform.anchoredPosition = new Vector2(x: 0, y: 0);

                    break;

                case FpsCounterAnchorPositions.TopRight:
                    m_TextMeshPro.alignment = TextAlignmentOptions.TopRight;
                    m_frameCounter_transform.pivot = new Vector2(x: 1, y: 1);
                    m_frameCounter_transform.anchorMin = new Vector2(x: 0.99f, y: 0.99f);
                    m_frameCounter_transform.anchorMax = new Vector2(x: 0.99f, y: 0.99f);
                    m_frameCounter_transform.anchoredPosition = new Vector2(x: 1, y: 1);

                    break;

                case FpsCounterAnchorPositions.BottomRight:
                    m_TextMeshPro.alignment = TextAlignmentOptions.BottomRight;
                    m_frameCounter_transform.pivot = new Vector2(x: 1, y: 0);
                    m_frameCounter_transform.anchorMin = new Vector2(x: 0.99f, y: 0.01f);
                    m_frameCounter_transform.anchorMax = new Vector2(x: 0.99f, y: 0.01f);
                    m_frameCounter_transform.anchoredPosition = new Vector2(x: 1, y: 0);

                    break;
            }
        }

        public enum FpsCounterAnchorPositions
        {
            TopLeft,
            BottomLeft,
            TopRight,
            BottomRight,
        }
    }
}