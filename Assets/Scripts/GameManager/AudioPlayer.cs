using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip bgmClip;
    private AudioSource audioSource;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Camera mainCamera = Camera.main;
        AudioSource existingAudioSource = mainCamera.GetComponent<AudioSource>();
        if (existingAudioSource != null)
        {
            audioSource = existingAudioSource;
        }
        else
        {
            audioSource = mainCamera.gameObject.AddComponent<AudioSource>();
        }
        audioSource.clip = bgmClip;
        audioSource.loop = true;
        audioSource.Play();
    }
}
