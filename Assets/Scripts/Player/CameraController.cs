using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private Transform cameraPivot;

    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float maxPitch = 85f;

    [SerializeField] private float bobFrequency = 1.5f;
    [SerializeField] private float bobAmplitude = 0.1f;
    [SerializeField] private float bobSmoothing = 10f;

    private float pitch;
    private float distanceTraveled;

    private void Awake()
    {
       Cursor.lockState = CursorLockMode.Locked;
       Cursor.visible = false;
    }

    private void LateUpdate()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        HandleRotation(mouseDelta);
        CalculateHeadBob();
    }

    private void HandleRotation(Vector2 lookInput)
    {
        if (Cursor.lockState != CursorLockMode.Locked)
            return;

        // Yaw (Horizontal rotation) is applied to the root physics object
        player.transform.Rotate(Vector3.up * (lookInput.x * mouseSensitivity));

        // Pitch (Vertical rotation) is applied only to the camera, isolated from physics
        pitch -= lookInput.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void CalculateHeadBob()
    {
        Vector3 flatVelocity = new Vector3(player.CurrentVelocity.x, 0f, player.CurrentVelocity.z);
        float speed = flatVelocity.magnitude;

        float targetBobOffset = 0f;

        if (speed > 0.1f)
        {
            distanceTraveled += speed * Time.deltaTime;
            targetBobOffset = Mathf.Sin(distanceTraveled * bobFrequency) * bobAmplitude;
        }
        else
        {
            // Reset phase to 0 when stopped to prevent sudden jolt
            distanceTraveled = 0f;
            targetBobOffset = 0f;
        }

        // Smooth camera position towards target bob offset
        float newY = Mathf.Lerp(transform.localPosition.y, targetBobOffset, Time.deltaTime * bobSmoothing);
        transform.localPosition = new Vector3(0, newY, 0);
    }
}