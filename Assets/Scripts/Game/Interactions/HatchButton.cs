public class HatchButton : Interactable
{
    protected override void ShowInteractMessage()
    {
        var ship = FindObjectOfType<Ship>();
        var action = ship.HatchOpen ? "close" : "open";
        var contextualMessage = $"Press F to {action} hatch";
        GameUI.DisplayInteractionInfo(contextualMessage);
    }

    protected override void Interact()
    {
        base.Interact();
        ShowInteractMessage();
    }

    private void OnValidate() => interactMessage = "#set from script#";
}