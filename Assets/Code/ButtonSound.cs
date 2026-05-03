using UnityEngine;
using UnityEngine.UI;

namespace Code
{
    public class ButtonSound : MonoBehaviour
    {
        [SerializeField] private AudioClip sound;
        [SerializeField] private float volume = 0.7f;
    
        private AudioSource audioSource;
    
        void Start()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        
            GetComponent<Button>().onClick.AddListener(() =>
            {
                if (sound != null)
                    audioSource.PlayOneShot(sound, volume);
            });
        }
    }
}