using UnityEngine;
using UnityEngine.UI;

namespace Code
{
    public class ButtonNw : MonoBehaviour
    {
        [SerializeField] private GameObject[] turnOn;   // Что включить
        [SerializeField] private GameObject[] turnOff;  // Что выключить
        [SerializeField] private GameObject newspaperUI; // Canvas с газетой
        
        [Header("Звуки")]
        [SerializeField] private AudioClip openSound;    // Звук открытия газеты
        [SerializeField] private AudioClip closeSound;   // Звук закрытия газеты
        [SerializeField] private float soundVolume = 0.7f;
        
        private bool isOpen = false;
        private PlayerMovement playerMovement;
        private AudioSource audioSource;
        
        void Start()
        {
            GetComponent<Button>().onClick.AddListener(ToggleNewspaper);
            
            // Находим скрипт движения игрока
            playerMovement = FindObjectOfType<PlayerMovement>();
            
            // Настраиваем AudioSource
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && (openSound != null || closeSound != null))
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Скрываем газету при старте
            if (newspaperUI != null)
                newspaperUI.SetActive(false);
            
            // === ПРИ СТАРТЕ: БЛОКИРУЕМ КАМЕРУ ===
            if (playerMovement != null)
                playerMovement.enabled = false;
            
            // Блокируем курсор (камера не крутится)
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        void ToggleNewspaper()
        {
            isOpen = !isOpen;
            
            if (isOpen)
            {
                // === ОТКРЫТИЕ ГАЗЕТЫ ===
                
                // Воспроизводим звук открытия
                PlaySound(openSound);
                
                // Показываем газету
                if (newspaperUI != null)
                    newspaperUI.SetActive(true);
                
                // Движение игрока уже выключено (оставляем)
                if (playerMovement != null)
                    playerMovement.enabled = true;
                
                // Разблокируем курсор для чтения газеты
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                
                // Включаем/выключаем массивы
                foreach (var obj in turnOn) 
                    if (obj != null) obj.SetActive(true);
                foreach (var obj in turnOff) 
                    if (obj != null) obj.SetActive(false);
            }
            else
            {
                // === ЗАКРЫТИЕ ГАЗЕТЫ ===
                
                // Воспроизводим звук закрытия
                PlaySound(closeSound);
                
                // Прячем газету
                if (newspaperUI != null)
                    newspaperUI.SetActive(false);
                
                // Возвращаем курсор в заблокированное состояние
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                
                // Движение игрока остаётся выключенным (как при старте)
                if (playerMovement != null) 
                    playerMovement.enabled = true;
            }
        }
        
        void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip, soundVolume);
            }
        }
    }
}