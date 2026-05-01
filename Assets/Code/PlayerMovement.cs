using UnityEngine;

public class PlayerMovementWithMouse : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    
    [Header("Настройки мыши")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private Transform playerBody; // Тело игрока (тот кто поворачивается по горизонтали)
    [SerializeField] private Transform cameraHolder; // Камера или её родитель (поворот по вертикали)
    [SerializeField] private float maxLookUpAngle = 80f; // Максимальный угол взгляда вверх
    [SerializeField] private float maxLookDownAngle = 80f; // Максимальный угол взгляда вниз
    
    private float xRotation = 0f;
    private float currentSpeed;
    private CharacterController controller;
    
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
                cameraHolder = transform; // fallback
        }
    }
    
    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        
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
}