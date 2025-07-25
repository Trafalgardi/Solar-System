using UnityEngine;

public class PlayerController : GravityObject
{
    // Private
    public Ship spaceship;

    private float yaw;
    private float pitch;
    private float smoothYaw;
    private float smoothPitch;

    private float yawSmoothV;
    private float pitchSmoothV;

    private Vector3 targetVelocity;
    private Vector3 cameraLocalPos;
    private Vector3 smoothVelocity;
    private Vector3 smoothVRef;

    // Jetpack
    private bool usingJetpack;
    private float jetpackFuelPercent = 1;
    private float lastJetpackUseTime;

    private CelestialBody referenceBody;

    private bool readyToFlyShip;
    private bool debug_playerFrozen;
    private Animator animator;

    private void Awake()
    {
        Camera = GetComponentInChildren<Camera>();
        
        cameraLocalPos = Camera.transform.localPosition;
        InitRigidbody();
        
        animator = GetComponentInChildren<Animator>();
        
        inputSettings.Begin();
    }

    // Exposed variables
    [Header(header: "Movement settings")] public float walkSpeed = 8;

    public float runSpeed = 14;
    public float jumpForce = 20;
    public float vSmoothTime = 0.1f;
    public float airSmoothTime = 0.5f;
    public float stickToGroundForce = 8;

    public float jetpackForce = 10;
    public float jetpackDuration = 2;
    public float jetpackRefuelTime = 2;
    public float jetpackRefuelDelay = 2;

    [Header(header: "Mouse settings")] public float mouseSensitivityMultiplier = 1;

    public float maxMouseSmoothTime = 0.3f;
    public Vector2 pitchMinMax = new(x: -40, y: 85);
    public InputSettings inputSettings;

    [Header(header: "Other")] public float mass = 70;

    public LayerMask walkableMask;
    public Transform feet;

    public Camera Camera { get; private set; }

    public Rigidbody Rigidbody { get; private set; }

    public void SetVelocity(Vector3 velocity) => Rigidbody.linearVelocity = velocity;
    

    public void ExitFromSpaceship()
    {
        Camera.transform.parent = transform;
        Camera.transform.localPosition = cameraLocalPos;
        smoothYaw = 0;
        yaw = 0;
        smoothPitch = Camera.transform.localEulerAngles.x;
        pitch = smoothPitch;
    }

