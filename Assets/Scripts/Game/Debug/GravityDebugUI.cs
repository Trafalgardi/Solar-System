using System.Collections.Generic;
using UnityEngine;

public class GravityDebugUI : MonoBehaviour
{
    private bool show;
    public TMPro.TMP_Text info;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            show = !show;
        }

        info.text = "";

        if (show)
        {
            var grav = GetGravityInfo(Camera.main.transform.position);

            for (var i = 0; i < grav.Length; i++)
            {
                info.text += grav[i] + "\n";
            }
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
        //info[0] = $"acceleration: {totalAcc.magnitude:0.00})";
        info[0] = "Acceleration due to bodies: (m/s^2)";

        for (var i = 0; i < forceAndName.Count; i++)
        {
            info[i + 1] =
                $"{forceAndName[i].stringVal}: {forceAndName[i].floatVal:0.00}".Replace(oldValue: ",", newValue: ".");
        }

        return info;
    }

    private struct FloatAndString
    {
        public float floatVal;
        public string stringVal;
    }
}