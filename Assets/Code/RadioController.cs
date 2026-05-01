using System.Collections.Generic;
using UnityEngine;

namespace Code
{
    public class RadioController : MonoBehaviour
    {
        [System.Serializable]
        public class SoundItem
        {
            public string name;
            public AudioClip clip;
            [Range(1, 100)]
            public int rarity = 50;
        }

        [Header("Настройки радио")]
        public List<SoundItem> soundList = new List<SoundItem>();
        public AudioSource audioSource;
        public float minDelay = 5f;
        public float maxDelay = 15f;
        
        [Header("Настройки включения/выключения")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private bool isOn = false;
        [SerializeField] private GameObject highlightObject;
        [SerializeField] private float interactionRange = 3f;
        
        [Header("Защита от спама")]
        [SerializeField] private float interactionCooldown = 0.5f;
        
        [Header("Звуки")]
        [SerializeField] private AudioClip turnOnSound;
        [SerializeField] private AudioClip turnOffSound;
        [SerializeField] private float soundVolume = 0.5f;
        
        [Header("Визуальные эффекты")]
        [SerializeField] private Material onMaterial;
        [SerializeField] private Material offMaterial;
        [SerializeField] private GameObject onLight;
        
        private float timer = 0f;
        private bool isWaitingForClip = false;
        private bool isPlayerInRange = false;
        private bool isCooldown = false;
        private float cooldownTimer = 0f;
        private bool isTransitioning = false;
        private Camera playerCamera;
        private MeshRenderer[] meshRenderers;
        private Material[] originalMaterials;
        private AudioSource interactionAudioSource;
        
        public System.Action OnRadioTurnedOn;
        public System.Action OnRadioTurnedOff;
        
        void Start()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            
            interactionAudioSource = gameObject.AddComponent<AudioSource>();
            interactionAudioSource.volume = soundVolume;
            
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerCamera = player.GetComponentInChildren<Camera>();
            
            meshRenderers = GetComponentsInChildren<MeshRenderer>();
            if (meshRenderers != null && meshRenderers.Length > 0)
            {
                originalMaterials = new Material[meshRenderers.Length];
                for (int i = 0; i < meshRenderers.Length; i++)
                {
                    originalMaterials[i] = meshRenderers[i].material;
                }
            }
            
            if (highlightObject != null)
                highlightObject.SetActive(false);
            
            UpdateVisualState();
            
            if (isOn && soundList.Count > 0)
            {
                PlayRandomSound();
            }
        }
        
        void Update()
        {
            // Обновляем таймер защиты от спама
            if (isCooldown)
            {
                cooldownTimer -= Time.deltaTime;
                if (cooldownTimer <= 0f)
                {
                    isCooldown = false;
                }
            }
            
            // Проверка взаимодействия с игроком
            if (isPlayerInRange && Input.GetKeyDown(interactKey) && !isCooldown && !isTransitioning)
            {
                ToggleRadio();
                isCooldown = true;
                cooldownTimer = interactionCooldown;
            }
            
            // Логика воспроизведения музыки
            if (isOn)
            {
                if (isWaitingForClip)
                {
                    if (!audioSource.isPlaying)
                    {
                        isWaitingForClip = false;
                        timer = Random.Range(minDelay, maxDelay);
                        Debug.Log($"Трек закончился. Пауза {timer:F1} сек.");
                    }
                }
                else
                {
                    if (timer > 0)
                    {
                        timer -= Time.deltaTime;
                        if (timer <= 0)
                        {
                            PlayRandomSound();
                        }
                    }
                }
            }
            else
            {
                if (audioSource.isPlaying)
                    audioSource.Stop();
                isWaitingForClip = false;
                timer = 0f;
            }
        }
        
        public void ToggleRadio()
        {
            if (isTransitioning) return;
            
            if (isOn)
                TurnOff();
            else
                TurnOn();
        }
        
        public void TurnOn()
        {
            if (isOn || isTransitioning) return;
            
            isTransitioning = true;
            isOn = true;
            PlayInteractionSound(turnOnSound);
            UpdateVisualState();
            PlayRandomSound();
            
            OnRadioTurnedOn?.Invoke();
            Debug.Log("Радио включено");
            
            Invoke(nameof(EndTransition), 0.3f);
        }
        
