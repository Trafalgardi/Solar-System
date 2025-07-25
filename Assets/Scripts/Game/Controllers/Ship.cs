using UnityEngine;

public class Ship : GravityObject
{
    private Quaternion targetRot;
    private Quaternion smoothedRot;

    private Vector3 thrusterInput;
    private PlayerController pilot;
    private int numCollisionTouches;

    private readonly KeyCode ascendKey = KeyCode.Space;
    private readonly KeyCode descendKey = KeyCode.LeftShift;
    private readonly KeyCode rollCounterKey = KeyCode.Q;
    private readonly KeyCode rollClockwiseKey = KeyCode.E;
    private readonly KeyCode forwardKey = KeyCode.W;
    private readonly KeyCode backwardKey = KeyCode.S;
    private readonly KeyCode leftKey = KeyCode.A;
    private readonly KeyCode rightKey = KeyCode.D;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        
        InitRigidbody();
        targetRot = transform.rotation;
        smoothedRot = transform.rotation;
        inputSettings.Begin();
    }

    public InputSettings inputSettings;
    public Transform hatch;
    public float hatchAngle;
    public Transform camViewPoint;
    public Transform pilotSeatPoint;
    public LayerMask groundedMask;
    public GameObject window;

    [Header(header: "Handling")] public float thrustStrength = 20;

    public float rotSpeed = 5;
    public float rollSpeed = 30;
    public float rotSmoothSpeed = 10;

    [Header(header: "Interact")] public Interactable flightControls;

    public bool ShowHUD { get; private set; }

    public bool HatchOpen { get; private set; }

    public bool IsPiloted => ShowHUD;

    public Rigidbody Rigidbody { get; private set; }

    public void ToggleHatch() => HatchOpen = !HatchOpen;
    

    public void TogglePiloting()
    {
        if (ShowHUD)
        {
            StopPilotingShip();
        }
        else
        {
            PilotShip();
        }
    }

    public void PilotShip()
    {
        pilot = FindObjectOfType<PlayerController>();
        ShowHUD = true;
        pilot.Camera.transform.parent = camViewPoint;
        pilot.Camera.transform.localPosition = Vector3.zero;
        pilot.Camera.transform.localRotation = Quaternion.identity;
        pilot.gameObject.SetActive(value: false);
        HatchOpen = false;
        window.SetActive(value: false);
    }

    public void SetVelocity(Vector3 velocity) => Rigidbody.linearVelocity = velocity;

    private void Update()
    {
        if (ShowHUD)
        {
            HandleMovement();
        }

        // Animate hatch
        var hatchTargetAngle = HatchOpen ? hatchAngle : 0;

        hatch.localEulerAngles =
            Vector3.right * Mathf.LerpAngle(hatch.localEulerAngles.x, hatchTargetAngle, Time.deltaTime);

        HandleCheats();
    }

    private void HandleMovement()
    {
        float thrustInputX, thrustInputY, thrustInputZ;
        float yawInput, pitchInput, rollInput;

        if (Application.isMobilePlatform)
        {
            // Thrusters
            var move = MobileInput.Instance.moveInput;
            thrustInputX = move.x;
            thrustInputZ = move.y;
            thrustInputY = 0; // можно потом кнопками сделать подъём/спуск

            // Rotation
            var look = MobileInput.Instance.lookInput;
            yawInput = look.x * rotSpeed * inputSettings.mouseSensitivity / 10f;
            pitchInput = look.y * rotSpeed * inputSettings.mouseSensitivity / 10f;
            rollInput = MobileInput.Instance.rollInput * rollSpeed * Time.deltaTime;
        }
        else
        {
            thrustInputX = GetInputAxis(leftKey, rightKey);
            thrustInputY = GetInputAxis(descendKey, ascendKey);
            thrustInputZ = GetInputAxis(backwardKey, forwardKey);

            yawInput = Input.GetAxisRaw("Mouse X") * rotSpeed * inputSettings.mouseSensitivity / 100f;
            pitchInput = Input.GetAxisRaw("Mouse Y") * rotSpeed * inputSettings.mouseSensitivity / 100f;
            rollInput = GetInputAxis(rollCounterKey, rollClockwiseKey) * rollSpeed * Time.deltaTime;
        }

        thrusterInput = new Vector3(thrustInputX, thrustInputY, thrustInputZ);

        // Calculate rotation
        if (numCollisionTouches == 0)
        {
            var yaw = Quaternion.AngleAxis(yawInput, transform.up);
            var pitch = Quaternion.AngleAxis(-pitchInput, transform.right);
            var roll = Quaternion.AngleAxis(-rollInput, transform.forward);

            targetRot = yaw * pitch * roll * targetRot;
            smoothedRot = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotSmoothSpeed);
        }
        else
        {
            targetRot = transform.rotation;
            smoothedRot = transform.rotation;
        }
    }


    private void FixedUpdate()
    {
        // Gravity
        var gravity = NBodySimulation.CalculateAcceleration(Rigidbody.position);
        Rigidbody.AddForce(gravity, ForceMode.Acceleration);

        // Thrusters
        var thrustDir = transform.TransformVector(thrusterInput);
        Rigidbody.AddForce(thrustDir * thrustStrength, ForceMode.Acceleration);

        if (numCollisionTouches == 0)
        {
            Rigidbody.MoveRotation(smoothedRot);
        }
    }

    private void TeleportToBody(CelestialBody body)
    {
        Rigidbody.linearVelocity = body.velocity;

        Rigidbody.MovePosition(body.transform.position +
                               (transform.position - body.transform.position).normalized * body.radius * 2);
    }

    private int GetInputAxis(KeyCode negativeAxis, KeyCode positiveAxis)
    {
        var axis = 0;

        if (Input.GetKey(positiveAxis))
        {
            axis++;
        }

        if (Input.GetKey(negativeAxis))
        {
            axis--;
        }

        return axis;
    }

    private void HandleCheats()
    {
        if (Universe.cheatsEnabled)
        {
            if (Input.GetKeyDown(KeyCode.Return) && IsPiloted && Time.timeScale != 0)
            {
                var shipHud = FindObjectOfType<ShipHUD>();

                if (shipHud.LockedBody)
                {
                    TeleportToBody(shipHud.LockedBody);
                }
            }
        }
    }

    private void InitRigidbody()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        Rigidbody.useGravity = false;
        Rigidbody.isKinematic = false;
        Rigidbody.centerOfMass = Vector3.zero;
        Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    private void StopPilotingShip()
    {
        ShowHUD = false;
        pilot.transform.position = pilotSeatPoint.position;
        pilot.transform.rotation = pilotSeatPoint.rotation;
        pilot.Rigidbody.linearVelocity = Rigidbody.linearVelocity;
        pilot.gameObject.SetActive(value: true);
        window.SetActive(value: true);
        pilot.ExitFromSpaceship();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (groundedMask == (groundedMask | (1 << other.gameObject.layer)))
        {
            numCollisionTouches++;
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (groundedMask == (groundedMask | (1 << other.gameObject.layer)))
        {
            numCollisionTouches--;
        }
    }
}