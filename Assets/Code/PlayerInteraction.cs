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
        private PickupItem currentPickup;
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
            FindByRaycast();
        }
    
        void FindByRaycast()
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

            if (Physics.Raycast(ray, out var hit, interactionRange))
            {
                // ПРОВЕРКА НА ПРЕДМЕТ
                PickupItem pickup = hit.collider.GetComponent<PickupItem>();
                if (pickup != null && pickup.CanPickup())
                {
                    if (currentTarget != pickup.gameObject)
                    {
                        ClearAll();
                        currentTarget = pickup.gameObject;
                        currentPickup = pickup;
                        ShowPickupPopup();
                    }
                
                    if (Input.GetKeyDown(interactKey))
                    {
                        currentPickup.Pickup();
                        HidePopup();
                        currentTarget = null;
                        currentPickup = null;
                    }
                    return;
                }
            
                // ПРОВЕРКА НА ДВЕРЬ
                Door door = hit.collider.GetComponent<Door>();
                if (door != null)
                {
                    if (currentTarget != door.gameObject)
                    {
                        ClearAll();
                        currentTarget = door.gameObject;
                        currentDoor = door;
                        currentDoor.SetHighlight(true);
                        ShowDoorPopup();
                    }
                
                    if (Input.GetKeyDown(interactKey))
                    {
                        currentDoor.Interact();
                        ShowTemporaryPopup(currentDoor.IsLocked() ? "🔒 Дверь заперта!" : 
                            currentDoor.IsOpen() ? "🚪 Дверь открыта" : "🚪 Дверь закрыта", 1f);
                        ShowDoorPopup();
                    }
                    return;
                }
            
                // ПРОВЕРКА НА РАДИО
                RadioController radio = hit.collider.GetComponent<RadioController>();
                if (radio != null)
                {
                    if (currentTarget != radio.gameObject)
                    {
                        ClearAll();
                        currentTarget = radio.gameObject;
                        currentRadio = radio;
                        currentRadio.SetHighlight(true);
                        ShowRadioPopup();
                    }
                
                    if (Input.GetKeyDown(interactKey))
                    {
                        currentRadio.ToggleRadio();
                        ShowTemporaryPopup(currentRadio.IsOn() ? "🎵 Радио включено" : "🔇 Радио выключено", 1f);
                        ShowRadioPopup();
                    }
                    return;
                }
            }
        
            if (currentTarget != null)
            {
                ClearAll();
                currentTarget = null;
                currentDoor = null;
                currentRadio = null;
                currentPickup = null;
                HidePopup();
            }
        }
    
        void ShowDoorPopup()
        {
            if (interactionPopupPrefab == null) return;
        
            if (activePopup != null)
                Destroy(activePopup);
        
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
        }
    
        void ShowRadioPopup()
        {
            if (interactionPopupPrefab == null) return;
        
            if (activePopup != null)
                Destroy(activePopup);
        
            activePopup = Instantiate(interactionPopupPrefab, playerTransform.position + popupOffset, Quaternion.identity);
            var popupText = activePopup.GetComponentInChildren<UnityEngine.UI.Text>();
            if (popupText != null)
            {
                if (currentRadio.IsOn())
                    popupText.text = "📻 Нажмите E чтобы выключить";
                else
                    popupText.text = "📻 Нажмите E чтобы включить";
            }
        }
    
        void ShowPickupPopup()
        {
            if (interactionPopupPrefab == null) return;
        
            if (activePopup != null)
                Destroy(activePopup);
        
            activePopup = Instantiate(interactionPopupPrefab, playerTransform.position + popupOffset, Quaternion.identity);
            var popupText = activePopup.GetComponentInChildren<UnityEngine.UI.Text>();
            if (popupText != null)
            {
                popupText.text = "📦 Нажмите E чтобы подобрать";
            }
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
    
        void ClearAll()
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