        public void TurnOff()
        {
            if (!isOn || isTransitioning) return;
            
            isTransitioning = true;
            isOn = false;
            PlayInteractionSound(turnOffSound);
            UpdateVisualState();
            
            if (audioSource.isPlaying)
                audioSource.Stop();
            
            isWaitingForClip = false;
            timer = 0f;
            
            OnRadioTurnedOff?.Invoke();
            Debug.Log("Радио выключено");
            
            Invoke(nameof(EndTransition), 0.3f);
        }
        
        void EndTransition()
        {
            isTransitioning = false;
        }
        
        public void PlayRandomSound()
        {
            if (!isOn) return;
            
            if (audioSource.isPlaying)
            {
                Debug.Log("Сейчас играет трек, подождите...");
                return;
            }
            
            if (soundList.Count == 0)
            {
                Debug.LogWarning("Нет треков в списке!");
                return;
            }
            
            AudioClip selectedClip = GetRandomSoundByRarity();
            
            if (selectedClip != null && audioSource != null)
            {
                audioSource.clip = selectedClip;
                audioSource.Play();
                isWaitingForClip = true;
                timer = 0f;
                Debug.Log($"Радио играет: {selectedClip.name}");
            }
        }
        
        public void PlaySoundByIndex(int index)
        {
            if (!isOn)
            {
                Debug.Log("Радио выключено, включите сначала");
                return;
            }
            
            if (index < 0 || index >= soundList.Count)
            {
                Debug.LogError($"Индекс {index} вне диапазона! Всего треков: {soundList.Count}");
                return;
            }
            
            SoundItem item = soundList[index];
            if (item.clip == null)
            {
                Debug.LogError($"Трек {item.name} не имеет аудиоклипа!");
                return;
            }
            
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            audioSource.clip = item.clip;
            audioSource.Play();
            isWaitingForClip = true;
            timer = 0f;
            
            Debug.Log($"Запущен трек: {item.name}");
        }
        
        public void PlaySoundByName(string soundName)
        {
            if (!isOn)
            {
                Debug.Log("Радио выключено, включите сначала");
                return;
            }
            
            for (int i = 0; i < soundList.Count; i++)
            {
                if (soundList[i].name == soundName)
                {
                    PlaySoundByIndex(i);
                    return;
                }
            }
            
            Debug.LogError($"Трек '{soundName}' не найден!");
        }
        
        public void PlaySoundByClip(AudioClip clip)
        {
            if (!isOn)
            {
                Debug.Log("Радио выключено, включите сначала");
                return;
            }
            
            for (int i = 0; i < soundList.Count; i++)
            {
                if (soundList[i].clip == clip)
                {
                    PlaySoundByIndex(i);
                    return;
                }
            }
            
            Debug.LogError($"Клип {clip.name} не найден!");
        }
        
        private AudioClip GetRandomSoundByRarity()
        {
            if (soundList.Count == 0) return null;
            
            int totalRarity = 0;
            foreach (SoundItem item in soundList)
            {
                totalRarity += item.rarity;
            }
            
            int randomValue = Random.Range(0, totalRarity);
            int cumulative = 0;
            
            foreach (SoundItem item in soundList)
            {
                cumulative += item.rarity;
                if (randomValue < cumulative)
                {
                    return item.clip;
                }
            }
            
            return soundList[0].clip;
        }
        
        private void PlayInteractionSound(AudioClip clip)
        {
            if (interactionAudioSource != null && clip != null)
            {
                interactionAudioSource.PlayOneShot(clip, soundVolume);
            }
        }
        
        private void UpdateVisualState()
        {
            if (onMaterial != null && offMaterial != null && meshRenderers != null)
            {
                Material currentMaterial = isOn ? onMaterial : offMaterial;
                foreach (var renderer in meshRenderers)
                {
                    renderer.material = currentMaterial;
                }
            }
            
            if (onLight != null)
            {
                onLight.SetActive(isOn);
            }
        }
        
        public void SetHighlight(bool highlight)
        {
            if (highlightObject != null)
            {
                highlightObject.SetActive(highlight);
            }
        }
        
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInRange = true;
                SetHighlight(true);
                Debug.Log("Подойдите к радио и нажмите E чтобы включить/выключить");
            }
        }
        
        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInRange = false;
                SetHighlight(false);
            }
        }
        
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
        
        public bool IsOn() => isOn;
        public bool IsPlaying() => audioSource.isPlaying;
        public List<SoundItem> GetSoundList() => soundList;
        
        public SoundItem GetCurrentTrack()
        {
            if (audioSource.isPlaying && audioSource.clip != null)
            {
                foreach (var item in soundList)
                {
                    if (item.clip == audioSource.clip)
                        return item;
                }
            }
            return null;
        }
    }
}