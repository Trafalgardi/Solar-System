using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TextureCombiner), editorForChildClasses: true)]
public class TextureCombinerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var textureCombiner = (TextureCombiner)target;

        if (GUILayout.Button(text: "Save"))
        {
            var path = Application.dataPath + "/Resources";
            textureCombiner.SaveTexture(path);
        }
    }
}