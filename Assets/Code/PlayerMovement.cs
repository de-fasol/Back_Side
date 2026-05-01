using UnityEngine;

public class PlayerMovementWithMouse : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    
    [Header("Настройки гравитации и прыжков")]
    [SerializeField] private float jumpHeight = 2f;      // Высота прыжка
    [SerializeField] private float gravity = -9.81f;     // Сила гравитации
    [SerializeField] private Transform groundCheck;      // Точка проверки земли
    [SerializeField] private float groundDistance = 0.4f; // Радиус проверки земли
    [SerializeField] private LayerMask groundMask;       // Слои, которые считаются землёй
    
    [Header("Настройки мыши")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private Transform playerBody;       // Тело игрока (поворот по горизонтали)
    [SerializeField] private Transform cameraHolder;     // Камера (поворот по вертикали)
    [SerializeField] private float maxLookUpAngle = 80f; // Максимальный угол взгляда вверх
    [SerializeField] private float maxLookDownAngle = 80f; // Максимальный угол взгляда вниз
    
    private float xRotation = 0f;
    private float currentSpeed;
    private CharacterController controller;
    private Vector3 velocity;          // Вертикальная скорость для гравитации
    private bool isGrounded;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // Блокируем курсор в центре экрана
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Если тело игрока не назначено - используем этот объект
        if (playerBody == null)
            playerBody = transform;
            
        // Если холдер камеры не назначен - ищем камеру в дочерних объектах
        if (cameraHolder == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
                cameraHolder = cam.transform;
            else
                cameraHolder = transform;
        }
        
        // Если точка проверки земли не назначена - создаём виртуальную
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.parent = transform;
            groundCheckObj.transform.localPosition = new Vector3(0, -0.9f, 0);
            groundCheck = groundCheckObj.transform;
        }
    }
    
    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleGravityAndJump();
        
        // Нажмите ESC чтобы разблокировать курсор
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        // Клик мышкой чтобы заблокировать обратно
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
        
        // Поворот тела по горизонтали (влево-вправо)
        playerBody.Rotate(Vector3.up * mouseX);
        
        // Поворот камеры по вертикали (вверх-вниз)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookUpAngle, maxLookDownAngle);
        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
    
    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        
        // Движение относительно поворота игрока
        Vector3 move = transform.right * x + transform.forward * z;
        
        // Бег по удержанию Shift
        if (Input.GetKey(KeyCode.LeftShift))
            currentSpeed = runSpeed;
        else
            currentSpeed = walkSpeed;
        
        controller.Move(move * currentSpeed * Time.deltaTime);
    }
    
    void HandleGravityAndJump()
    {
        // Проверка на земле ли игрок
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        
        // Сброс вертикальной скорости если на земле и она отрицательная
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Небольшое прижатие к земле
        }
        
        // Прыжок
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // Формула: v = sqrt(2 * g * h)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        
        // Применяем гравитацию
        velocity.y += gravity * Time.deltaTime;
        
        // Применяем вертикальное движение
        controller.Move(velocity * Time.deltaTime);
    }
    
    // Визуализация точки проверки земли в редакторе
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}