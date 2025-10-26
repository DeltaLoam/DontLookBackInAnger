using UnityEngine;

namespace EasyPeasyFirstPersonController
{
    // THIS SCRIPT IS YOUR ORIGINAL CONTROLLER.
    // IT HAS NO FUSION CODE. WE HAVE ONLY REPLACED 'INPUT' CALLS WITH PUBLIC PROPERTIES.
    public class FirstPersonController_Motor : MonoBehaviour
    {
        #region Original Public Variables
        [Range(0, 100)] public float mouseSensitivity = 50f;
        [Range(0f, 200f)] private float snappiness = 100f;
        [Range(0f, 20f)] public float walkSpeed = 3f;
        [Range(0f, 30f)] public float sprintSpeed = 5f;
        [Range(0f, 10f)] public float crouchSpeed = 1.5f;
        public float crouchHeight = 1f;
        public float crouchCameraHeight = 1f;
        public float slideSpeed = 8f;
        public float slideDuration = 0.7f;
        public float slideFovBoost = 5f;
        public float slideTiltAngle = 5f;
        [Range(0f, 15f)] public float jumpSpeed = 3f;
        [Range(0f, 50f)] public float gravity = 9.81f;
        public bool coyoteTimeEnabled = true;
        [Range(0.01f, 0.3f)] public float coyoteTimeDuration = 0.2f;
        public float normalFov = 60f;
        public float sprintFov = 70f;
        public float fovChangeSpeed = 5f;
        public float walkingBobbingSpeed = 10f;
        public float bobbingAmount = 0.05f;
        private float sprintBobMultiplier = 1.5f;
        private float recoilReturnSpeed = 8f;
        public bool canSlide = true;
        public bool canJump = true;
        public bool canSprint = true;
        public bool canCrouch = true;
        public QueryTriggerInteraction ceilingCheckQueryTriggerInteraction = QueryTriggerInteraction.Ignore;
        public QueryTriggerInteraction groundCheckQueryTriggerInteraction = QueryTriggerInteraction.Ignore;
        public Transform groundCheck;
        public float groundDistance = 0.2f;
        public LayerMask groundMask;
        public Transform playerCamera;
        public Transform cameraParent;
        #endregion

        #region Public Inputs (Controlled by Network Script)
        // These properties will be set by the 'Puppeteer' script
        public float MouseXInput { get; set; }
        public float MouseYInput { get; set; }
        public Vector2 MoveInput { get; set; }
        public bool JumpInputDown { get; set; }
        public bool CrouchInputHeld { get; set; }
        public bool IsLocalPlayer { get; set; } // To enable/disable visual-only effects
        #endregion

        #region Original Private Variables
        private float rotX, rotY;
        private float xVelocity, yVelocity;
        private CharacterController characterController;
        private Vector3 moveDirection = Vector3.zero;
        private bool isGrounded;
        public bool isSprinting;
        public bool isCrouching;
        public bool isSliding;
        private float slideTimer;
        private float postSlideCrouchTimer;
        private Vector3 slideDirection;
        private float originalHeight;
        private float originalCameraParentHeight;
        private float coyoteTimer;
        private Camera cam;
        private AudioSource slideAudioSource;
        private float bobTimer;
        private float defaultPosY;
        private Vector3 recoil = Vector3.zero;
        private bool isLook = true, isMove = true;
        private float currentCameraHeight;
        private float currentBobOffset;
        private float currentFov;
        private float fovVelocity;
        private float currentSlideSpeed;
        private float slideSpeedVelocity;
        private float currentTiltAngle;
        private float tiltVelocity;
        private Animator anim;
        private bool jumpPressed = false;
        #endregion

        private void Awake()
        {
            // All original Awake logic is kept
            characterController = GetComponent<CharacterController>();
            cam = playerCamera.GetComponent<Camera>();
            anim = GetComponentInChildren<Animator>();
            originalHeight = characterController.height;
            originalCameraParentHeight = cameraParent.localPosition.y;
            defaultPosY = cameraParent.localPosition.y;
            slideAudioSource = gameObject.AddComponent<AudioSource>();
            slideAudioSource.playOnAwake = false;
            slideAudioSource.loop = false;
            currentCameraHeight = originalCameraParentHeight;
            currentBobOffset = 0f;
            currentFov = normalFov;
            currentSlideSpeed = 0f;
            currentTiltAngle = 0f;
            rotX = transform.rotation.eulerAngles.y;
            rotY = playerCamera.localRotation.eulerAngles.x;
            xVelocity = rotX;
            yVelocity = rotY;
        }

        // The Network script will call this method instead of Unity's Update.
        public void MotorUpdate(float deltaTime)
        {
            // Use the public properties instead of Input.Get...
            if (canJump && JumpInputDown && !isSliding)
            {
                jumpPressed = true;
            }

            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask, groundCheckQueryTriggerInteraction);
            if (isGrounded && moveDirection.y < 0)
            {
                moveDirection.y = -2f;
                coyoteTimer = coyoteTimeEnabled ? coyoteTimeDuration : 0f;
            }
            else if (coyoteTimeEnabled)
            {
                coyoteTimer -= deltaTime;
            }

