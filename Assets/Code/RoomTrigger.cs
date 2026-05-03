using UnityEngine;

namespace Code
{
    public class RoomTrigger : MonoBehaviour
    {
        [Header("ID комнаты (должен быть уникальным)")]
        [SerializeField] private string roomID = "Room1";
    
        [Header("Ссылка на трекер комнат")]
        [SerializeField] private UniqueRoomTracker roomTracker;
    
        [Header("Настройки триггера")]
        [SerializeField] private bool destroyAfterVisit = true; // Удалить триггер после посещения
    
        private bool playerInside = false;
    
        private void Start()
        {
            // Если трекер не назначен вручную, ищем его на сцене
            if (roomTracker == null)
            {
                roomTracker = FindObjectOfType<UniqueRoomTracker>();
            
                if (roomTracker == null)
                {
                    Debug.LogError($"RoomTracker не найден на сцене! Комната {roomID} не будет работать.");
                }
            }
        }
    
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !playerInside)
            {
                playerInside = true;
            
                if (roomTracker != null)
                {
                    roomTracker.VisitRoom(roomID);
                
                    if (destroyAfterVisit)
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }
    
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInside = false;
            }
        }
    
        // Визуализация триггера в редакторе
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawCube(transform.position, transform.localScale);
        
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }
    }
}