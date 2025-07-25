using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MobileInput : MonoBehaviour
{
    public static MobileInput Instance { get; private set; }

    [Header("Joysticks")]
    [SerializeField] private VariableJoystick moveJoystick;
    [SerializeField] private VariableJoystick lookJoystick;

    [Header("Buttons")]
    [SerializeField] private Button jumpButton;
    [SerializeField] private EventTrigger jetpackButton;

    [Header("Ship Roll Buttons")]
    [SerializeField] private EventTrigger rollLeftButton;
    [SerializeField] private EventTrigger rollRightButton;

    [HideInInspector] public float rollInput;

    [Header("Output")]
    public Vector2 moveInput;
    public Vector2 lookInput;
    public bool jumpPressed;
    public bool jetpackHeld;

    // Держим явные делегаты для корректного Remove
    private UnityEngine.Events.UnityAction<BaseEventData> _jetpackDown;
    private UnityEngine.Events.UnityAction<BaseEventData> _jetpackUp;
    private UnityEngine.Events.UnityAction<BaseEventData> _rollLeftDown;
    private UnityEngine.Events.UnityAction<BaseEventData> _rollLeftUp;
    private UnityEngine.Events.UnityAction<BaseEventData> _rollRightDown;
    private UnityEngine.Events.UnityAction<BaseEventData> _rollRightUp;

    private void Awake()
    {
        Instance = this;
        Debug.Log("MobileInput Awake");
        // Делегаты заранее
        _jetpackDown = OnJetpackDown;
        _jetpackUp = OnJetpackUp;
        _rollLeftDown = e => { rollInput = -1f; Debug.Log("Roll Left Down"); };
        _rollLeftUp = e => { rollInput = 0f; Debug.Log("Roll Left Up"); };
        _rollRightDown = e => { rollInput = 1f; Debug.Log("Roll Right Down"); };
        _rollRightUp = e => { rollInput = 0f; Debug.Log("Roll Right Up"); };
    }

    private void OnEnable()
    {
        Debug.Log("MobileInput OnEnable");
        if (jumpButton != null)
        {
            jumpButton.onClick.AddListener(OnJumpDown);
            Debug.Log("JumpButton listener added");
        }

        if (jetpackButton != null)
        {
            AddEventTrigger(jetpackButton, EventTriggerType.PointerDown, _jetpackDown);
            AddEventTrigger(jetpackButton, EventTriggerType.PointerUp, _jetpackUp);
        }

        if (rollLeftButton != null)
        {
            AddEventTrigger(rollLeftButton, EventTriggerType.PointerDown, _rollLeftDown);
            AddEventTrigger(rollLeftButton, EventTriggerType.PointerUp, _rollLeftUp);
        }

        if (rollRightButton != null)
        {
            AddEventTrigger(rollRightButton, EventTriggerType.PointerDown, _rollRightDown);
            AddEventTrigger(rollRightButton, EventTriggerType.PointerUp, _rollRightUp);
        }
    }

    private void OnDisable()
    {
        Debug.Log("MobileInput OnDisable");
        if (jumpButton != null)
        {
            jumpButton.onClick.RemoveListener(OnJumpDown);
            Debug.Log("JumpButton listener removed");
        }

        if (jetpackButton != null)
        {
            RemoveEventTrigger(jetpackButton, EventTriggerType.PointerDown, _jetpackDown);
            RemoveEventTrigger(jetpackButton, EventTriggerType.PointerUp, _jetpackUp);
        }

        if (rollLeftButton != null)
        {
            RemoveEventTrigger(rollLeftButton, EventTriggerType.PointerDown, _rollLeftDown);
            RemoveEventTrigger(rollLeftButton, EventTriggerType.PointerUp, _rollLeftUp);
        }

        if (rollRightButton != null)
        {
            RemoveEventTrigger(rollRightButton, EventTriggerType.PointerDown, _rollRightDown);
            RemoveEventTrigger(rollRightButton, EventTriggerType.PointerUp, _rollRightUp);
        }
    }

    private void Update()
    {
        moveInput = moveJoystick.Direction;
        lookInput = lookJoystick.Direction;

        //если джойстики не используются, то их значения будут (0,0) лог не выводим
        if (moveInput == Vector2.zero && lookInput == Vector2.zero)
        {
            return;
        }
        
        Debug.Log($"Update: moveInput={moveInput}, lookInput={lookInput}, rollInput={rollInput}, jumpPressed={jumpPressed}, jetpackHeld={jetpackHeld}");
    }

    public void ClearFrameInputs()
    {
        jumpPressed = false;
        // jetpackHeld НЕ сбрасывается здесь — это удержание
    }

    private void OnJumpDown()
    {
        jumpPressed = true;
        Debug.Log("Jump Pressed");
    }

    private void OnJetpackDown(BaseEventData _)
    {
        jetpackHeld = true;
        Debug.Log("Jetpack Down (held true)");
    }
    private void OnJetpackUp(BaseEventData _)
    {
        jetpackHeld = false;
        Debug.Log("Jetpack Up (held false)");
    }

    private void AddEventTrigger(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> callback)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener(callback);
        trigger.triggers.Add(entry);
        Debug.Log($"EventTrigger {trigger.name} {type} added [{callback.Method.Name}]");
    }

    private void RemoveEventTrigger(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> callback)
    {
        int removed = trigger.triggers.RemoveAll(e =>
            e.eventID == type &&
            e.callback != null &&
            // сравниваем по самой ссылке делегата
            e.callback.GetPersistentEventCount() > 0 &&
            e.callback.GetPersistentTarget(0) == callback.Target &&
            e.callback.GetPersistentMethodName(0) == callback.Method.Name
        );
        Debug.Log($"EventTrigger {trigger.name} {type} removed [{callback.Method.Name}], removed count = {removed}");
    }
}
