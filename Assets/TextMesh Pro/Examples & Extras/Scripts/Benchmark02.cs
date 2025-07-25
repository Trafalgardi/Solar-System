using UnityEngine;

namespace TMPro.Examples
{
    public class Benchmark02 : MonoBehaviour
    {
        private TextMeshProFloatingText floatingText_Script;


        private void Start()
        {
            for (var i = 0; i < NumberOfNPC; i++)
            {
                if (SpawnType == 0)
                {
                    // TextMesh Pro Implementation
                    var go = new GameObject();

                    go.transform.position = new Vector3(Random.Range(minInclusive: -95f, maxInclusive: 95f), y: 0.25f,
                        Random.Range(minInclusive: -95f, maxInclusive: 95f));

                    var textMeshPro = go.AddComponent<TextMeshPro>();

                    textMeshPro.autoSizeTextContainer = true;
                    textMeshPro.rectTransform.pivot = new Vector2(x: 0.5f, y: 0);

                    textMeshPro.alignment = TextAlignmentOptions.Bottom;
                    textMeshPro.fontSize = 96;
                    textMeshPro.fontFeatures.Clear();

                    textMeshPro.color = new Color32(r: 255, g: 255, b: 0, a: 255);
                    textMeshPro.text = "!";
                    textMeshPro.isTextObjectScaleStatic = IsTextObjectScaleStatic;

                    // Spawn Floating Text
                    floatingText_Script = go.AddComponent<TextMeshProFloatingText>();
                    floatingText_Script.SpawnType = 0;
                    floatingText_Script.IsTextObjectScaleStatic = IsTextObjectScaleStatic;
                }
                else if (SpawnType == 1)
                {
                    // TextMesh Implementation
                    var go = new GameObject();

                    go.transform.position = new Vector3(Random.Range(minInclusive: -95f, maxInclusive: 95f), y: 0.25f,
                        Random.Range(minInclusive: -95f, maxInclusive: 95f));

                    var textMesh = go.AddComponent<TextMesh>();
                    textMesh.font = Resources.Load<Font>(path: "Fonts/ARIAL");
                    textMesh.GetComponent<Renderer>().sharedMaterial = textMesh.font.material;

                    textMesh.anchor = TextAnchor.LowerCenter;
                    textMesh.fontSize = 96;

                    textMesh.color = new Color32(r: 255, g: 255, b: 0, a: 255);
                    textMesh.text = "!";

                    // Spawn Floating Text
                    floatingText_Script = go.AddComponent<TextMeshProFloatingText>();
                    floatingText_Script.SpawnType = 1;
                }
                else if (SpawnType == 2)
                {
                    // Canvas WorldSpace Camera
                    var go = new GameObject();
                    var canvas = go.AddComponent<Canvas>();
                    canvas.worldCamera = Camera.main;

                    go.transform.localScale = new Vector3(x: 0.1f, y: 0.1f, z: 0.1f);

                    go.transform.position = new Vector3(Random.Range(minInclusive: -95f, maxInclusive: 95f), y: 5f,
                        Random.Range(minInclusive: -95f, maxInclusive: 95f));

                    var textObject = new GameObject().AddComponent<TextMeshProUGUI>();
                    textObject.rectTransform.SetParent(go.transform, worldPositionStays: false);

                    textObject.color = new Color32(r: 255, g: 255, b: 0, a: 255);
                    textObject.alignment = TextAlignmentOptions.Bottom;
                    textObject.fontSize = 96;
                    textObject.text = "!";

                    // Spawn Floating Text
                    floatingText_Script = go.AddComponent<TextMeshProFloatingText>();
                    floatingText_Script.SpawnType = 0;
                }
            }
        }

        public int SpawnType;
        public int NumberOfNPC = 12;

        public bool IsTextObjectScaleStatic;
    }
}