    private void InitRigidbody()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        Rigidbody.useGravity = false;
        Rigidbody.isKinematic = false;
        Rigidbody.mass = mass;
    }

    private void Update()
    {
        HandleMovement();
        
        if (Application.isMobilePlatform)
            MobileInput.Instance.ClearFrameInputs();
    }

    private void HandleMovement()
    {
        HandleEditorInput();

        if (Time.timeScale == 0)
        {
            return;
        }

        // Look input
        float lookX = Application.isMobilePlatform ? MobileInput.Instance.lookInput.x : Input.GetAxisRaw("Mouse X");
        float lookY = Application.isMobilePlatform ? MobileInput.Instance.lookInput.y : Input.GetAxisRaw("Mouse Y");
        yaw += lookX * inputSettings.mouseSensitivity / 10 * mouseSensitivityMultiplier;
        pitch -= lookY * inputSettings.mouseSensitivity / 10 * mouseSensitivityMultiplier;

        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);
        var mouseSmoothTime = Mathf.Lerp(a: 0.01f, maxMouseSmoothTime, inputSettings.mouseSmoothing);
        smoothPitch = Mathf.SmoothDampAngle(smoothPitch, pitch, ref pitchSmoothV, mouseSmoothTime);
        var smoothYawOld = smoothYaw;
        smoothYaw = Mathf.SmoothDampAngle(smoothYaw, yaw, ref yawSmoothV, mouseSmoothTime);

        if (!debug_playerFrozen && Time.timeScale > 0)
        {
            Camera.transform.localEulerAngles = Vector3.right * smoothPitch;
            transform.Rotate(Vector3.up * Mathf.DeltaAngle(smoothYawOld, smoothYaw), Space.Self);
        }

        // Movement
        var isGrounded = IsGrounded();
        Vector3 input = Application.isMobilePlatform
            ? new Vector3(MobileInput.Instance.moveInput.x, 0, MobileInput.Instance.moveInput.y)
            : new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        var running = Input.GetKey(KeyCode.LeftShift);
        targetVelocity = transform.TransformDirection(input.normalized) * (running ? runSpeed : walkSpeed);

        smoothVelocity = Vector3.SmoothDamp(smoothVelocity, targetVelocity, ref smoothVRef,
            isGrounded ? vSmoothTime : airSmoothTime);

        bool jumpPressed = Application.isMobilePlatform ? MobileInput.Instance.jumpPressed : Input.GetKeyDown(KeyCode.Space);
        bool jetpackHeld = Application.isMobilePlatform ? MobileInput.Instance.jetpackHeld : Input.GetKey(KeyCode.Space);

        if (isGrounded)
        {
            if (jumpPressed)
            {
                Rigidbody.AddForce(transform.up * jumpForce, ForceMode.VelocityChange);
                isGrounded = false;
            }
            else
            {
                // Apply small downward force to prevent bouncing on slopes
                Rigidbody.AddForce(-transform.up * stickToGroundForce, ForceMode.VelocityChange);
            }
        }
        else
        {
            // Start jetpack on jumpPressed (tap), hold with jetpackHeld
            if (jumpPressed)
            {
                usingJetpack = true;
            }
        }

        if (usingJetpack && jetpackHeld && jetpackFuelPercent > 0)
        {
            lastJetpackUseTime = Time.time;
            jetpackFuelPercent -= Time.deltaTime / jetpackDuration;
            Rigidbody.AddForce(transform.up * jetpackForce, ForceMode.Acceleration);
        }
        else
        {
            usingJetpack = false;
        }


        // Refuel jetpack
        if (Time.time - lastJetpackUseTime > jetpackRefuelDelay)
        {
            jetpackFuelPercent = Mathf.Clamp01(jetpackFuelPercent + Time.deltaTime / jetpackRefuelTime);
        }

        // Handle animations
        var currentSpeed = smoothVelocity.magnitude;
        var animationSpeedPercent = currentSpeed <= walkSpeed ? currentSpeed / walkSpeed / 2 : currentSpeed / runSpeed;
        animator.SetBool(name: "Grounded", isGrounded);
        animator.SetFloat(name: "Speed", animationSpeedPercent);
    }

    private bool IsGrounded()
    {
        // Sphere must not overlay terrain at origin otherwise no collision will be detected
        // so rayRadius should not be larger than controller's capsule collider radius
        const float rayRadius = .3f;
        const float groundedRayDst = .2f;
        var grounded = false;

        if (referenceBody)
        {
            var relativeVelocity = Rigidbody.linearVelocity - referenceBody.velocity;

            // Don't cast ray down if player is jumping up from surface
            if (relativeVelocity.y <= jumpForce * .5f)
            {
                RaycastHit hit;
                var offsetToFeet = feet.position - transform.position;
                var rayOrigin = Rigidbody.position + offsetToFeet + transform.up * rayRadius;
                var rayDir = -transform.up;

                grounded = Physics.SphereCast(rayOrigin, rayRadius, rayDir, out hit, groundedRayDst, walkableMask);
            }
        }

        return grounded;
    }

    private void FixedUpdate()
    {
        var bodies = NBodySimulation.Bodies;
        var gravityOfNearestBody = Vector3.zero;
        var nearestSurfaceDst = float.MaxValue;

        // Gravity
        foreach (var body in bodies)
        {
            var sqrDst = (body.Position - Rigidbody.position).sqrMagnitude;
            var forceDir = (body.Position - Rigidbody.position).normalized;
            var acceleration = forceDir * Universe.gravitationalConstant * body.mass / sqrDst;
            Rigidbody.AddForce(acceleration, ForceMode.Acceleration);

            var dstToSurface = Mathf.Sqrt(sqrDst) - body.radius;

            // Find body with strongest gravitational pull 
            if (dstToSurface < nearestSurfaceDst)
            {
                nearestSurfaceDst = dstToSurface;
                gravityOfNearestBody = acceleration;
                referenceBody = body;
            }
        }

        // Rotate to align with gravity up
        var gravityUp = -gravityOfNearestBody.normalized;
        Rigidbody.rotation = Quaternion.FromToRotation(transform.up, gravityUp) * Rigidbody.rotation;

        // Move
        Rigidbody.MovePosition(Rigidbody.position + smoothVelocity * Time.fixedDeltaTime);
    }

    private void HandleEditorInput()
    {
        if (Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                Debug.Log(message: "Debug mode: Toggle freeze player");
                debug_playerFrozen = !debug_playerFrozen;
            }
        }
    }
}