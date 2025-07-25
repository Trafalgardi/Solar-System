using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ColourTest))]
public class ColourTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button(text: "Random"))
        {
            ((ColourTest)target).Random();
        }
    }
}