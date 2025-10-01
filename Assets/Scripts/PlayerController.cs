using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float lookSensitivity = 2f;

    [Header("Camera & Hold Point")]
    public Camera playerCamera;
    public Transform holdPoint;
    public LayerMask interactMask;

    [Header("Package Bob/Sway")]
    public float bobAmplitude = 0.05f;   // vertical bob height
    public float bobFrequency = 2f;      // vertical bob speed
    public float swayAmount = 5f;        // max sway angle in degrees
    public float swaySpeed = 5f;         // speed of sway interpolation

    private CharacterController controller;
    private float xRotation = 0f;
    private GameObject heldObject;
    private Vector3 holdPointOriginalPos;
    private Quaternion holdPointOriginalRot;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        holdPointOriginalPos = holdPoint.localPosition;
        holdPointOriginalRot = holdPoint.localRotation;
    }

    void Update()
    {
        HandleLook();
        HandleMovement();
        HandleInteract();
        HandleHeldObjectBobAndSway();
    }

    // ----- Look -----
    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    // ----- Movement -----
    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;

        float adjustedSpeed = moveSpeed;
        if (heldObject != null)
        {
            Package pkg = heldObject.GetComponent<Package>();
            if (pkg != null)
                adjustedSpeed = Mathf.Max(1f, moveSpeed - pkg.weight);
        }

        controller.SimpleMove(move * adjustedSpeed);
    }

    // ----- Interact -----
    void HandleInteract()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null)
                TryPickUp();
            else
                DropObject();
        }
    }

    void TryPickUp()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (Physics.Raycast(ray, out RaycastHit hit, 3f, interactMask))
        {
            if (hit.collider.CompareTag("Package"))
            {
                heldObject = hit.collider.gameObject;
                Rigidbody rb = heldObject.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;

                // Save original rotation
                Quaternion originalRot = heldObject.transform.rotation;

                // Parent to hold point
                heldObject.transform.SetParent(holdPoint);
                heldObject.transform.localPosition = Vector3.zero;

                // Force upright but keep yaw
                heldObject.transform.rotation = Quaternion.Euler(0f, originalRot.eulerAngles.y, 0f);
            }
        }
    }

    void DropObject()
    {
        if (heldObject != null)
        {
            heldObject.transform.SetParent(null);
            Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;
            heldObject = null;

            // Reset hold point
            holdPoint.localPosition = holdPointOriginalPos;
            holdPoint.localRotation = holdPointOriginalRot;
        }
    }

    // ----- Bob + Sway -----
    void HandleHeldObjectBobAndSway()
    {
        if (heldObject != null)
        {
            // Bob
            float bob = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            holdPoint.localPosition = holdPointOriginalPos + new Vector3(0f, bob, 0f);

            // Sway based on input
            float targetX = -Input.GetAxis("Vertical") * swayAmount;
            float targetZ = Input.GetAxis("Horizontal") * swayAmount;

            Quaternion targetRot = Quaternion.Euler(targetX, holdPointOriginalRot.eulerAngles.y, targetZ);
            holdPoint.localRotation = Quaternion.Slerp(holdPoint.localRotation, targetRot, Time.deltaTime * swaySpeed);
        }
        else
        {
            // Reset when empty
            holdPoint.localPosition = holdPointOriginalPos;
            holdPoint.localRotation = holdPointOriginalRot;
        }
    }

    // Utility
    public bool IsHolding() => heldObject != null;
    public GameObject GetHeldObject() => heldObject;
}
