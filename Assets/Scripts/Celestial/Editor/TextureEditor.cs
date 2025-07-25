using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TextureViewer), editorForChildClasses: true)]
public class TextureEditor : Editor
{
    private Editor generatorEditor;
    private bool generatorFoldout;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var textureViewer = (TextureViewer)target;

        if (GUILayout.Button(text: "Generate"))
        {
            textureViewer.UpdateTexture();
        }

        if (GUILayout.Button(text: "Save"))
        {
            var path = Application.dataPath + "/Resources";
            textureViewer.SaveTexture(path);
        }

        if (textureViewer.generator)
        {
            var settingsUpdated =
                DrawSettingsEditor(textureViewer.generator, ref generatorFoldout, ref generatorEditor);

            if (settingsUpdated)
            {
                textureViewer.UpdateTexture();
            }
        }
    }

    private bool DrawSettingsEditor(Object settings, ref bool foldout, ref Editor editor)
    {
        if (settings != null)
        {
            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);

            if (foldout)
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    CreateCachedEditor(settings, editorType: null, ref editor);
                    editor.OnInspectorGUI();

                    if (check.changed)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void OnEnable() => generatorFoldout = EditorPrefs.GetBool(nameof(generatorFoldout), defaultValue: false);

    private void OnDisable() => EditorPrefs.SetBool(nameof(generatorFoldout), generatorFoldout);
}