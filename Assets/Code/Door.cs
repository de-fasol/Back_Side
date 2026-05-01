using UnityEngine;

namespace Code
{
    public class Door : MonoBehaviour
    {
        [Header("Настройки открывания")]
        [SerializeField] private KeyCode openKey = KeyCode.E;
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float openSpeed = 180f;
        [SerializeField] private float closeSpeed = 180f;
        [SerializeField] private bool autoClose = false;
        [SerializeField] private float autoCloseDelay = 2f;
    
        [Header("Блокировка двери")]
        [SerializeField] private bool isLocked = false;
    
        [Header("Настройки движения")]
        [SerializeField] private bool rotateAroundY = true;
        [SerializeField] private bool rotateAroundX = false;
        [SerializeField] private bool rotateAroundZ = false;
        [SerializeField] private bool openInward = true;
    
        [Header("Анимация покачивания")]
        [SerializeField] private bool enableShakeAnimation = true;
        [SerializeField] private float shakeAngle = 10f;
        [SerializeField] private float shakeSpeed = 360f;
        [SerializeField] private int shakeCount = 2;
    
        [Header("Звуки")]
        [SerializeField] private AudioClip openSound;
        [SerializeField] private AudioClip closeSound;
        [SerializeField] private AudioClip lockedSound;
        [SerializeField] private float soundVolume = 0.5f;
    
        [Header("Визуальные эффекты")]
        [SerializeField] private GameObject highlightObject;
    
        private bool isOpen = false;
        private bool isAnimating = false;
        private bool isShaking = false;
        private float currentAngle = 0f;
        private float targetAngle = 0f;
        private Quaternion closedRotation;
        private Quaternion openRotation;
        private AudioSource audioSource;
        private float autoCloseTimer = 0f;
        private float shakePhase = 0f;
    
        public System.Action OnDoorLocked;
    
        void Start()
        {
            closedRotation = transform.localRotation;
        
            float direction = openInward ? 1f : -1f;
            float angle = openAngle * direction;
        
            Vector3 rotationVector = Vector3.zero;
        
            if (rotateAroundY)
                rotationVector = new Vector3(0, angle, 0);
            else if (rotateAroundX)
                rotationVector = new Vector3(angle, 0, 0);
            else if (rotateAroundZ)
                rotationVector = new Vector3(0, 0, angle);
            else
                rotationVector = new Vector3(0, angle, 0);
        
            openRotation = closedRotation * Quaternion.Euler(rotationVector);
            targetAngle = 0f;
            currentAngle = 0f;
        
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null && (openSound != null || closeSound != null || lockedSound != null))
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        
            if (highlightObject != null)
                highlightObject.SetActive(false);
        }
    
        void Update()
        {
            // Анимация покачивания
            if (isShaking)
            {
                shakePhase += Time.deltaTime * shakeSpeed;
            
                if (shakePhase >= Mathf.PI * 2 * shakeCount)
                {
                    isShaking = false;
                    SetDoorAngle(0f);
                    shakePhase = 0f;
                    return;
                }
            
                float t = shakePhase / (Mathf.PI * 2 * shakeCount);
                float amplitude = shakeAngle * (1 - t);
                float angle = Mathf.Sin(shakePhase) * amplitude;
                SetDoorAngle(angle);
            }
        
            // Плавное вращение двери
            if (isAnimating)
            {
                float speed = isOpen ? openSpeed : closeSpeed;
                float step = speed * Time.deltaTime;
            
                if (Mathf.Abs(currentAngle - targetAngle) < step)
                {
                    currentAngle = targetAngle;
                    SetDoorAngle(currentAngle);
                    isAnimating = false;
                
                    if (!isOpen)
                    {
                        PlaySound(closeSound);
                    }
                }
                else
                {
                    currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, step);
                    SetDoorAngle(currentAngle);
                }
            }
        
            // Автозакрывание
            if (autoClose && isOpen && !isAnimating && !isShaking)
            {
                autoCloseTimer -= Time.deltaTime;
                if (autoCloseTimer <= 0f)
                {
                    CloseDoor();
                }
            }
        }
    
        void SetDoorAngle(float angle)
        {
            Vector3 rotationVector = Vector3.zero;
            float direction = openInward ? 1f : -1f;
            float finalAngle = angle * direction;
        
            if (rotateAroundY)
                rotationVector = new Vector3(0, finalAngle, 0);
            else if (rotateAroundX)
                rotationVector = new Vector3(finalAngle, 0, 0);
            else if (rotateAroundZ)
                rotationVector = new Vector3(0, 0, finalAngle);
        
            transform.localRotation = closedRotation * Quaternion.Euler(rotationVector);
        }
    
        public void Interact()
        {
            if (isLocked)
            {
                PlaySound(lockedSound);
                OnDoorLocked?.Invoke();
            
                if (enableShakeAnimation && !isShaking && !isAnimating && !isOpen)
                {
                    isShaking = true;
                    shakePhase = 0f;
                }
                return;
            }
        
            if (isOpen)
                CloseDoor();
            else
                OpenDoor();
        }
    
        public void OpenDoor()
        {
            if (isOpen || isAnimating || isShaking) return;
        
            isOpen = true;
            isAnimating = true;
            targetAngle = openAngle;
            autoCloseTimer = autoCloseDelay;
            PlaySound(openSound);
            Debug.Log("Дверь открывается...");
        }
    
        public void CloseDoor()
        {
            if (!isOpen || isAnimating || isShaking) return;
        
            isOpen = false;
            isAnimating = true;
            targetAngle = 0f;
            Debug.Log("Дверь закрывается...");
        }
    
        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip, soundVolume);
            }
        }
    
        public void SetHighlight(bool highlight)
        {
            if (highlightObject != null)
            {
                highlightObject.SetActive(highlight);
            }
        }
    
        public bool IsLocked() => isLocked;
        public bool IsOpen() => isOpen;
    
        void OnDrawGizmosSelected()
        {
            Gizmos.color = isLocked ? Color.red : Color.green;
        
            if (rotateAroundY)
            {
                Vector3 forwardDirection = openInward ? transform.forward : -transform.forward;
                Gizmos.DrawRay(transform.position, forwardDirection * 1.5f);
            }
            else if (rotateAroundX)
            {
                Vector3 upDirection = openInward ? transform.up : -transform.up;
                Gizmos.DrawRay(transform.position, upDirection * 1.5f);
            }
            else if (rotateAroundZ)
            {
                Vector3 rightDirection = openInward ? transform.right : -transform.right;
                Gizmos.DrawRay(transform.position, rightDirection * 1.5f);
            }
        }
    }
}