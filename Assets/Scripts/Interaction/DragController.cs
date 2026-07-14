using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class DragController : MonoBehaviour
{
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private float maxPickupDistance = 10f;

    [Header("Hold Distance Settings")]
    [SerializeField] private float minHoldDistance = 1.5f;
    [SerializeField] private float maxHoldDistance = 10f;

    [Header("Physics Settings")]
    [SerializeField] private float grabRadius = 0.5f;
    [SerializeField] private float pullForce = 15f;
    [SerializeField] private float throwMultiplier = 1.5f;
    [SerializeField] private float heldDamping = 10f;

    private Camera mainCamera;
    private Rigidbody heldObject;
    private float currentHoldDistance;
    private float originalDamping;
    private float originalAngularDamping;
    private bool useGravityOriginal;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        HandlePhysics();
    }

    private void HandleInput()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            AttemptGrab();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame && heldObject != null)
        {
            ReleaseObject();
        }
    }

    private void AttemptGrab()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.SphereCast(ray, grabRadius, out RaycastHit hit, maxPickupDistance, interactableLayer, QueryTriggerInteraction.Ignore))
        {
            if (hit.rigidbody != null)
            {
                if (hit.rigidbody.isKinematic) return;
                // Record the original distance to the object, clamped within our min/max limits
                currentHoldDistance = Mathf.Clamp(hit.distance, minHoldDistance, maxHoldDistance);

                GrabObject(hit.rigidbody);
            }
        }
    }

    private void GrabObject(Rigidbody targetRb)
    {
        heldObject = targetRb;

        // Cache original physics state to restore upon release
        originalDamping = heldObject.linearDamping;
        originalAngularDamping = heldObject.angularDamping;
        useGravityOriginal = heldObject.useGravity;
        
        heldObject.useGravity = false;
        heldObject.linearDamping = heldDamping;
        heldObject.angularDamping = heldDamping;
    }

    private void HandlePhysics()
    {
        if (heldObject == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        Vector3 targetPosition = ray.origin + ray.direction * currentHoldDistance;

        Vector3 directionToTarget = targetPosition - heldObject.position;
        float distanceToTarget = directionToTarget.magnitude;

        Vector3 force = directionToTarget.normalized * (distanceToTarget * pullForce);

        heldObject.linearVelocity = force;
    }

    private void ReleaseObject()
    {
        // Restore original physics state
        heldObject.useGravity = useGravityOriginal;
        heldObject.linearDamping = originalDamping;
        heldObject.angularDamping = originalAngularDamping;

        // Throw
        heldObject.linearVelocity *= throwMultiplier;
        heldObject = null;
    }
}