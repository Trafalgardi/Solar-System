using UnityEngine;

public class Interactable : MonoBehaviour
{
    protected bool playerInInteractionZone;

    public string interactMessage = "Press F to interact";
    public UnityEngine.Events.UnityEvent interactEvent;

    public void ForcePlayerInInteractionZone() => playerInInteractionZone = true;

    protected virtual void Interact()
    {
        if (interactEvent != null)
        {
            interactEvent.Invoke();
        }
    }

    protected virtual void Update()
    {
        if (playerInInteractionZone && Input.GetKeyDown(KeyCode.F))
        {
            GameUI.CancelInteractionDisplay();
            Interact();
        }
    }

    protected virtual void OnTriggerEnter(Collider c)
    {
        if (c.tag == "Player")
        {
            playerInInteractionZone = true;
            ShowInteractMessage();
        }
    }

    protected virtual void OnTriggerExit(Collider c)
    {
        if (c.tag == "Player")
        {
            GameUI.CancelInteractionDisplay();
            playerInInteractionZone = false;
        }
    }

    protected virtual void ShowInteractMessage() => GameUI.DisplayInteractionInfo(interactMessage);
}