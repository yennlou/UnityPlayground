using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    InputManager inputManager;
    AnimatorManager animatorManager;

    Vector3 moveDirection;
    Transform cameraObject;
    public Rigidbody playerRb;

    [Header("Falling")]
    public float inAirTimer;
    public float leapingVelocity;
    public float fallingVelocity;
    public float rayCastHeightOffset;
    public LayerMask groundLayer;

    [Header("Jump Speeds")]
    public float gravityIntensity = -15f;
    public float jumpHeight = 3f;

    [Header("Movement Flags")]
    public bool isGrounded;

    [Header("Movement Speeds")]
    public float movementSpeed = 7;
    public float sprintSpeed = 10;
    public float rotationSpeed = 15;

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
        animatorManager = GetComponent<AnimatorManager>();
        playerRb = GetComponent<Rigidbody>();
        cameraObject = Camera.main.transform;
    }

    public void HandleAllMovement()
    {
        HandleFallingAndLanding();
        if (inputManager.isInteracting) return;
        HandleMovement();
        HandleRotation();
        HandleRollingAndSprinting();
        HandleJump();
    }

    private void HandleMovement()
    {
        if (inputManager.isJumping) return;
        moveDirection = cameraObject.forward * inputManager.verticalInput;
        moveDirection = moveDirection + cameraObject.right * inputManager.horizontalInput;
        moveDirection.Normalize();
        moveDirection.y = 0;

        Vector3 movementVelocity = moveDirection * movementSpeed;
        playerRb.velocity = movementVelocity;
    }

    private void HandleRotation()
    {
        if (inputManager.isJumping) return;
        Vector3 targetDirection = Vector3.zero;

        targetDirection = cameraObject.forward * inputManager.verticalInput;
        targetDirection = targetDirection + cameraObject.right * inputManager.horizontalInput;
        targetDirection.Normalize();
        targetDirection.y = 0;

        if (targetDirection == Vector3.zero)
            targetDirection = transform.forward;

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        transform.rotation = playerRotation;
    }

    public void HandleRollingAndSprinting()
    {

        if (animatorManager.animator.GetBool("isInteracting"))
            return;

        if (inputManager.rollFlag)
        {
            moveDirection = cameraObject.forward * inputManager.verticalInput;
            moveDirection += cameraObject.right * inputManager.horizontalInput;
            
            if (inputManager.moveAmount > 0)
            {
                animatorManager.PlayTargetAnimation("Roll", true);
                moveDirection.y = 0;
                Quaternion rollRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = rollRotation;
            }
            else
            {
                animatorManager.PlayTargetAnimation("RunBack", true);
            }
        }
        if (inputManager.sprintFlag)
        {
            Vector3 movementVelocity = moveDirection * sprintSpeed;
            playerRb.velocity = movementVelocity;
        }
    }

    private void HandleFallingAndLanding()
    {
        RaycastHit hit;
        Vector3 rayCastOrigin = transform.position;
        rayCastOrigin.y = rayCastOrigin.y + rayCastHeightOffset;
        if (!isGrounded && !inputManager.isJumping)
        {
            if (!inputManager.isInteracting)
            {
                animatorManager.PlayTargetAnimation("Falling", true);
            }

            inAirTimer = inAirTimer + Time.deltaTime;
            playerRb.AddForce(transform.forward * leapingVelocity);
            playerRb.AddForce(-Vector3.up * fallingVelocity * inAirTimer);
        }
        if (Physics.SphereCast(rayCastOrigin, 0.2f, -Vector3.up, out hit, 0.4f, groundLayer))
        {
            if (!isGrounded && !inputManager.isJumping)
            {
                animatorManager.PlayTargetAnimation("LandSoft", true);
            }
            inAirTimer = 0;
            isGrounded = true;
        } else
        {
            isGrounded = false;
        }
        
    }

    private void HandleJump()
    {
        if (isGrounded && inputManager.jumpFlag && !inputManager.isInteracting && !inputManager.isJumping)
        {
            animatorManager.animator.SetBool("isJumping", true);
            animatorManager.PlayTargetAnimation("Jump", false);
            float jumpingVelocity = Mathf.Sqrt(-2 * gravityIntensity * jumpHeight);
            Vector3 playerVelocity = moveDirection * 6;
            playerVelocity.y = jumpingVelocity;
            playerRb.velocity = playerVelocity;
        }
    }
}
