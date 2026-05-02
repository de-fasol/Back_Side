using UnityEngine;

namespace Code
{
    public class PlayerMovementWithMouse : MonoBehaviour
    {
        [Header("Настройки движения")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float runSpeed = 10f;
    
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
        [SerializeField] private AudioSource footstepAudioSource; // Источник звука
        [SerializeField] private AudioClip[] footstepSounds;      // Массив звуков шагов
        [SerializeField] private float walkStepInterval = 0.5f;   // Интервал между шагами при ходьбе
        [SerializeField] private float runStepInterval = 0.3f;    // Интервал между шагами при беге
        [SerializeField] private float stepVolumeWalk = 0.5f;     // Громкость шагов при ходьбе
        [SerializeField] private float stepVolumeRun = 0.8f;      // Громкость шагов при беге
    
        private float xRotation = 0f;
        private float currentSpeed;
        private CharacterController controller;
        private Vector3 velocity;
        private bool isGrounded;
        private float stepTimer;
        private bool isMoving;
    
        void Start()
        {
            controller = GetComponent<CharacterController>();
        
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
                groundCheckObj.transform.localPosition = new Vector3(0, -0.9f, 0);
                groundCheck = groundCheckObj.transform;
            }
            
            // Если AudioSource не назначен, добавляем компонент автоматически
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
        }
    
        void Update()
        {
            HandleMouseLook();
            HandleMovement();
            HandleGravityAndJump();
            HandleFootsteps();
        
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        
            if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
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
            
            // Определяем, движется ли игрок
            isMoving = (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f) && isGrounded;
        }
    
        void HandleGravityAndJump()
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }
        
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
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
            
            // Выбираем интервал в зависимости от режима движения
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
                // Выбираем случайный звук из массива
                AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
                footstepAudioSource.PlayOneShot(clip, volume);
            }
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