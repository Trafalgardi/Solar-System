using UnityEngine;

public class CheatCodes : MonoBehaviour
{
    public bool cheatsEnabled = true;
    public bool disableInBuild = true;

    public KeyCode flyShip = KeyCode.Plus;

    private void Update()
    {
        if ((Application.isEditor || !disableInBuild) && Application.isPlaying && cheatsEnabled)
        {
            if (Input.GetKeyDown(flyShip))
            {
                // FindObjectOfType<Ship> ().StartFlying (FindObjectOfType<PlayerController> ());
            }
        }
    }
}