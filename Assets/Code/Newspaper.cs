using UnityEngine;
using UnityEngine.UI;

namespace Code
{
    public class Newspaper : MonoBehaviour
    {
        [Header("Настройки")]
        [SerializeField] private Sprite newspaperSprite;
        [SerializeField] private string newspaperText = "";
        [SerializeField] private float interactionRange = 3f;
    
        [Header("UI")]
        [SerializeField] private GameObject uiCanvas;      // Canvas с газетой
        [SerializeField] private Image backgroundImage;    // Затемнение
        [SerializeField] private Image newspaperImage;     // Картинка газеты
        [SerializeField] private Text newspaperTextUI;     // Текст газеты
    
        private Camera playerCamera;
        private bool isOpen = false;
    
        void Start()
        {
            playerCamera = Camera.main;
        
            if (uiCanvas != null)
                uiCanvas.SetActive(false);
        
            if (newspaperImage != null && newspaperSprite != null)
                newspaperImage.sprite = newspaperSprite;
        
            if (newspaperTextUI != null)
                newspaperTextUI.text = newspaperText;
        }
    
        void Update()
        {
            // Проверка взгляда И нажатие E
            if (!isOpen && Input.GetKeyDown(KeyCode.E))
            {
                Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
                RaycastHit hit;
            
                if (Physics.Raycast(ray, out hit, interactionRange))
                {
                    if (hit.collider.gameObject == gameObject)
                    {
                        Open();
                    }
                }
            }
        
            // Закрытие по Escape
            if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }
    
        void Open()
        {
            isOpen = true;
        
            if (uiCanvas != null)
                uiCanvas.SetActive(true);
        
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        
            Time.timeScale = 0; // Останавливаем время
            Debug.Log("Газета открыта");
        }
    
        public void Close()
        {
            isOpen = false;
        
            if (uiCanvas != null)
                uiCanvas.SetActive(false);
        
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        
            Time.timeScale = 1; // Возвращаем время
            Debug.Log("Газета закрыта");
        }
    }
}