using UnityEngine;

namespace Code
{
    public class PickupItem : MonoBehaviour
    {
        [Header("Настройки подбора")]
        [SerializeField] private string itemName = "Предмет";
    

        public enum PickupActionType
        {
            Nothing,
            ToggleArrays,
            FadeToScene,
            FadeAndToggle
        }
    
        [SerializeField] private PickupActionType actionType = PickupActionType.ToggleArrays;
    

        public enum ArrayMode
        {
            OneTimeEnable,     // ОДИН РАЗ: включить один массив, выключить другой
            ToggleMode         // ПЕРЕКЛЮЧАТЕЛЬ: один массив вкл/выкл при каждом подборе
        }
    
        [SerializeField] private ArrayMode arrayMode = ArrayMode.OneTimeEnable;
    
        [Header("Режим: OneTimeEnable (включить/выключить разные массивы)")]
        [SerializeField] private GameObject[] arrayToEnable;
        [SerializeField] private GameObject[] arrayToDisable;
    
        [Header("Режим: ToggleMode (переключать один массив)")]
        [SerializeField] private GameObject[] arrayToToggle;
        [SerializeField] private bool startState = false;
    

        public enum PickupDestroyType
        {
            Destroy,
            Disable,
            DoNothing,
            RespawnAfterDelay
        }
    
        [SerializeField] private PickupDestroyType destroyType = PickupDestroyType.Destroy;
        [SerializeField] private float respawnDelay = 3f;
    
        [Header("Переход на сцену")]
        [SerializeField] private int targetSceneIndex = 1;
        [SerializeField] private string targetSceneName = "";
        [SerializeField] private float fadeDuration = 1f;
    
        [Header("Звуки")]
        [SerializeField] private AudioClip pickupSound;           // Обычный звук подбора
        [SerializeField] private AudioClip toggleOnSound;        // Звук ВКЛЮЧЕНИЯ (ToggleMode)
        [SerializeField] private AudioClip toggleOffSound;       // Звук ВЫКЛЮЧЕНИЯ (ToggleMode)
        [SerializeField] private float soundVolume = 0.5f;
    
        [Header("Визуальные эффекты")]
        [SerializeField] private GameObject pickupEffect;
        [SerializeField] private GameObject highlightObject;
        [SerializeField] private Material highlightMaterial;
        [SerializeField] private Material defaultMaterial;
    
        private bool isPickedUp = false;
        private bool isToggleState = false;
        private MeshRenderer[] meshRenderers;
        private Material[] originalMaterials;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
    
        void Start()
        {
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
        
            originalPosition = transform.position;
            originalRotation = transform.rotation;
        
            isToggleState = startState;
            if (arrayMode == ArrayMode.ToggleMode)
            {
                SetArraysToggleState(isToggleState);
            }
        }
    
        public void Pickup()
        {
            if (isPickedUp && destroyType != PickupDestroyType.DoNothing) return;
        
            Debug.Log($"Подобран предмет: {itemName}, Режим: {arrayMode}");
        
            PlayEffect();
        
            switch (actionType)
            {
                case PickupActionType.ToggleArrays:
                    HandleArrays();
                    break;
                
                case PickupActionType.FadeToScene:
                    FadeToScene();
                    break;
                
                case PickupActionType.FadeAndToggle:
                    HandleArrays();
                    FadeToScene();
                    break;
            }
        
            HandlePickupObject();
        }
    
        void HandleArrays()
        {
            if (arrayMode == ArrayMode.OneTimeEnable)
            {
                OneTimeEnableMode();
            }
            else if (arrayMode == ArrayMode.ToggleMode)
            {
                ToggleMode();
            }
        }
    
        void OneTimeEnableMode()
        {
            PlaySound(pickupSound);
        
            if (arrayToEnable != null)
            {
                foreach (GameObject obj in arrayToEnable)
                {
                    if (obj != null) obj.SetActive(true);
                }
            }
        
            if (arrayToDisable != null)
            {
                foreach (GameObject obj in arrayToDisable)
                {
                    if (obj != null) obj.SetActive(false);
                }
            }
        }
    
        void ToggleMode()
        {
            // Переключаем состояние
            isToggleState = !isToggleState;
            SetArraysToggleState(isToggleState);
        
            // Воспроизводим РАЗНЫЕ звуки для включения и выключения
            if (isToggleState)
            {
                PlaySound(toggleOnSound);
                Debug.Log($"Переключатель {itemName}: ВКЛЮЧЕН");
            }
            else
            {
                PlaySound(toggleOffSound);
                Debug.Log($"Переключатель {itemName}: ВЫКЛЮЧЕН");
            }
        }
    
        void SetArraysToggleState(bool state)
        {
            if (arrayToToggle != null)
            {
                foreach (GameObject obj in arrayToToggle)
                {
                    if (obj != null) obj.SetActive(state);
                }
            }
        }
    
        void FadeToScene()
        {
            if (!string.IsNullOrEmpty(targetSceneName))
            {
                ScreenFader.FadeToScene(targetSceneName, fadeDuration);
            }
            else
            {
                ScreenFader.FadeToScene(targetSceneIndex, fadeDuration);
            }
        }
    
        void PlayEffect()
        {
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }
        }
    
        void PlaySound(AudioClip clip)
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, transform.position, soundVolume);
            }
        }
    
        void HandlePickupObject()
        {
            switch (destroyType)
            {
                case PickupDestroyType.Destroy:
                    Destroy(gameObject);
                    break;
                
                case PickupDestroyType.Disable:
                    gameObject.SetActive(false);
                    break;
                
                case PickupDestroyType.DoNothing:
                    if (arrayMode == ArrayMode.OneTimeEnable)
                    {
                        isPickedUp = true;
                    }
                    break;
                
                case PickupDestroyType.RespawnAfterDelay:
                    gameObject.SetActive(false);
                    Invoke(nameof(Respawn), respawnDelay);
                    break;
            }
        }
    
        void Respawn()
        {
            gameObject.SetActive(true);
            transform.position = originalPosition;
            transform.rotation = originalRotation;
            isPickedUp = false;
        }
    
        public void ResetPickup()
        {
            isPickedUp = false;
            gameObject.SetActive(true);
            transform.position = originalPosition;
            transform.rotation = originalRotation;
        }
    
        public void SetHighlight(bool highlight)
        {
            if (highlightObject != null)
            {
                highlightObject.SetActive(highlight);
            }
            else if (highlightMaterial != null && defaultMaterial != null && meshRenderers != null)
            {
                foreach (var renderer in meshRenderers)
                {
                    renderer.material = highlight ? highlightMaterial : defaultMaterial;
                }
            }
        }
    
        public bool CanPickup()
        {
            return !isPickedUp || arrayMode == ArrayMode.ToggleMode;
        }
    
        public bool GetToggleState()
        {
            return isToggleState;
        }
    
        void OnDrawGizmosSelected()
        {
            Gizmos.color = arrayMode == ArrayMode.ToggleMode ? Color.cyan : Color.green;
            Gizmos.DrawWireSphere(transform.position, 2f);
        }
    }
}