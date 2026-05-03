using UnityEngine;

namespace Code
{
    public class TriggerArraySwitch : MonoBehaviour
    {
        [Header("Настройки триггера")]
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private bool oneTimeOnly = true;      // Сработает только один раз
        [SerializeField] private bool resetOnExit = false;      // Вернуть обратно при выходе
    
        [Header("Массивы для управления")]
        [SerializeField] private GameObject[] arrayToEnable;    // Включится при входе
        [SerializeField] private GameObject[] arrayToDisable;   // Выключится при входе
    
        [Header("Массивы для управления (при выходе)")]
        [SerializeField] private GameObject[] arrayToEnableOnExit;   // Включится при выходе
        [SerializeField] private GameObject[] arrayToDisableOnExit;  // Выключится при выходе
    
        [Header("Звуки")]
        [SerializeField] private AudioClip triggerSound;
        [SerializeField] private float soundVolume = 0.5f;
    
        [Header("Визуальные эффекты")]
        [SerializeField] private GameObject triggerEffect;
        [SerializeField] private bool showGizmo = true;
    
        private bool hasTriggered = false;
        private bool isPlayerInside = false;
        private AudioSource audioSource;
    
        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && triggerSound != null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(playerTag))
            {
                isPlayerInside = true;
            
                if (!oneTimeOnly || !hasTriggered)
                {
                    TriggerEnterAction();
                    hasTriggered = true;
                }
            }
        }
    
        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(playerTag))
            {
                isPlayerInside = false;
            
                if (resetOnExit)
                {
                    TriggerExitAction();
                }
            
                if (oneTimeOnly && hasTriggered)
                {
                    // Можно ничего не делать или добавить логику
                }
            }
        }
    
        void TriggerEnterAction()
        {
            Debug.Log($"Триггер {gameObject.name}: вход игрока");
        
            // Включаем массив
            if (arrayToEnable != null)
            {
                foreach (GameObject obj in arrayToEnable)
                {
                    if (obj != null)
                    {
                        obj.SetActive(true);
                        Debug.Log($"Включен: {obj.name}");
                    }
                }
            }
        
            // Выключаем массив
            if (arrayToDisable != null)
            {
                foreach (GameObject obj in arrayToDisable)
                {
                    if (obj != null)
                    {
                        obj.SetActive(false);
                        Debug.Log($"Выключен: {obj.name}");
                    }
                }
            }
        
            // Эффекты
            PlaySound();
            PlayEffect();
        }
    
        void TriggerExitAction()
        {
            Debug.Log($"Триггер {gameObject.name}: выход игрока");
        
            // Включаем массив при выходе
            if (arrayToEnableOnExit != null)
            {
                foreach (GameObject obj in arrayToEnableOnExit)
                {
                    if (obj != null)
                    {
                        obj.SetActive(true);
                        Debug.Log($"Включен (при выходе): {obj.name}");
                    }
                }
            }
        
            // Выключаем массив при выходе
            if (arrayToDisableOnExit != null)
            {
                foreach (GameObject obj in arrayToDisableOnExit)
                {
                    if (obj != null)
                    {
                        obj.SetActive(false);
                        Debug.Log($"Выключен (при выходе): {obj.name}");
                    }
                }
            }
        }
    
        void PlaySound()
        {
            if (triggerSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(triggerSound, soundVolume);
            }
        }
    
        void PlayEffect()
        {
            if (triggerEffect != null)
            {
                Instantiate(triggerEffect, transform.position, Quaternion.identity);
            }
        }
    
        // Публичный метод для ручного сброса
        public void ResetTrigger()
        {
            hasTriggered = false;
            Debug.Log($"Триггер {gameObject.name} сброшен");
        }
    
        // Публичный метод для ручного переключения
        public void ManualTrigger()
        {
            TriggerEnterAction();
        }
    
        void OnDrawGizmos()
        {
            if (!showGizmo) return;
        
            Gizmos.color = new Color(0, 1, 0, 0.3f);
        
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                if (col is BoxCollider box)
                {
                    Gizmos.DrawWireCube(transform.position + box.center, box.size);
                }
                else if (col is SphereCollider sphere)
                {
                    Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius);
                }
                else
                {
                    Gizmos.DrawWireCube(transform.position, Vector3.one);
                }
            }
        }
    }
}