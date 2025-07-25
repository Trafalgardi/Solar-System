using UnityEngine;

[ExecuteInEditMode]
public class SunTest : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        FindObjectOfType<Light>().transform.forward = -transform.position.normalized;
        FindObjectOfType<Light>().transform.position = transform.position;
    }
}