using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SceneCamManager))]
public class SceneCamEditor : Editor
{
    private SceneCamManager manager;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var activeSceneView = SceneView.lastActiveSceneView;
        var allViews = SceneView.sceneViews;

        if (manager.savedViews.Count > 0)
        {
            GUILayout.Label($"Saved views: ({manager.savedViews.Count})");
            var deleteIndex = -1;

            for (var i = 0; i < manager.savedViews.Count; i++)
            {
                GUILayout.BeginVertical(style: "GroupBox");
                var savedView = manager.savedViews[i];

                savedView.name = GUILayout.TextField(savedView.name);

                GUILayout.BeginHorizontal();

                if (GUILayout.Button(text: "Set Camera View"))
                {
                    Undo.RecordObject(manager, name: "Set Camera View");
                    activeSceneView.pivot = savedView.pivot;
                    activeSceneView.rotation = savedView.rotation;
                    activeSceneView.size = savedView.size;
                }

                if (GUILayout.Button(text: "Replace"))
                {
                    Undo.RecordObject(manager, name: "Replace View");
                    savedView.pivot = activeSceneView.pivot;
                    savedView.rotation = activeSceneView.rotation;
                    savedView.size = activeSceneView.size;
                }

                if (GUILayout.Button(text: "Delete"))
                {
                    Undo.RecordObject(manager, name: "Delete View");
                    deleteIndex = i;
                }

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }

            if (deleteIndex != -1)
            {
                manager.savedViews.RemoveAt(deleteIndex);
            }

            GUILayout.Space(pixels: 15);
        }

        foreach (var v in allViews)
        {
            //GUILayout.Label (v.ToString ());
        }

        if (GUILayout.Button(text: "Save Current View"))
        {
            Undo.RecordObject(manager, name: "Save View");
            var newView = new SceneCamManager.SavedView();
            newView.name = $"View ({manager.savedViews.Count})";
            newView.pivot = activeSceneView.pivot;
            newView.rotation = activeSceneView.rotation;
            newView.size = activeSceneView.size;
            manager.savedViews.Add(newView);
        }
    }

    private void OnEnable()
    {
        manager = (SceneCamManager)target;

        if (manager.savedViews == null)
        {
            manager.savedViews = new List<SceneCamManager.SavedView>();
        }
    }
}