using System.Collections.Generic;
using UnityEngine;

public class EndlessManager : MonoBehaviour
{
    private List<Transform> physicsObjects;
    private Ship ship;
    private PlayerController player;
    private Camera playerCamera;

    private void Awake()
    {
        var ship = FindObjectOfType<Ship>();
        var player = FindObjectOfType<PlayerController>();
        var bodies = FindObjectsOfType<CelestialBody>();

        physicsObjects = new List<Transform>();
        physicsObjects.Add(ship.transform);
        physicsObjects.Add(player.transform);

        foreach (var c in bodies)
        {
            physicsObjects.Add(c.transform);
        }

        playerCamera = Camera.main;
    }

    public float distanceThreshold = 1000;

    public event System.Action PostFloatingOriginUpdate;

    private void LateUpdate()
    {
        UpdateFloatingOrigin();

        if (PostFloatingOriginUpdate != null)
        {
            PostFloatingOriginUpdate();
        }
    }

    private void UpdateFloatingOrigin()
    {
        var originOffset = playerCamera.transform.position;
        var dstFromOrigin = originOffset.magnitude;

        if (dstFromOrigin > distanceThreshold)
        {
            foreach (var t in physicsObjects)
            {
                t.position -= originOffset;
            }
        }
    }
}