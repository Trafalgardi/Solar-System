using UnityEngine;

[ExecuteInEditMode]
public class OrbitDebugDisplay : MonoBehaviour
{
    private void Start()
    {
        if (Application.isPlaying)
        {
            HideOrbits();
        }
    }

    public int numSteps = 1000;
    public float timeStep = 0.1f;
    public bool usePhysicsTimeStep;

    public bool relativeToBody;
    public CelestialBody centralBody;
    public float width = 100;
    public bool useThickLines;

    private void Update()
    {
        if (!Application.isPlaying)
        {
            DrawOrbits();
        }
    }

    private void DrawOrbits()
    {
        var bodies = FindObjectsOfType<CelestialBody>();
        var virtualBodies = new VirtualBody[bodies.Length];
        var drawPoints = new Vector3[bodies.Length][];
        var referenceFrameIndex = 0;
        var referenceBodyInitialPosition = Vector3.zero;

        // Initialize virtual bodies (don't want to move the actual bodies)
        for (var i = 0; i < virtualBodies.Length; i++)
        {
            virtualBodies[i] = new VirtualBody(bodies[i]);
            drawPoints[i] = new Vector3[numSteps];

            if (bodies[i] == centralBody && relativeToBody)
            {
                referenceFrameIndex = i;
                referenceBodyInitialPosition = virtualBodies[i].position;
            }
        }

        // Simulate
        for (var step = 0; step < numSteps; step++)
        {
            var referenceBodyPosition = relativeToBody ? virtualBodies[referenceFrameIndex].position : Vector3.zero;

            // Update velocities
            for (var i = 0; i < virtualBodies.Length; i++)
            {
                virtualBodies[i].velocity += CalculateAcceleration(i, virtualBodies) * timeStep;
            }

            // Update positions
            for (var i = 0; i < virtualBodies.Length; i++)
            {
                var newPos = virtualBodies[i].position + virtualBodies[i].velocity * timeStep;
                virtualBodies[i].position = newPos;

                if (relativeToBody)
                {
                    var referenceFrameOffset = referenceBodyPosition - referenceBodyInitialPosition;
                    newPos -= referenceFrameOffset;
                }

                if (relativeToBody && i == referenceFrameIndex)
                {
                    newPos = referenceBodyInitialPosition;
                }

                drawPoints[i][step] = newPos;
            }
        }

        // Draw paths
        for (var bodyIndex = 0; bodyIndex < virtualBodies.Length; bodyIndex++)
        {
            var pathColour =
                bodies[bodyIndex].gameObject.GetComponentInChildren<MeshRenderer>().sharedMaterial.color; //

            if (useThickLines)
            {
                var lineRenderer = bodies[bodyIndex].gameObject.GetComponentInChildren<LineRenderer>();
                lineRenderer.enabled = true;
                lineRenderer.positionCount = drawPoints[bodyIndex].Length;
                lineRenderer.SetPositions(drawPoints[bodyIndex]);
                lineRenderer.startColor = pathColour;
                lineRenderer.endColor = pathColour;
                lineRenderer.widthMultiplier = width;
            }
            else
            {
                for (var i = 0; i < drawPoints[bodyIndex].Length - 1; i++)
                {
                    Debug.DrawLine(drawPoints[bodyIndex][i], drawPoints[bodyIndex][i + 1], pathColour);
                }

                // Hide renderer
                var lineRenderer = bodies[bodyIndex].gameObject.GetComponentInChildren<LineRenderer>();

                if (lineRenderer)
                {
                    lineRenderer.enabled = false;
                }
            }
        }
    }

    private Vector3 CalculateAcceleration(int i, VirtualBody[] virtualBodies)
    {
        var acceleration = Vector3.zero;

        for (var j = 0; j < virtualBodies.Length; j++)
        {
            if (i == j)
            {
                continue;
            }

            var forceDir = (virtualBodies[j].position - virtualBodies[i].position).normalized;
            var sqrDst = (virtualBodies[j].position - virtualBodies[i].position).sqrMagnitude;
            acceleration += forceDir * Universe.gravitationalConstant * virtualBodies[j].mass / sqrDst;
        }

        return acceleration;
    }

    private void HideOrbits()
    {
        var bodies = FindObjectsOfType<CelestialBody>();

        // Draw paths
        for (var bodyIndex = 0; bodyIndex < bodies.Length; bodyIndex++)
        {
            var lineRenderer = bodies[bodyIndex].gameObject.GetComponentInChildren<LineRenderer>();
            lineRenderer.positionCount = 0;
        }
    }

    private void OnValidate()
    {
        if (usePhysicsTimeStep)
        {
            timeStep = Universe.physicsTimeStep;
        }
    }

    private class VirtualBody
    {
        public VirtualBody(CelestialBody body)
        {
            position = body.transform.position;
            velocity = body.initialVelocity;
            mass = body.mass;
        }

        public Vector3 position;
        public Vector3 velocity;
        public readonly float mass;
    }
}