using System.Collections;
using UnityEngine;

namespace Code
{
    public class RandomIntervalSoundPlayer : MonoBehaviour
    {
        [Header("Sound Clips")]
        [SerializeField] private AudioClip[] soundClips;
    
        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSource;
        [Range(0f, 1f)] [SerializeField] private float minVolume = 0.8f;
        [Range(0f, 1f)] [SerializeField] private float maxVolume = 1f;
        [SerializeField] private float minPitch = 0.9f;
        [SerializeField] private float maxPitch = 1.1f;
    
        [Header("Interval Settings (seconds)")]
        [SerializeField] private float minInterval = 3f;
        [SerializeField] private float maxInterval = 10f;
    
        [Header("Playback")]
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private bool loopIndefinitely = true;
    
        private bool isPlaying = false;
        private Coroutine soundRoutine;
    
        private void Start()
        {
            // Настраиваем AudioSource если его нет
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                    audioSource = gameObject.AddComponent<AudioSource>();
            }
        
            // Настройки AudioSource
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        
            if (playOnStart)
                StartSoundLoop();
        }
    
        public void StartSoundLoop()
        {
            if (soundRoutine != null)
                StopCoroutine(soundRoutine);
        
            soundRoutine = StartCoroutine(SoundLoopCoroutine());
        }
    
        public void StopSoundLoop()
        {
            if (soundRoutine != null)
            {
                StopCoroutine(soundRoutine);
                soundRoutine = null;
            }
        
            // Останавливаем текущий звук если он играет
            if (audioSource.isPlaying)
                audioSource.Stop();
        
            isPlaying = false;
        }
    
        private IEnumerator SoundLoopCoroutine()
        {
            while (loopIndefinitely)
            {
                // Ждем случайный интервал перед следующим звуком
                float waitTime = Random.Range(minInterval, maxInterval);
                yield return new WaitForSeconds(waitTime);
            
                // Проигрываем звук и ждем его окончания
                yield return StartCoroutine(PlaySoundAndWait());
            }
        }
    
        private IEnumerator PlaySoundAndWait()
        {
            if (soundClips == null || soundClips.Length == 0)
            {
                Debug.LogError("No sound clips assigned!");
                yield break;
            }
        
            // Выбираем случайный звук
            AudioClip randomClip = soundClips[Random.Range(0, soundClips.Length)];
        
            // Случайные параметры
            float randomVolume = Random.Range(minVolume, maxVolume);
            float randomPitch = Random.Range(minPitch, maxPitch);
        
            // Настраиваем и проигрываем
            audioSource.clip = randomClip;
            audioSource.volume = randomVolume;
            audioSource.pitch = randomPitch;
            audioSource.Play();
        
            // Ждем пока звук закончится
            float clipLength = randomClip.length;
            float elapsedTime = 0f;
        
            while (elapsedTime < clipLength && audioSource.isPlaying)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        
            // Небольшая задержка чтобы убедиться что звук полностью остановился
            yield return null;
        }
    
        // Проверка - играет ли сейчас звук?
        public bool IsSoundPlaying()
        {
            return audioSource.isPlaying;
        }
    
        // Получить оставшееся время текущего звука
        public float GetRemainingTime()
        {
            if (audioSource.isPlaying && audioSource.clip != null)
            {
                return audioSource.clip.length - audioSource.time;
            }
            return 0f;
        }
    
        // Пропустить текущий звук и начать ожидание следующего
        public void SkipCurrentSound()
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
        }
    }
}