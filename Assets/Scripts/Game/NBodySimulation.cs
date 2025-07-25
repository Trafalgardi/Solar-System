using UnityEngine;

public class NBodySimulation : MonoBehaviour
{
    private CelestialBody[] bodies;
    private static NBodySimulation instance;

    private void Awake()
    {
        bodies = FindObjectsOfType<CelestialBody>();
        Time.fixedDeltaTime = Universe.physicsTimeStep;
        Debug.Log("Setting fixedDeltaTime to: " + Universe.physicsTimeStep);
    }

    public static CelestialBody[] Bodies => Instance.bodies;

    public static Vector3 CalculateAcceleration(Vector3 point, CelestialBody ignoreBody = null)
    {
        var acceleration = Vector3.zero;

        foreach (var body in Instance.bodies)
        {
            if (body != ignoreBody)
            {
                var sqrDst = (body.Position - point).sqrMagnitude;
                var forceDir = (body.Position - point).normalized;
                acceleration += forceDir * Universe.gravitationalConstant * body.mass / sqrDst;
            }
        }

        return acceleration;
    }

    private static NBodySimulation Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<NBodySimulation>();
            }

            return instance;
        }
    }

    private void FixedUpdate()
    {
        for (var i = 0; i < bodies.Length; i++)
        {
            var acceleration = CalculateAcceleration(bodies[i].Position, bodies[i]);
            bodies[i].UpdateVelocity(acceleration, Universe.physicsTimeStep);
            //bodies[i].UpdateVelocity (bodies, Universe.physicsTimeStep);
        }

        for (var i = 0; i < bodies.Length; i++)
        {
            bodies[i].UpdatePosition(Universe.physicsTimeStep);
        }
    }
}