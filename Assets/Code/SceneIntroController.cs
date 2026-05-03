using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Code
{
    public class SceneIntroController : MonoBehaviour
    {
        [Header("UI Затемнение")]
        [SerializeField] private Image fadePanel;
    
        [Header("Звук")]
        [SerializeField] private AudioClip introSound;
        [SerializeField] private float soundVolume = 0.7f;  // Громкость звука (0-1)
    
        [Header("Настройки")]
        [SerializeField] private float fadeInDuration = 2f;
    
        [Header("Объекты для активации")]
        [SerializeField] private GameObject[] objectsToActivate;
    
        private AudioSource audioSource;
    
        private void Start()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = soundVolume;  // Устанавливаем громкость
        
            // Изначально экран черный
            fadePanel.color = Color.black;
        
            // Деактивируем все объекты из массива
            foreach (GameObject obj in objectsToActivate)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        
            StartCoroutine(IntroSequence());
        }
    
        private IEnumerator IntroSequence()
        {
            // Запускаем звук
            audioSource.clip = introSound;
            audioSource.Play();
        
            // Ждем пока звук закончится
            yield return new WaitForSeconds(introSound.length);
        
            // Активируем все объекты из массива
            foreach (GameObject obj in objectsToActivate)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        
            // Плавно убираем черный экран
            float elapsedTime = 0f;
        
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeInDuration);
                fadePanel.color = new Color(0, 0, 0, alpha);
                yield return null;
            }
        
            // Полностью убираем панель
            fadePanel.gameObject.SetActive(false);
        }
        
        // Публичный метод для изменения громкости (можно вызвать из другого скрипта)
        public void SetVolume(float volume)
        {
            soundVolume = Mathf.Clamp01(volume);
            if (audioSource != null)
                audioSource.volume = soundVolume;
        }
        
        // Публичный метод для получения текущей громкости
        public float GetVolume()
        {
            return soundVolume;
        }
    }
}