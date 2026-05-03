using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Code
{
    public class UniqueRoomTracker : MonoBehaviour
    {
        [Header("Настройки комнат")]
        [SerializeField] private int totalRooms = 5;
    
        [Header("Объекты для ВКЛЮЧЕНИЯ после посещения всех комнат")]
        [SerializeField] private GameObject[] objectsToActivate;
    
        [Header("Объекты для ВЫКЛЮЧЕНИЯ после посещения всех комнат")]
        [SerializeField] private GameObject[] objectsToDeactivate;
    
        [Header("Звуковые настройки")]
        [SerializeField] private AudioClip roomVisitSound; // Звук при посещении каждой комнаты
        [SerializeField] private AudioClip completionSound; // Звук при посещении всех комнат
        [Range(0f, 1f)]
        [SerializeField] private float soundVolume = 1f;
    
        [Header("События")]
        public UnityEvent onRoomVisited; // Вызывается при посещении любой комнаты
        public UnityEvent onAllRoomsVisited; // Вызывается когда все комнаты посещены
    
        // Приватные переменные
        private HashSet<string> visitedRooms = new HashSet<string>();
        private AudioSource audioSource;
        private bool allRoomsVisited = false;
    
        // Синглтон
        public static UniqueRoomTracker Instance { get; private set; }
    
        private void Awake()
        {
            // Настройка синглтона
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        
            // Настройка аудио
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = soundVolume;
        
            // Начальная деактивация объектов
            InitializeObjects();
        }
    
        private void Start()
        {
            // Показываем текущий прогресс
            Debug.Log($"Прогресс комнат: {visitedRooms.Count}/{totalRooms}");
        }
    
        private void InitializeObjects()
        {
            // Деактивируем объекты, которые должны появиться после всех комнат
            foreach (GameObject obj in objectsToActivate)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    
        /// <summary>
        /// Отметить комнату как посещенную
        /// </summary>
        public void VisitRoom(string roomID)
        {
            // Проверяем, не завершены ли уже все комнаты
            if (allRoomsVisited)
            {
                Debug.Log("Все комнаты уже посещены!");
                return;
            }
        
            // Проверяем, не была ли эта комната уже посещена
            if (visitedRooms.Contains(roomID))
            {
                Debug.Log($"Комната '{roomID}' уже была посещена ранее!");
                return;
            }
        
            // Отмечаем комнату как посещенную
            visitedRooms.Add(roomID);
        
            // Воспроизводим звук посещения комнаты
            PlaySound(roomVisitSound);
        
            // Вызываем событие посещения комнаты
            onRoomVisited?.Invoke();
        
            // Выводим прогресс в консоль
            Debug.Log($"✓ Комната '{roomID}' посещена! Прогресс: {visitedRooms.Count}/{totalRooms}");
        
            // Проверяем, все ли комнаты посещены
            if (visitedRooms.Count >= totalRooms)
            {
                CompleteAllRooms();
            }
        }
    
        /// <summary>
        /// Завершение после посещения всех комнат
        /// </summary>
        private void CompleteAllRooms()
        {
            allRoomsVisited = true;
        
            Debug.Log("🎉 ВСЕ КОМНАТЫ ПОСЕЩЕНЫ!");
        
            // Включаем объекты
            foreach (GameObject obj in objectsToActivate)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                    Debug.Log($"Активирован объект: {obj.name}");
                }
            }
        
            // Выключаем объекты
            foreach (GameObject obj in objectsToDeactivate)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                    Debug.Log($"Деактивирован объект: {obj.name}");
                }
            }
        
            // Воспроизводим звук завершения
            PlaySound(completionSound);
        
            // Вызываем событие завершения
            onAllRoomsVisited?.Invoke();
        }
    
        /// <summary>
        /// Воспроизвести звук
        /// </summary>
        private void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    
        /// <summary>
        /// Получить количество посещенных комнат
        /// </summary>
        public int GetVisitedRoomsCount()
        {
            return visitedRooms.Count;
        }
    
        /// <summary>
        /// Получить общее количество комнат
        /// </summary>
        public int GetTotalRoomsCount()
        {
            return totalRooms;
        }
    
        /// <summary>
        /// Проверить, все ли комнаты посещены
        /// </summary>
        public bool AreAllRoomsVisited()
        {
            return allRoomsVisited;
        }
    
        /// <summary>
        /// Проверить, была ли конкретная комната посещена
        /// </summary>
        public bool IsRoomVisited(string roomID)
        {
            return visitedRooms.Contains(roomID);
        }
    
        /// <summary>
        /// Сбросить прогресс посещения комнат
        /// </summary>
        public void ResetProgress()
        {
            visitedRooms.Clear();
            allRoomsVisited = false;
        
            // Возвращаем объекты в исходное состояние
            foreach (GameObject obj in objectsToActivate)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        
            foreach (GameObject obj in objectsToDeactivate)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        
            Debug.Log("Прогресс посещения комнат сброшен!");
        }
    
        /// <summary>
        /// Получить список посещенных комнат
        /// </summary>
        public List<string> GetVisitedRoomsList()
        {
            return new List<string>(visitedRooms);
        }
    }
}