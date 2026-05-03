using UnityEngine;

namespace Code
{
    public class PickupItem : MonoBehaviour
    {
        [Header("Настройки подбора")]
        [SerializeField] private string itemName = "Предмет";
        
        [Header("Настройки взаимодействия")]
        [SerializeField] private Transform interactionCenter;
        [SerializeField] private float interactionRadius = 2f;
        [SerializeField] private bool useRaycast = true;      // Использовать луч
        [SerializeField] private bool useRadius = true;       // Использовать радиус
        [SerializeField] private bool showGizmo = true;

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
            OneTimeEnable,
            ToggleMode
        }
    
        [SerializeField] private ArrayMode arrayMode = ArrayMode.OneTimeEnable;
    
        [Header("Режим: OneTimeEnable")]
        [SerializeField] private GameObject[] arrayToEnable;
        [SerializeField] private GameObject[] arrayToDisable;
    
        [Header("Режим: ToggleMode")]
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
        [SerializeField] private AudioClip pickupSound;
        [SerializeField] private AudioClip toggleOnSound;
        [SerializeField] private AudioClip toggleOffSound;
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
            
            if (interactionCenter == null)
            {
                GameObject center = new GameObject("InteractionCenter");
                center.transform.SetParent(transform);
                center.transform.localPosition = Vector3.zero;
                interactionCenter = center.transform;
            }
        
            isToggleState = startState;
            if (arrayMode == ArrayMode.ToggleMode)
            {
                SetArraysToggleState(isToggleState);
            }
        }
    
        /// <summary>
        /// Проверка, смотрит ли игрок на предмет (через луч)
        /// </summary>
        public bool IsPlayerLooking(Transform playerCamera)
        {
            if (playerCamera == null) return false;
            if (!useRaycast) return false;
            
            Ray ray = new Ray(playerCamera.position, playerCamera.forward);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, interactionRadius))
            {
                // Проверяем, попали ли в этот предмет или его дочерние объекты
                if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
                {
                    return true;
                }
            }
            return false;
        }
    
        /// <summary>
        /// Проверка, находится ли игрок в зоне взаимодействия (через радиус)
        /// </summary>
        public bool IsPlayerInRange(Transform playerTransform)
        {
            if (playerTransform == null) return false;
            if (!useRadius) return false;
            
            float distance = Vector3.Distance(interactionCenter.position, playerTransform.position);
            return distance <= interactionRadius;
        }
        
        /// <summary>
        /// Комбинированная проверка: луч ИЛИ радиус
        /// </summary>
        public bool CanInteract(Transform playerCamera, Transform playerTransform)
        {
            bool looking = useRaycast && IsPlayerLooking(playerCamera);
            bool inRange = useRadius && IsPlayerInRange(playerTransform);
            
            return looking || inRange;
        }
        
        public float GetDistanceToPlayer(Transform playerTransform)
        {
            if (playerTransform == null) return Mathf.Infinity;
            return Vector3.Distance(interactionCenter.position, playerTransform.position);
        }
        
        public Transform GetInteractionCenter()
        {
            return interactionCenter;
        }
        
        public float GetInteractionRadius()
        {
            return interactionRadius;
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
            isToggleState = !isToggleState;
            SetArraysToggleState(isToggleState);
        
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
            if (!showGizmo) return;
            
            if (interactionCenter != null)
            {
                // Рисуем центр (красная точка)
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(interactionCenter.position, 0.1f);
                
                // Рисуем радиус (зелёная/синяя сфера)
                Gizmos.color = arrayMode == ArrayMode.ToggleMode ? Color.cyan : Color.green;
                Gizmos.DrawWireSphere(interactionCenter.position, interactionRadius);
                
                // Рисуем луч (жёлтая линия) если используется raycast
                if (useRaycast)
                {
                    Gizmos.color = Color.yellow;
                    Vector3 rayEnd = interactionCenter.position + interactionCenter.forward * interactionRadius;
                    Gizmos.DrawLine(interactionCenter.position, rayEnd);
                }
            }
        }
    }
}