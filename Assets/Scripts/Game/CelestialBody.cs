using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Rigidbody))]
public class CelestialBody : GravityObject
{
    private Transform meshHolder;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        velocity = initialVelocity;
        RecalculateMass();
    }

    public BodyType bodyType;
    public float radius;
    public float surfaceGravity;
    public Vector3 initialVelocity;
    public string bodyName = "Unnamed";

    public Vector3 velocity { get; private set; }

    public float mass { get; private set; }

    public Rigidbody Rigidbody
    {
        get
        {
            if (!rb)
            {
                rb = GetComponent<Rigidbody>();
            }

            return rb;
        }
    }

    public Vector3 Position => rb.position;

    public void UpdateVelocity(CelestialBody[] allBodies, float timeStep)
    {
        foreach (var otherBody in allBodies)
        {
            if (otherBody != this)
            {
                var sqrDst = (otherBody.rb.position - rb.position).sqrMagnitude;
                var forceDir = (otherBody.rb.position - rb.position).normalized;

                var acceleration = forceDir * Universe.gravitationalConstant * otherBody.mass / sqrDst;
                velocity += acceleration * timeStep;
            }
        }
    }

    public void UpdateVelocity(Vector3 acceleration, float timeStep) => velocity += acceleration * timeStep;

    public void UpdatePosition(float timeStep) => rb.MovePosition(rb.position + velocity * timeStep);

    public void RecalculateMass()
    {
        mass = surfaceGravity * radius * radius / Universe.gravitationalConstant;
        Rigidbody.mass = mass;
    }

    private void OnValidate()
    {
        RecalculateMass();

        if (GetComponentInChildren<CelestialBodyGenerator>())
        {
            GetComponentInChildren<CelestialBodyGenerator>().transform.localScale = Vector3.one * radius;
        }

        gameObject.name = bodyName;
    }

    public enum BodyType
    {
        Planet,
        Moon,
        Sun,
    }
}