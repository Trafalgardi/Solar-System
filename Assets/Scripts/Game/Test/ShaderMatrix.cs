using UnityEngine;

[ExecuteInEditMode]
public class ShaderMatrix : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        GetComponent<MeshRenderer>().sharedMaterial.SetVector(name: "ihat", transform.right);
        GetComponent<MeshRenderer>().sharedMaterial.SetVector(name: "jhat", transform.up);
        GetComponent<MeshRenderer>().sharedMaterial.SetVector(name: "khat", transform.forward);
        Debug.Log(transform.right);
    }
}