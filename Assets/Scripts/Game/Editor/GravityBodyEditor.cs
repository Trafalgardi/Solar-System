using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GravityObject), editorForChildClasses: true)]
[CanEditMultipleObjects]
public class GravityBodyEditor : Editor
{
    private GravityObject gravityObject;
    private bool showDebugInfo;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(width: 10);
        EditorGUILayout.LabelField(label: "Debug", EditorStyles.boldLabel);
        showDebugInfo = EditorGUILayout.Foldout(showDebugInfo, content: "Debug info");

        if (showDebugInfo)
        {
            var gravityInfo = GetGravityInfo(gravityObject.transform.position, gravityObject as CelestialBody);

            for (var i = 0; i < gravityInfo.Length; i++)
            {
                EditorGUILayout.LabelField(gravityInfo[i]);
            }
        }
    }

    private void OnEnable()
    {
        gravityObject = (GravityObject)target;
        showDebugInfo = EditorPrefs.GetBool(gravityObject.gameObject.name + nameof(showDebugInfo), defaultValue: false);
    }

    private void OnDisable()
    {
        if (gravityObject)
        {
            EditorPrefs.SetBool(gravityObject.gameObject.name + nameof(showDebugInfo), showDebugInfo);
        }
    }

    private static string[] GetGravityInfo(Vector3 point, CelestialBody ignore = null)
    {
        var bodies = FindObjectsOfType<CelestialBody>();
        var totalAcc = Vector3.zero;

        // gravity
        var forceAndName = new List<FloatAndString>();

        foreach (var body in bodies)
        {
            if (body != ignore)
            {
                var offsetToBody = body.Position - point;
                var sqrDst = offsetToBody.sqrMagnitude;
                var dst = Mathf.Sqrt(sqrDst);
                var dirToBody = offsetToBody / Mathf.Sqrt(sqrDst);
                var acceleration = Universe.gravitationalConstant * body.mass / sqrDst;
                totalAcc += dirToBody * acceleration;
                forceAndName.Add(new FloatAndString { floatVal = acceleration, stringVal = body.gameObject.name, });
            }
        }

        forceAndName.Sort((a, b) => b.floatVal.CompareTo(a.floatVal));
        var info = new string[forceAndName.Count + 1];
        info[0] = $"acc: {totalAcc} (mag = {totalAcc.magnitude})";

        for (var i = 0; i < forceAndName.Count; i++)
        {
            info[i + 1] = $"acceleration due to {forceAndName[i].stringVal}: {forceAndName[i].floatVal}";
        }

        return info;
    }

    private struct FloatAndString
    {
        public float floatVal;
        public string stringVal;
    }
}