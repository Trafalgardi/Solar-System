using UnityEngine;

public class GameUI : MonoBehaviour
{
    private float interactionInfoDisplayTimeRemaining;
    private static GameUI instance;

    public TMPro.TMP_Text interactionInfo;

    public static void DisplayInteractionInfo(string info)
    {
        if (Instance)
        {
            Instance.interactionInfo.text = info;
            Instance.interactionInfoDisplayTimeRemaining = 3;
        }
        else
        {
            Debug.Log($"{info} (no UI instance found)");
        }
    }

    public static void CancelInteractionDisplay()
    {
        if (Instance)
        {
            Instance.interactionInfoDisplayTimeRemaining = 0;
        }
    }

    private static GameUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameUI>();
            }

            return instance;
        }
    }

    private void Update()
    {
        if (interactionInfo)
        {
            interactionInfoDisplayTimeRemaining -= Time.deltaTime;
            interactionInfo.enabled = interactionInfoDisplayTimeRemaining > 0;
        }
    }
}