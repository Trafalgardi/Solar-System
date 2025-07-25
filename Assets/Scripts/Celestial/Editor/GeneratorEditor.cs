using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CelestialBodyGenerator))]
public class GeneratorEditor : Editor
{
    private CelestialBodyGenerator generator;
    private Editor shapeEditor;
    private Editor shadingEditor;

    private bool shapeFoldout;
    private bool shadingFoldout;

    public override void OnInspectorGUI()
    {
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            DrawDefaultInspector();

            if (check.changed)
            {
                Regenerate();
            }
        }

        if (GUILayout.Button(text: "Generate"))
        {
            Regenerate();
        }

        if (GUILayout.Button(text: "Randomize Shading"))
        {
            var prng = new System.Random();
            generator.body.shading.randomize = true;
            generator.body.shading.seed = prng.Next(minValue: -10000, maxValue: 10000);
            Regenerate();
        }

        if (GUILayout.Button(text: "Randomize Shape"))
        {
            var prng = new System.Random();
            generator.body.shape.randomize = true;
            generator.body.shape.seed = prng.Next(minValue: -10000, maxValue: 10000);
            Regenerate();
        }

        if (GUILayout.Button(text: "Randomize Both"))
        {
            var prng = new System.Random();
            generator.body.shading.randomize = true;
            generator.body.shape.randomize = true;
            generator.body.shape.seed = prng.Next(minValue: -10000, maxValue: 10000);
            generator.body.shading.seed = prng.Next(minValue: -10000, maxValue: 10000);
            Regenerate();
        }

        var randomized = generator.body.shading.randomize || generator.body.shape.randomize;
        randomized |= generator.body.shading.seed != 0 || generator.body.shape.seed != 0;

        using (new EditorGUI.DisabledGroupScope(!randomized))
        {
            if (GUILayout.Button(text: "Reset Randomization"))
            {
                var prng = new System.Random();
                generator.body.shading.randomize = false;
                generator.body.shape.randomize = false;
                generator.body.shape.seed = 0;
                generator.body.shading.seed = 0;
                Regenerate();
            }
        }

        // Draw shape/shading object editors
        DrawSettingsEditor(generator.body.shape, ref shapeFoldout, ref shapeEditor);
        DrawSettingsEditor(generator.body.shading, ref shadingFoldout, ref shadingEditor);

        SaveState();
    }

    private void Regenerate()
    {
        generator.OnShapeSettingChanged();
        generator.OnShadingNoiseSettingChanged();
        EditorApplication.QueuePlayerLoopUpdate();
    }

    private void DrawSettingsEditor(Object settings, ref bool foldout, ref Editor editor)
    {
        if (settings != null)
        {
            foldout = EditorGUILayout.InspectorTitlebar(foldout, settings);

            if (foldout)
            {
                CreateCachedEditor(settings, editorType: null, ref editor);
                editor.OnInspectorGUI();
            }
        }
    }

    private void OnEnable()
    {
        shapeFoldout = EditorPrefs.GetBool(nameof(shapeFoldout), defaultValue: false);
        shadingFoldout = EditorPrefs.GetBool(nameof(shadingFoldout), defaultValue: false);
        generator = (CelestialBodyGenerator)target;
    }

    private void SaveState()
    {
        EditorPrefs.SetBool(nameof(shapeFoldout), shapeFoldout);
        EditorPrefs.SetBool(nameof(shadingFoldout), shadingFoldout);
    }
}