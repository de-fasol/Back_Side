using UnityEngine;

namespace PauseMenu
{
    public class PlayerCamera : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private float mouseSensitivity = 100f;
        [SerializeField] private Transform playerBody;
        [SerializeField] private bool invertY = false;
        
        [Header("Limits")]
        [SerializeField] private float minXLook = -90f;
        [SerializeField] private float maxXLook = 90f;
        
        private float xRotation = 0f;
        private bool isInputEnabled = true;
        
        void Start()
        {
            // Если не назначен body, ищем родительский объект
            if (playerBody == null)
                playerBody = transform.parent;
            
            // Если всё ещё null, создаём предупреждение
            if (playerBody == null)
                Debug.LogWarning("Player Body not assigned to PlayerCamera script!");
            
            // Блокируем курсор при старте
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isInputEnabled = true;
        }
        
        void OnEnable()
        {
            // Подписываемся на событие изменения состояния паузы
            PauseMenu.OnPauseStateChanged += SetInputState;
        }
        
        void OnDisable()
        {
            // Отписываемся от события
            PauseMenu.OnPauseStateChanged -= SetInputState;
        }
        
        private void SetInputState(bool isPaused)
        {
            isInputEnabled = !isPaused;
            
            // Сбрасываем накопленный ввод мыши при паузе
            if (isPaused)
            {
                ResetMouseInput();
            }
        }
        
        private void ResetMouseInput()
        {
            // Сброс осей мыши
            Input.ResetInputAxes();
            
            // Опционально: сброс накопленного вращения (раскомментируйте если нужно)
            // xRotation = 0f;
            // transform.localRotation = Quaternion.identity;
            // if (playerBody != null)
            //     playerBody.rotation = Quaternion.identity;
        }
        
        void Update()
        {
            // Если ввод отключен (игра на паузе), не обрабатываем движение камеры
            if (!isInputEnabled)
                return;
            
            // Получаем ввод мыши
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
            
            // Инвертирование оси Y
            if (invertY)
                mouseY = -mouseY;
            
            // Вращение по вертикали (камера)
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, minXLook, maxXLook);
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            
            // Вращение по горизонтали (игрок)
            if (playerBody != null)
                playerBody.Rotate(Vector3.up * mouseX);
        }
        
        // Публичный метод для изменения чувствительности
        public void SetSensitivity(float newSensitivity)
        {
            mouseSensitivity = newSensitivity;
        }
        
        // Публичный метод для изменения инверсии
        public void SetInvertY(bool invert)
        {
            invertY = invert;
        }
    }
}