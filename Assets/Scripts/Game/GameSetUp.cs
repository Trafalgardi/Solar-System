using UnityEngine;

public class GameSetUp : MonoBehaviour
{
    private void Start()
    {
        var ship = FindObjectOfType<Ship>();
        var player = FindObjectOfType<PlayerController>();

        if (startCondition == StartCondition.InShip)
        {
            ship.PilotShip();
            ship.flightControls.ForcePlayerInInteractionZone();
        }
        else if (startCondition == StartCondition.OnBody)
        {
            if (startBody)
            {
                var pointAbovePlanet = startBody.transform.position + Vector3.right * startBody.radius * 1.1f;
                player.transform.position = pointAbovePlanet;
                player.SetVelocity(startBody.initialVelocity);
                ship.transform.position = pointAbovePlanet + Vector3.right * 20;
                ship.SetVelocity(startBody.initialVelocity);
                ship.ToggleHatch();
            }
        }
    }

    public StartCondition startCondition;
    public CelestialBody startBody;

    public enum StartCondition
    {
        InShip,
        OnBody,
    }
}