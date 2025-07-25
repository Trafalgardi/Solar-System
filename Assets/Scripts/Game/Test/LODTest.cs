using UnityEngine;

public class LODTest : MonoBehaviour
{
    private void Start()
    {
        // Programmatically create a LOD group and add LOD levels.
        // Create a GUI that allows for forcing a specific LOD level.
        group = gameObject.AddComponent<LODGroup>();

        // Add 4 LOD levels
        var lods = new LOD[4];

        for (var i = 0; i < 4; i++)
        {
            var primType = PrimitiveType.Cube;

            switch (i)
            {
                case 1: primType = PrimitiveType.Capsule; break;
                case 2: primType = PrimitiveType.Sphere; break;
                case 3: primType = PrimitiveType.Cylinder; break;
            }

            var go = GameObject.CreatePrimitive(primType);
            go.transform.parent = gameObject.transform;
            var renderers = new Renderer[1];
            renderers[0] = go.GetComponent<Renderer>();
            var v = 1.0F / (i + 1);
            lods[i] = new LOD(v, renderers);
            Debug.Log(i + " screenrelative transition height: " + v);
        }

        group.SetLODs(lods);
        group.RecalculateBounds();
    }

    public LODGroup group;
    public float[] vals;
    public bool useVals;

    private void Update()
    {
    }

    private void OnGUI()
    {
        if (GUILayout.Button(text: "Enable / Disable"))
        {
            group.enabled = !group.enabled;
        }

        if (GUILayout.Button(text: "Default"))
        {
            group.ForceLOD(index: -1);
        }

        if (GUILayout.Button(text: "Force 0"))
        {
            group.ForceLOD(index: 0);
        }

        if (GUILayout.Button(text: "Force 1"))
        {
            group.ForceLOD(index: 1);
        }

        if (GUILayout.Button(text: "Force 2"))
        {
            group.ForceLOD(index: 2);
        }

        if (GUILayout.Button(text: "Force 3"))
        {
            group.ForceLOD(index: 3);
        }

        if (GUILayout.Button(text: "Force 4"))
        {
            group.ForceLOD(index: 4);
        }

        if (GUILayout.Button(text: "Force 5"))
        {
            group.ForceLOD(index: 5);
        }

        if (GUILayout.Button(text: "Force 6"))
        {
            group.ForceLOD(index: 6);
        }
    }
}