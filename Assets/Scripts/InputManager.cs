using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    PlayerControls playerControls;
    AnimatorManager animatorManager;
    
    public Vector2 movementInput;
    public Vector2 cameraInput;
    public bool b_input;
    public bool jump_Input;

    public bool rollFlag;
    public bool jumpFlag;
    public bool sprintFlag;

    public float rollInputTimer;
    public float rollEndTimer;
    public float jumpEndTimer;
    public bool isInteracting;
    public bool isSprinting;
    public bool isJumping;

    public float cameraInputX;
    public float cameraInputY;

    public float moveAmount;
    public float verticalInput;
    public float horizontalInput;


    private void Awake()
    {
        animatorManager = GetComponent<AnimatorManager>();
    }

    private void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new PlayerControls();
            playerControls.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();
            playerControls.PlayerMovement.Camera.performed += i => cameraInput = i.ReadValue<Vector2>();
            playerControls.PlayerActions.Roll.started += i => b_input = true;
            playerControls.PlayerActions.Roll.canceled += i => b_input = false;
            playerControls.PlayerActions.Jump.performed += i => jump_Input = true;
            playerControls.PlayerActions.Jump.canceled += i => jump_Input = false;
        }
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    public void HandleAllInputs()
    {
        HandleIsInteracting();
        HandleMovementInput();
        HandleRollInput();
        HandleJumpingInput();
    }

    public void HandleIsInteracting()
    {
        isInteracting = animatorManager.animator.GetBool("isInteracting");
        isJumping = animatorManager.animator.GetBool("isJumping");
        if (Time.time > rollEndTimer)
            rollFlag = false;
        if (Time.time > jumpEndTimer)
            jumpFlag = false;
        sprintFlag = false;
    }
    
    private void HandleMovementInput()
    {
        verticalInput = movementInput.y;
        horizontalInput = movementInput.x;

        cameraInputY = cameraInput.y;
        cameraInputX = cameraInput.x;

        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput));
        animatorManager.UpdateAnimatorValues(0, moveAmount, isSprinting);

    }

    private void HandleRollInput()
    {
        if (b_input)
        {
            rollInputTimer += Time.deltaTime;
            sprintFlag = true;
        }
        else
        {
            
            if (rollInputTimer > 0 && rollInputTimer < 0.3)
            {
                sprintFlag = false;
                rollFlag = true;
                rollEndTimer = Time.time + 0.1f;
            }
            rollInputTimer = 0;
        }
        isSprinting = sprintFlag;
    }

    private void HandleJumpingInput()
    {
        if (jump_Input)
        {
            jumpFlag = true;
            jumpEndTimer = Time.time + 0.3f;
        }
    }
}
