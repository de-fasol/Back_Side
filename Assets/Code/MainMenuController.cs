using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using System.Collections;

namespace Code
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("🎛 Кнопки")]
        [SerializeField] private Button btnPlay;
        [SerializeField] private Button btnExit;
        [SerializeField] private Button btnSettings;
        [SerializeField] private Button btnBack;

        [Header("📦 Панели")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("🔊 Настройки звука")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string volumeParameter = "MasterVolume";
        [SerializeField] private Slider volumeSlider;

        [Header("🌑 Затемнение экрана")]
        [SerializeField] private Image fadeImage;              // Изображение для затемнения
        [SerializeField] private float fadeDuration = 1f;      // Длительность затемнения
        [SerializeField] private Color fadeColor = Color.black; // Цвет затемнения
        [SerializeField] private bool fadeOnStart = true;      // Затемнение при старте меню
        [SerializeField] private bool fadeOnSceneLoad = true;  // Затемнение при загрузке игры

        [Header("👤 Информация о разработчике")]
        [TextArea(3, 5)]
        [SerializeField] private string developerInfo = "Разработано: Ваше Имя/Никнейм\nEmail: your@email.com\nGitHub: github.com/yourname\n© 2026 Все права защищены";
        [SerializeField] private TextMeshProUGUI infoText;

        [Header("⚙️ Настройки загрузки")]
        [SerializeField] private string gameSceneName = "GameScene";

        private void Start()
        {
            if (infoText != null)
                infoText.text = developerInfo;

            // Настройка затемнения
            if (fadeImage != null)
            {
                if (fadeOnStart)
                {
                    StartCoroutine(FadeIn());
                }
                else
                {
                    fadeImage.gameObject.SetActive(false);
                }
            }

            // Кнопки
            if (btnPlay)     btnPlay.onClick.AddListener(PlayGame);
            if (btnExit)     btnExit.onClick.AddListener(ExitGame);
            if (btnSettings) btnSettings.onClick.AddListener(OpenSettings);
            if (btnBack)     btnBack.onClick.AddListener(CloseSettings);

            // Слайдер громкости
            if (volumeSlider != null)
            {
                float savedVolume = PlayerPrefs.GetFloat(volumeParameter, 1f);
                volumeSlider.value = savedVolume;
                SetVolume(savedVolume);
                volumeSlider.onValueChanged.AddListener(SetVolume);
            }

            UpdatePanels(true);
        }

        private void SetVolume(float value)
        {
            float dB = Mathf.Lerp(-80f, 0f, value);
            if (audioMixer != null)
            {
                audioMixer.SetFloat(volumeParameter, dB);
                PlayerPrefs.SetFloat(volumeParameter, value);
                PlayerPrefs.Save();
            }
        }

        private void PlayGame()
        {
            if (string.IsNullOrEmpty(gameSceneName))
            {
                Debug.LogError("❌ Не указано имя сцены игры в Inspector!");
                return;
            }
            
            if (fadeOnSceneLoad && fadeImage != null)
            {
                StartCoroutine(FadeOutAndLoadScene());
            }
            else
            {
                SceneManager.LoadScene(gameSceneName);
            }
        }

        private void ExitGame()
        {
            if (fadeOnSceneLoad && fadeImage != null)
            {
                StartCoroutine(FadeOutAndExit());
            }
            else
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                Debug.Log("🚪 Выход из игры...");
            }
        }

        private void OpenSettings()
        {
            UpdatePanels(false);
            if (settingsPanel) settingsPanel.SetActive(true);
            
            // Небольшая анимация открытия настроек (опционально)
            if (settingsPanel != null)
            {
                settingsPanel.transform.localScale = Vector3.zero;
                StartCoroutine(AnimatePanelOpen(settingsPanel));
            }
        }

        private void CloseSettings()
        {
            if (settingsPanel) settingsPanel.SetActive(false);
            UpdatePanels(true);
        }

        private void UpdatePanels(bool showMain)
        {
            if (mainMenuPanel) mainMenuPanel.SetActive(showMain);
        }

        // Анимация появления панели
        private IEnumerator AnimatePanelOpen(GameObject panel)
        {
            float elapsed = 0f;
            Vector3 startScale = Vector3.zero;
            Vector3 targetScale = Vector3.one;
            
            while (elapsed < fadeDuration / 2f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (fadeDuration / 2f);
                panel.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }
            
            panel.transform.localScale = targetScale;
        }

        // Затемнение при старте (появление меню)
        private IEnumerator FadeIn()
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
            
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
                yield return null;
            }
            
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            fadeImage.gameObject.SetActive(false);
        }

        // Затемнение при загрузке сцены
        private IEnumerator FadeOutAndLoadScene()
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
                yield return null;
            }
            
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
            
            // Небольшая задержка для плавности
            yield return new WaitForSeconds(0.1f);
            
            SceneManager.LoadScene(gameSceneName);
        }

        // Затемнение при выходе из игры
        private IEnumerator FadeOutAndExit()
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
                yield return null;
            }
            
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
            
            // Небольшая задержка перед выходом
            yield return new WaitForSeconds(0.2f);
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            Debug.Log("🚪 Выход из игры...");
        }

        // Публичный метод для ручного затемнения (можно вызвать из других скриптов)
        public void TriggerFadeIn()
        {
            if (fadeImage != null)
                StartCoroutine(FadeIn());
        }

        public void TriggerFadeOut(System.Action onComplete = null)
        {
            if (fadeImage != null)
                StartCoroutine(FadeOutCoroutine(onComplete));
        }

        private IEnumerator FadeOutCoroutine(System.Action onComplete)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
                yield return null;
            }
            
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
            onComplete?.Invoke();
        }
    }
}