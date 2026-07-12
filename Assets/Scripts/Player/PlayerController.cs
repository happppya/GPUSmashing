using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PlayerController : MonoBehaviour
{

    [SerializeField] private float maxWalkSpeed = 6f;
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private float deceleration = 25f;

    private Rigidbody rigidBody;
    private Vector2 moveInput;

    public Vector3 CurrentVelocity => rigidBody.linearVelocity;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.freezeRotation = true;
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        bool isMovingLeft = Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed;
        bool isMovingRight = Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed;
        // Fixed the inverted arrow keys here
        bool isMovingUp = Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed;
        bool isMovingDown = Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed;

        moveInput = new Vector2
            (
                (isMovingLeft == isMovingRight) ? 0f : (isMovingLeft ? -1f : 1f),
                (isMovingUp == isMovingDown) ? 0f : (isMovingUp ? 1f : -1f)
            );

        // Calculate target velocity relative to the player's direction
        Vector3 targetVelocity = (transform.forward * moveInput.y + transform.right * moveInput.x) * maxWalkSpeed;

        // Current velocity without Y axis movement
        Vector3 currentHorizontalVelocity = new Vector3(rigidBody.linearVelocity.x, 0f, rigidBody.linearVelocity.z);

        // Determine whether we are accelerating or decelerating
        float accelRate = moveInput.sqrMagnitude > 0.01f ? acceleration : deceleration;

        Vector3 velocityDifference = targetVelocity - currentHorizontalVelocity;
        Vector3 movementForce = velocityDifference * accelRate;
        rigidBody.AddForce(movementForce, ForceMode.Acceleration);
    }
}