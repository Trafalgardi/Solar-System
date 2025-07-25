using UnityEngine;

namespace TMPro.Examples
{
    public class TMP_FrameRateCounter : MonoBehaviour
    {
        private float m_LastInterval;
        private int m_Frames;

        private string htmlColorTag;
        private const string fpsLabel = "{0:2}</color> <#8080ff>FPS \n<#FF8000>{1:2} <#8080ff>MS";

        private TextMeshPro m_TextMeshPro;
        private Transform m_frameCounter_transform;
        private Camera m_camera;

        private FpsCounterAnchorPositions last_AnchorPosition;

        private void Awake()
        {
            if (!enabled)
            {
                return;
            }

            m_camera = Camera.main;
            Application.targetFrameRate = 9999;

            var frameCounter = new GameObject(name: "Frame Counter");

            m_TextMeshPro = frameCounter.AddComponent<TextMeshPro>();
            m_TextMeshPro.font = Resources.Load<TMP_FontAsset>(path: "Fonts & Materials/LiberationSans SDF");

            m_TextMeshPro.fontSharedMaterial =
                Resources.Load<Material>(path: "Fonts & Materials/LiberationSans SDF - Overlay");


            m_frameCounter_transform = frameCounter.transform;
            m_frameCounter_transform.SetParent(m_camera.transform);
            m_frameCounter_transform.localRotation = Quaternion.identity;

            m_TextMeshPro.textWrappingMode = TextWrappingModes.NoWrap;
            m_TextMeshPro.fontSize = 24;
            //m_TextMeshPro.FontColor = new Color32(255, 255, 255, 128);
            //m_TextMeshPro.edgeWidth = .15f;
            //m_TextMeshPro.isOverlay = true;

            //m_TextMeshPro.FaceColor = new Color32(255, 128, 0, 0);
            //m_TextMeshPro.EdgeColor = new Color32(0, 255, 0, 255);
            //m_TextMeshPro.FontMaterial.renderQueue = 4000;

            //m_TextMeshPro.CreateSoftShadowClone(new Vector2(1f, -1f));

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

                //string format = System.String.Format(htmlColorTag + "{0:F2} </color>FPS \n{1:F2} <#8080ff>MS",fps, ms);
                //m_TextMeshPro.text = format;

                m_TextMeshPro.SetText(htmlColorTag + fpsLabel, fps, ms);

                m_Frames = 0;
                m_LastInterval = timeNow;
            }
        }


        private void Set_FrameCounter_Position(FpsCounterAnchorPositions anchor_position)
        {
            //Debug.Log("Changing frame counter anchor position.");
            m_TextMeshPro.margin = new Vector4(x: 1f, y: 1f, z: 1f, w: 1f);

            switch (anchor_position)
            {
                case FpsCounterAnchorPositions.TopLeft:
                    m_TextMeshPro.alignment = TextAlignmentOptions.TopLeft;
                    m_TextMeshPro.rectTransform.pivot = new Vector2(x: 0, y: 1);

                    m_frameCounter_transform.position =
                        m_camera.ViewportToWorldPoint(new Vector3(x: 0, y: 1, z: 100.0f));

                    break;

                case FpsCounterAnchorPositions.BottomLeft:
                    m_TextMeshPro.alignment = TextAlignmentOptions.BottomLeft;
                    m_TextMeshPro.rectTransform.pivot = new Vector2(x: 0, y: 0);

                    m_frameCounter_transform.position =
                        m_camera.ViewportToWorldPoint(new Vector3(x: 0, y: 0, z: 100.0f));

                    break;

                case FpsCounterAnchorPositions.TopRight:
                    m_TextMeshPro.alignment = TextAlignmentOptions.TopRight;
                    m_TextMeshPro.rectTransform.pivot = new Vector2(x: 1, y: 1);

                    m_frameCounter_transform.position =
                        m_camera.ViewportToWorldPoint(new Vector3(x: 1, y: 1, z: 100.0f));

                    break;

                case FpsCounterAnchorPositions.BottomRight:
                    m_TextMeshPro.alignment = TextAlignmentOptions.BottomRight;
                    m_TextMeshPro.rectTransform.pivot = new Vector2(x: 1, y: 0);

                    m_frameCounter_transform.position =
                        m_camera.ViewportToWorldPoint(new Vector3(x: 1, y: 0, z: 100.0f));

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