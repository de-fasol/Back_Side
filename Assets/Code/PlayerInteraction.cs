using UnityEngine;

namespace Code
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Настройки")]
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
    
        [Header("Попап")]
        [SerializeField] private GameObject interactionPopupPrefab;
        [SerializeField] private Vector3 popupOffset = new Vector3(0, 2f, 0);
    
        private Camera playerCamera;
        private Transform playerTransform;
        private Door currentDoor;
        private RadioController currentRadio;
        private GameObject currentTarget;
        private GameObject activePopup;
    
        void Start()
        {
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null)
                playerCamera = Camera.main;
        
            playerTransform = transform;
        }
    
        void Update()
        {
            // Поиск объектов через луч из центра камеры
            FindByRaycast();
        }
    
        void FindByRaycast()
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;
        
            if (Physics.Raycast(ray, out hit, interactionRange))
            {
                // Проверка на дверь
                Door door = hit.collider.GetComponent<Door>();
                if (door != null)
                {
                    if (currentTarget != door.gameObject)
                    {
                        ClearHighlight();
                        currentTarget = door.gameObject;
                        currentDoor = door;
                        currentRadio = null;
                        currentDoor.SetHighlight(true);
                        ShowPopupForDoor();  // Показываем попап (без таймера)
                    }
                
                    if (Input.GetKeyDown(interactKey))
                    {
                        currentDoor.Interact();
                        ShowTemporaryPopup(currentDoor.IsLocked() ? "🔒 Дверь заперта!" : 
                            currentDoor.IsOpen() ? "🚪 Дверь открыта" : "🚪 Дверь закрыта", 1f);
                        // Обновляем попап после взаимодействия
                        ShowPopupForDoor();
                    }
                    return;
                }
            
                // Проверка на радио
                RadioController radio = hit.collider.GetComponent<RadioController>();
                if (radio != null)
                {
                    if (currentTarget != radio.gameObject)
                    {
                        ClearHighlight();
                        currentTarget = radio.gameObject;
                        currentRadio = radio;
                        currentDoor = null;
                        currentRadio.SetHighlight(true);
                        ShowPopupForRadio();  // Показываем попап (без таймера)
                    }
                
                    if (Input.GetKeyDown(interactKey))
                    {
                        currentRadio.ToggleRadio();
                        ShowTemporaryPopup(currentRadio.IsOn() ? "🎵 Радио включено" : "🔇 Радио выключено", 1f);
                        // Обновляем попап после взаимодействия
                        ShowPopupForRadio();
                    }
                    return;
                }
            }
        
            // Если не смотрим ни на что - убираем всё
            if (currentTarget != null)
            {
                ClearHighlight();
                currentTarget = null;
                currentDoor = null;
                currentRadio = null;
                HidePopup();
            }
        }
    
        void ShowPopupForDoor()
        {
            if (interactionPopupPrefab == null) return;
        
            // Удаляем старый попап
            if (activePopup != null)
                Destroy(activePopup);
        
            // Создаём новый
            activePopup = Instantiate(interactionPopupPrefab, playerTransform.position + popupOffset, Quaternion.identity);
            var popupText = activePopup.GetComponentInChildren<UnityEngine.UI.Text>();
            if (popupText != null)
            {
                if (currentDoor.IsLocked())
                    popupText.text = "🔒 Дверь заперта!";
                else if (currentDoor.IsOpen())
                    popupText.text = "🚪 Дверь открыта";
                else
                    popupText.text = "🚪 Нажмите E чтобы открыть";
            }
            // НЕТ ТАЙМЕРА! Попап будет висеть, пока смотрим на дверь
        }
    
        void ShowPopupForRadio()
        {
            if (interactionPopupPrefab == null) return;
        
            // Удаляем старый попап
            if (activePopup != null)
                Destroy(activePopup);
        
            // Создаём новый
            activePopup = Instantiate(interactionPopupPrefab, playerTransform.position + popupOffset, Quaternion.identity);
            var popupText = activePopup.GetComponentInChildren<UnityEngine.UI.Text>();
            if (popupText != null)
            {
                if (currentRadio.IsOn())
                    popupText.text = "📻 Нажмите E чтобы выключить";
                else
                    popupText.text = "📻 Нажмите E чтобы включить";
            }
            // НЕТ ТАЙМЕРА! Попап будет висеть, пока смотрим на радио
        }
    
        void ShowTemporaryPopup(string message, float duration)
        {
            if (interactionPopupPrefab == null) return;
        
            GameObject tempPopup = Instantiate(interactionPopupPrefab, playerTransform.position + popupOffset, Quaternion.identity);
            var popupText = tempPopup.GetComponentInChildren<UnityEngine.UI.Text>();
            if (popupText != null)
                popupText.text = message;
        
            Destroy(tempPopup, duration);
        }
    
        void HidePopup()
        {
            if (activePopup != null)
            {
                Destroy(activePopup);
                activePopup = null;
            }
        }
    
        void ClearHighlight()
        {
            if (currentDoor != null)
                currentDoor.SetHighlight(false);
            if (currentRadio != null)
                currentRadio.SetHighlight(false);
        }
    
        void OnDrawGizmos()
        {
            if (playerCamera != null)
            {
                Gizmos.color = Color.red;
                Vector3 rayEnd = playerCamera.transform.position + playerCamera.transform.forward * interactionRange;
                Gizmos.DrawLine(playerCamera.transform.position, rayEnd);
            }
        }
    }
}