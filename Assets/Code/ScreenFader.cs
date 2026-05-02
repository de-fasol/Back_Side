using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Code
{
    public class ScreenFader : MonoBehaviour
    {
        [Header("Настройки затемнения")]
        [SerializeField] private float defaultFadeDuration = 1f;
        [SerializeField] private Color fadeColor = Color.black;
    
        private Canvas canvas;
        private Image fadeImage;
        private bool isFading = false;
    
        private static ScreenFader _instance;
    
        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                SetupFader();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    
        void SetupFader()
        {
            // Создаём корневой объект
            gameObject.name = "ScreenFader";
        
            // Создаём Canvas
            canvas = gameObject.GetComponent<Canvas>();
            if (canvas == null)
                canvas = gameObject.AddComponent<Canvas>();
        
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999; // Максимальный порядок, чтобы быть поверх всего
        
            // Добавляем CanvasScaler для автоматического масштабирования под экран
            CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
            if (scaler == null)
                scaler = gameObject.AddComponent<CanvasScaler>();
        
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        
            // Добавляем GraphicRaycaster для корректной работы (опционально)
            if (gameObject.GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();
        
            // Создаём панель затемнения
            GameObject panelObj = new GameObject("FadePanel");
            panelObj.transform.SetParent(transform, false);
        
            // Настраиваем RectTransform на весь экран
            RectTransform rectTransform = panelObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        
            // Добавляем Image для цвета
            fadeImage = panelObj.AddComponent<Image>();
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
            fadeImage.raycastTarget = false;
        
            // Отключаем объект, чтобы не мешал (будет включаться только при затемнении)
            canvas.enabled = false;
        }
    
        public static void FadeToScene(int sceneIndex, float duration = -1)
        {
            if (_instance != null && !_instance.isFading)
            {
                _instance.StartCoroutine(_instance.FadeSequence(sceneIndex, "", duration));
            }
        }
    
        public static void FadeToScene(string sceneName, float duration = -1)
        {
            if (_instance != null && !_instance.isFading)
            {
                _instance.StartCoroutine(_instance.FadeSequence(-1, sceneName, duration));
            }
        }
    
        IEnumerator FadeSequence(int sceneIndex, string sceneName, float customDuration)
        {
            isFading = true;
            float duration = customDuration > 0 ? customDuration : defaultFadeDuration;
        
            // Включаем Canvas
            canvas.enabled = true;
        
            // Затемнение
            yield return StartCoroutine(Fade(0, 1, duration));
        
            // Загружаем сцену
            AsyncOperation asyncLoad;
            if (!string.IsNullOrEmpty(sceneName))
            {
                asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            }
            else if (sceneIndex >= 0)
            {
                asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
            }
            else
            {
                asyncLoad = null;
            }
        
            if (asyncLoad != null)
            {
                while (!asyncLoad.isDone)
                {
                    yield return null;
                }
            }
        
            // Осветление
            yield return StartCoroutine(Fade(1, 0, duration));
        
            // Выключаем Canvas
            canvas.enabled = false;
        
            isFading = false;
        }
    
        IEnumerator Fade(float startAlpha, float endAlpha, float duration)
        {
            float elapsed = 0;
        
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            
                if (fadeImage != null)
                {
                    Color color = fadeImage.color;
                    color.a = alpha;
                    fadeImage.color = color;
                }
            
                yield return null;
            }
        
            if (fadeImage != null)
            {
                Color color = fadeImage.color;
                color.a = endAlpha;
                fadeImage.color = color;
            }
        }
    }
}