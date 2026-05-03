using UnityEngine;

namespace Code
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Настройки движения")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float runSpeed = 10f;
        [SerializeField] private float stepOffset = 0.3f;
    
        [Header("Настройки гравитации и прыжков")]
        [SerializeField] private float jumpHeight = 2f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundDistance = 0.4f;
        [SerializeField] private LayerMask groundMask;
    
        [Header("Настройки мыши")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private Transform playerBody;
        [SerializeField] private Transform cameraHolder;
        [SerializeField] private float maxLookUpAngle = 80f;
        [SerializeField] private float maxLookDownAngle = 80f;
        
        [Header("Настройки звуков шагов")]
        [SerializeField] private AudioSource footstepAudioSource;
        [SerializeField] private AudioClip[] footstepSounds;
        [SerializeField] private float walkStepInterval = 0.5f;
        [SerializeField] private float runStepInterval = 0.3f;
        [SerializeField] private float stepVolumeWalk = 0.5f;
        [SerializeField] private float stepVolumeRun = 0.8f;
    
        private float xRotation = 0f;
        private float currentSpeed;
        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;
        private float stepTimer;
        private bool isMoving;
        private bool isOnCeiling;
        private bool justJumped;
        
        // Флаг для проверки паузы
        private bool isInputEnabled = true;

        void Start()
        {
            controller = GetComponent<CharacterController>();
            controller.stepOffset = stepOffset;
            controller.slopeLimit = 45f;
            controller.skinWidth = 0.08f;
        
            isOnCeiling = gravity > 0;
        
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        
            if (playerBody == null)
                playerBody = transform;
            
            if (cameraHolder == null)
            {
                Camera cam = GetComponentInChildren<Camera>();
                if (cam != null)
                    cameraHolder = cam.transform;
                else
                    cameraHolder = transform;
            }
        
            if (groundCheck == null)
            {
                GameObject groundCheckObj = new GameObject("GroundCheck");
                groundCheckObj.transform.parent = transform;
                if (isOnCeiling)
                    groundCheckObj.transform.localPosition = new Vector3(0, 0.9f, 0);
                else
                    groundCheckObj.transform.localPosition = new Vector3(0, -0.9f, 0);
                groundCheck = groundCheckObj.transform;
            }
            
            if (footstepAudioSource == null)
            {
                footstepAudioSource = GetComponent<AudioSource>();
                if (footstepAudioSource == null)
                {
                    footstepAudioSource = gameObject.AddComponent<AudioSource>();
                }
                footstepAudioSource.spatialBlend = 1f;
                footstepAudioSource.playOnAwake = false;
            }
            
            // Подписываемся на событие паузы
            PauseMenu.PauseMenu.OnPauseStateChanged += SetInputState;
        }
        
        void OnDestroy()
        {
            // Отписываемся от события
            PauseMenu.PauseMenu.OnPauseStateChanged -= SetInputState;
        }
        
        private void SetInputState(bool isPaused)
        {
            isInputEnabled = !isPaused;
            
            if (isPaused)
            {
                // Сбрасываем движение при паузе
                stepTimer = 0f;
                isMoving = false;
                
                // Опционально: сброс ввода
                Input.ResetInputAxes();
            }
        }
    
        void Update()
        {
            // Если игра на паузе - не обрабатываем управление
            if (!isInputEnabled)
                return;
                
            HandleMouseLook();
            HandleMovement();
            HandleGravityAndJump();
            HandleFootsteps();
        
            // Убираем старую логику ESC, так как теперь это обрабатывается в PauseMenu
            // if (Input.GetKeyDown(KeyCode.Escape))
            // {
            //     Cursor.lockState = CursorLockMode.None;
            //     Cursor.visible = true;
            // }
        
            // Оставляем клик для возврата курсора только если игра НЕ на паузе
            if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None && !PauseMenu.PauseMenu.IsGamePaused)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    
        void HandleMouseLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
            playerBody.Rotate(Vector3.up * mouseX);
        
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -maxLookUpAngle, maxLookDownAngle);
            cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    
        void HandleMovement()
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
        
            Vector3 move = transform.right * x + transform.forward * z;
        
            if (Input.GetKey(KeyCode.LeftShift))
                currentSpeed = runSpeed;
            else
                currentSpeed = walkSpeed;
        
            controller.Move(move * currentSpeed * Time.deltaTime);
            
            isMoving = (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f) && isGrounded;
        }
    
        void HandleGravityAndJump()
        {
            bool wasGrounded = isGrounded;
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        
            if (isGrounded && !wasGrounded)
            {
                justJumped = false;
                velocity.y = 0f;
            }
        
            if (isGrounded && !justJumped)
            {
                if (isOnCeiling)
                    velocity.y = 0.5f;
                else
                    velocity.y = -0.5f;
            }
        
            if (Input.GetButtonDown("Jump") && isGrounded && !justJumped)
            {
                justJumped = true;
                
                if (isOnCeiling)
                {
                    velocity.y = -Mathf.Sqrt(jumpHeight * 2f * gravity);
                }
                else
                {
                    velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }
            }
        
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
        
        void HandleFootsteps()
        {
            if (!isMoving || footstepSounds.Length == 0)
            {
                stepTimer = 0f;
                return;
            }
            
            float currentStepInterval = Input.GetKey(KeyCode.LeftShift) ? runStepInterval : walkStepInterval;
            float currentStepVolume = Input.GetKey(KeyCode.LeftShift) ? stepVolumeRun : stepVolumeWalk;
            
            stepTimer -= Time.deltaTime;
            
            if (stepTimer <= 0f)
            {
                PlayFootstepSound(currentStepVolume);
                stepTimer = currentStepInterval;
            }
        }
        
        void PlayFootstepSound(float volume)
        {
            if (footstepAudioSource != null && footstepSounds.Length > 0)
            {
                AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
                footstepAudioSource.PlayOneShot(clip, volume);
            }
        }
        
        // Публичные методы для настроек
        public void SetMouseSensitivity(float sensitivity)
        {
            mouseSensitivity = sensitivity;
        }
        
        public float GetMouseSensitivity()
        {
            return mouseSensitivity;
        }
    
        void OnDrawGizmosSelected()
        {
            if (groundCheck != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
            }
        }
    }
}