            if (isLook)
            {
                // Use the public input properties
                rotX += MouseXInput;
                rotY -= MouseYInput;
                rotY = Mathf.Clamp(rotY, -90f, 90f);

                // Visual-only logic should only run for the local player
                if (IsLocalPlayer)
                {
                    xVelocity = Mathf.Lerp(xVelocity, rotX, snappiness * deltaTime);
                    yVelocity = Mathf.Lerp(yVelocity, rotY, snappiness * deltaTime);
                    float targetTiltAngle = isSliding ? slideTiltAngle : 0f;
                    currentTiltAngle = Mathf.SmoothDamp(currentTiltAngle, targetTiltAngle, ref tiltVelocity, 0.2f);
                    playerCamera.transform.localRotation = Quaternion.Euler(yVelocity - currentTiltAngle, 0f, 0f);
                }
                transform.rotation = Quaternion.Euler(0f, xVelocity, 0f);
            }
            
            // Visual-only logic
            if(IsLocalPlayer) HandleHeadBob();

            // Use the public input property
            bool wantsToCrouch = canCrouch && CrouchInputHeld && !isSliding;
            Vector3 point1 = transform.position + characterController.center - Vector3.up * (characterController.height * 0.5f);
            Vector3 point2 = point1 + Vector3.up * characterController.height * 0.6f;
            float capsuleRadius = characterController.radius * 0.95f;
            float castDistance = isSliding ? originalHeight + 0.2f : originalHeight - crouchHeight + 0.2f;
            bool hasCeiling = Physics.CapsuleCast(point1, point2, capsuleRadius, Vector3.up, castDistance, groundMask, ceilingCheckQueryTriggerInteraction);
            if (isSliding)
            {
                postSlideCrouchTimer = 0.3f;
            }
            if (postSlideCrouchTimer > 0)
            {
                postSlideCrouchTimer -= deltaTime;
                isCrouching = canCrouch;
            }
            else
            {
                isCrouching = canCrouch && (wantsToCrouch || (hasCeiling && !isSliding));
            }
            
            // Use the public input property
            if (canSlide && isSprinting && CrouchInputHeld && isGrounded)
            {
                isSliding = true;
                slideTimer = slideDuration;
                slideDirection = MoveInput.magnitude > 0.1f ? (transform.right * MoveInput.x + transform.forward * MoveInput.y).normalized : transform.forward;
                currentSlideSpeed = sprintSpeed;
            }

            float slideProgress = slideTimer / slideDuration;
            if (isSliding)
            {
                slideTimer -= deltaTime;
                if (slideTimer <= 0f || !isGrounded)
                {
                    isSliding = false;
                }
                float targetSlideSpeed = slideSpeed * Mathf.Lerp(0.7f, 1f, slideProgress);
                currentSlideSpeed = Mathf.SmoothDamp(currentSlideSpeed, targetSlideSpeed, ref slideSpeedVelocity, 0.2f);
                characterController.Move(slideDirection * currentSlideSpeed * deltaTime);
            }

            float targetHeight = isCrouching || isSliding ? crouchHeight : originalHeight;
            characterController.height = Mathf.Lerp(characterController.height, targetHeight, deltaTime * 10f);
            characterController.center = new Vector3(0f, characterController.height * 0.5f, 0f);

            // Visual-only logic
            if (IsLocalPlayer)
            {
                float targetFov = isSprinting ? sprintFov : (isSliding ? sprintFov + (slideFovBoost * Mathf.Lerp(0f, 1f, 1f - slideProgress)) : normalFov);
                currentFov = Mathf.SmoothDamp(currentFov, targetFov, ref fovVelocity, 1f / fovChangeSpeed);
                cam.fieldOfView = currentFov;
            }

            HandleMovement(deltaTime);
            HandleAnimations();
        }

        // --- All original private methods are kept exactly as they were ---
        private void HandleHeadBob() { /* Your original code here */ }
        private void HandleMovement(float deltaTime)
        {
            // In a perfectly separated model, this would also be controlled.
            // But to keep your methods identical, we use the public property here.
            isSprinting = canSprint && CrouchInputHeld == false && MoveInput.y > 0.1f && isGrounded && !isCrouching && !isSliding;

            float currentSpeed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);
            if (!isMove) currentSpeed = 0f;

            Vector3 direction = new Vector3(MoveInput.x, 0f, MoveInput.y);
            Vector3 moveVector = transform.TransformDirection(direction) * currentSpeed;
            moveVector = Vector3.ClampMagnitude(moveVector, currentSpeed);

            if (isGrounded || coyoteTimer > 0f)
            {
                // Use public property
                if (canJump && JumpInputDown && !isSliding)
                {
                    moveDirection.y = jumpSpeed;
                }
                else if (moveDirection.y < 0)
                {
                    moveDirection.y = -2f;
                }
            }
            else
            {
                moveDirection.y -= gravity * deltaTime;
            }

            if (!isSliding)
            {
                moveDirection = new Vector3(moveVector.x, moveDirection.y, moveVector.z);
                characterController.Move(moveDirection * deltaTime);
            }
        }
        private void HandleAnimations() { /* Your original code here */ }

        #region Original Public Methods
        public void SetControl(bool newState)
        {
            SetLookControl(newState);
            SetMoveControl(newState);
        }
        public void SetLookControl(bool newState)
        {
            isLook = newState;
        }
        public void SetMoveControl(bool newState)
        {
            isMove = newState;
        }
        public void SetCursorVisibility(bool newVisibility)
        {
            Cursor.lockState = newVisibility ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = newVisibility;
        }
        #endregion
    }
}