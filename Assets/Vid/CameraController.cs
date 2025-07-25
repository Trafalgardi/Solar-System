using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Rotation state
    private Vector2 rotInput;

    // Zoom state
    private float targetZoomDst;
    private float currentZoomDst;

    private void Start()
    {
        rotInput = transform.eulerAngles;
        targetZoomDst = (transform.position - pivot.position).magnitude;
        currentZoomDst = targetZoomDst;
    }

    public Transform pivot;

    [Header(header: "Rotation (alt-drag)")]
    public float rotSpeed = 6;

    public float rotSmoothing = 10;

    [Header(header: "Zoom - (alt-ctrl-drag)")]
    public float zoomSpeed = 6;

    public float zoomSmoothing = 10;

    private void LateUpdate()
    {
        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(button: 0))
        {
            var mouseInput = new Vector2(Input.GetAxisRaw(axisName: "Mouse X"), Input.GetAxisRaw(axisName: "Mouse Y"));

            if (Input.GetKey(KeyCode.LeftControl))
            {
                HandleZoomInput(mouseInput);
            }
            else
            {
                HandleRotationInput(mouseInput);
            }
        }

        UpdateRotation();
        UpdateZoom();
    }

    private void HandleRotationInput(Vector2 mouseInput)
        => rotInput += new Vector2(-mouseInput.y, mouseInput.x) * rotSpeed;

    private void UpdateRotation()
    {
        var targetRot = Quaternion.Euler(rotInput.x, rotInput.y, z: 0);
        var rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotSmoothing);
        var position = rotation * Vector3.forward * -(pivot.position - transform.position).magnitude + pivot.position;

        transform.rotation = rotation;
        transform.position = position;
    }

    private void HandleZoomInput(Vector2 mouseInput)
    {
        var zoomDir = -Mathf.Sign(mouseInput.x);
        targetZoomDst += mouseInput.magnitude * zoomSpeed * zoomDir;
    }

    private void UpdateZoom()
    {
        currentZoomDst = Mathf.Lerp(currentZoomDst, targetZoomDst, Time.deltaTime * zoomSmoothing);
        var dirToPivot = (pivot.transform.position - transform.position).normalized;
        transform.position = pivot.transform.position - dirToPivot * currentZoomDst;
    }
}