using UnityEngine;

[ExecuteInEditMode]
public class TextureViewer : MonoBehaviour
{
    public TextureGenerator generator;

    [Header(header: "Preview Settings")]
    [Range(min: 1, max: 10)]
    public int tiling = 1;

    public Material previewMaterial;
    public MeshRenderer[] previewObjects;

    public void UpdateTexture()
    {
        if (generator)
        {
            var renderTexture = generator.GenerateTexture();
            DisplayPreview(renderTexture);
        }
    }

    public void SaveTexture(string path)
    {
        if (generator)
        {
            var renderTexture = generator.GenerateTexture();
            var oldRT = RenderTexture.active;

            var tex2D = new Texture2D(renderTexture.width, renderTexture.height);
            RenderTexture.active = renderTexture;
            tex2D.ReadPixels(new Rect(x: 0, y: 0, renderTexture.width, renderTexture.height), destX: 0, destY: 0);
            tex2D.Apply();

            System.IO.File.WriteAllBytes(System.IO.Path.Combine(path, renderTexture.name + ".png"),
                tex2D.EncodeToPNG());

            RenderTexture.active = oldRT;
        }
    }

    private void Update() => UpdateTexture();

    private void DisplayPreview(RenderTexture renderTexture)
    {
        if (previewMaterial == null)
        {
            previewMaterial = new Material(Shader.Find(name: "Unlit/Texture"));
        }

        previewMaterial.SetTexture(name: "_MainTex", renderTexture);
        previewMaterial.mainTextureScale = new Vector2(tiling, tiling);

        if (previewObjects != null)
        {
            foreach (var previewObject in previewObjects)
            {
                if (previewObject)
                {
                    previewObject.sharedMaterial = previewMaterial;
                }
            }
        }
    }
}