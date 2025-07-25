using UnityEngine;

public class CamTest : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Rigidbody>().linearVelocity = initial;

        if (setTimestep)
        {
        }
    }

    public bool orient;
    public bool setTimestep;
    public float physicsStep = 0.2f;
    public Vector3 initial;

    private void FixedUpdate()
    {
        Debug.Log(GetComponent<Rigidbody>().linearVelocity);
        GetComponent<Rigidbody>().position += GetComponent<Rigidbody>().linearVelocity * Time.deltaTime;
    }
}