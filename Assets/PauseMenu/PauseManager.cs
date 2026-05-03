using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace PauseMenu
{
    public class PauseMenu : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject pauseMenuUI;
        [SerializeField] private GameObject settingsMenuUI;
        [SerializeField] private GameObject exitConfirmPanel;

        [Header("Settings")]
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
        [SerializeField] private string menuName;

        [Header("Audio Settings")]
        [SerializeField] private bool muteAudioOnPause = true;
        [SerializeField] private AudioListener cameraAudioListener; // Ссылка на AudioListener камеры

        [Header("Events")]
        public UnityEvent OnPause;
        public UnityEvent OnResume;
        public UnityEvent OnMainMenuLoad;
        public UnityEvent OnQuitGame;

        // Статический флаг для проверки состояния паузы из других скриптов
        private static bool globalPauseState = false;
        public static bool IsGamePaused => globalPauseState;
        
        // Событие для оповещения других скриптов об изменении состояния паузы
        public static System.Action<bool> OnPauseStateChanged;

        private bool isPaused = false;

        void Start()
        {
            if (pauseMenuUI != null)
                pauseMenuUI.SetActive(false);
        
            if (settingsMenuUI != null)
                settingsMenuUI.SetActive(false);
        
            if (exitConfirmPanel != null)
                exitConfirmPanel.SetActive(false);
            
            // Сброс глобального состояния паузы при старте
            globalPauseState = false;
            isPaused = false;
            Time.timeScale = 1f;
            
            // Автоматически находим AudioListener на камере, если не назначен
            if (cameraAudioListener == null)
            {
                cameraAudioListener = GetComponentInChildren<AudioListener>();
                
                // Если всё ещё не найден, ищем на основной камере
                if (cameraAudioListener == null)
                {
                    Camera mainCamera = Camera.main;
                    if (mainCamera != null)
                        cameraAudioListener = mainCamera.GetComponent<AudioListener>();
                }
                
                // Если нашли - выводим сообщение
                if (cameraAudioListener != null)
                    Debug.Log("AudioListener автоматически найден на: " + cameraAudioListener.gameObject.name);
                else
                    Debug.LogWarning("AudioListener не найден! Добавьте компонент AudioListener на камеру или назначьте вручную.");
            }
            
            // Убеждаемся, что AudioListener включен при старте
            if (cameraAudioListener != null)
                cameraAudioListener.enabled = true;
        }

        void Update()
        {
            if (Input.GetKeyDown(pauseKey))
            {
                if (isPaused)
                    Resume();
                else
                    Pause();
            }
        }

        public void Resume()
        {
            if (pauseMenuUI != null)
                pauseMenuUI.SetActive(false);
        
            if (settingsMenuUI != null)
                settingsMenuUI.SetActive(false);
        
            if (exitConfirmPanel != null)
                exitConfirmPanel.SetActive(false);
        
            Time.timeScale = 1f;
            isPaused = false;
            globalPauseState = false;
            
            // Для вашего скрипта - возвращаем управление мышью
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // Включаем AudioListener обратно
            if (muteAudioOnPause && cameraAudioListener != null)
            {
                cameraAudioListener.enabled = true;
                Debug.Log("AudioListener включен - звуки возобновлены");
            }
        
            OnResume?.Invoke();
            OnPauseStateChanged?.Invoke(false);
        }

        public void Pause()
        {
            if (pauseMenuUI != null)
                pauseMenuUI.SetActive(true);
        
            Time.timeScale = 0f;
            isPaused = true;
            globalPauseState = true;
            
            // Для вашего скрипта - отключаем управление мышью
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Отключаем AudioListener - это выключит ВСЕ звуки в игре
            if (muteAudioOnPause && cameraAudioListener != null)
            {
                cameraAudioListener.enabled = false;
                Debug.Log("AudioListener выключен - все звуки заглушены");
            }
        
            OnPause?.Invoke();
            OnPauseStateChanged?.Invoke(true);
        }

        public void OpenSettings()
        {
            pauseMenuUI?.SetActive(false);
            settingsMenuUI?.SetActive(true);
        }

        public void CloseSettings()
        {
            settingsMenuUI?.SetActive(false);
            pauseMenuUI?.SetActive(true);
        }

        public void ShowExitConfirmation()
        {
            pauseMenuUI?.SetActive(false);
            exitConfirmPanel?.SetActive(true);
        }

        public void CancelExit()
        {
            exitConfirmPanel?.SetActive(false);
            pauseMenuUI?.SetActive(true);
        }

        public void ConfirmMainMenu()
        {
            // Включаем AudioListener перед загрузкой новой сцены
            if (cameraAudioListener != null)
                cameraAudioListener.enabled = true;
                
            OnMainMenuLoad?.Invoke();
            Time.timeScale = 1f;
            isPaused = false;
            globalPauseState = false;
            SceneManager.LoadScene("MainMenu");
        }

        public void ConfirmQuitGame()
        {
            OnQuitGame?.Invoke();
            Time.timeScale = 1f;
        
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void QuickLoadMainMenu()
        {
            // Включаем AudioListener перед загрузкой новой сцены
            if (cameraAudioListener != null)
                cameraAudioListener.enabled = true;
                
            OnMainMenuLoad?.Invoke();
            Time.timeScale = 1f;
            isPaused = false;
            globalPauseState = false;
            SceneManager.LoadScene(menuName);
        }
        
        // Метод для принудительного включения/выключения AudioListener
        public void SetAudioListenerState(bool enabled)
        {
            if (cameraAudioListener != null)
            {
                cameraAudioListener.enabled = enabled;
            }
        }
        
        // Метод для ручного назначения AudioListener
        public void AssignAudioListener(AudioListener listener)
        {
            cameraAudioListener = listener;
        }
        
        public static void ResetPauseState()
        {
            globalPauseState = false;
            Time.timeScale = 1f;
        }
        
        void OnDestroy()
        {
            OnPauseStateChanged = null;
        }
        
        // Для отладки - визуально показываем в инспекторе, есть ли AudioListener
        void OnValidate()
        {
            if (cameraAudioListener == null)
            {
                // Пытаемся найти в детейлах
                cameraAudioListener = GetComponentInChildren<AudioListener>();
            }
        }
    }
}