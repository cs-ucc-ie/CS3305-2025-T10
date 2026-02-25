using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip bgmClip;
    private AudioSource audioSource;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        InventorySlotUI.OnQuickSlotClicked += PlayUIClickSound;      // 点击 UI
        // QuickSlotUI.OnQuickSlotClicked += ;          // 点击 UI
        // InputManager.OnInteractPressed += ;          // 玩家交互
        // InputManager.OnInventoryTogglePressed += ;   // 玩家打开/关闭背包
        // AbilityDash.OnDashUsed += ;                  // 玩家使用冲刺
        // AbilitySlowTime.OnSlowTimeEnabled += ;       // 玩家使用子弹时间
        // AbilitySlowTime.OnSlowTimeDisabled += ;      // 玩家停止子弹时间
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        InventorySlotUI.OnQuickSlotClicked -= PlayUIClickSound;      // 点击 UI
        // QuickSlotUI.OnQuickSlotClicked -= ;          // 点击 UI
        // InputManager.OnInteractPressed -= ;          // 玩家交互
        // InputManager.OnInventoryTogglePressed -= ;   // 玩家打开/关闭背包
        // AbilityDash.OnDashUsed -= ;                  // 玩家使用冲刺
        // AbilitySlowTime.OnSlowTimeEnabled -= ;       // 玩家使用子弹时间
        // AbilitySlowTime.OnSlowTimeDisabled -= ;      // 玩家停止子弹时间
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Camera mainCamera = Camera.main;
        AudioSource existingAudioSource = mainCamera.GetComponent<AudioSource>();
        audioSource = existingAudioSource;
        PlayBGM();
    }

    private void PlayBGM()
    {
        audioSource.clip = bgmClip;
        audioSource.loop = true;
        audioSource.Play();
    }

    // 用 audioSource.PlayOneShot(AudioClip) 来播放一次性音效，而不打断 BGM
    private void PlayUIClickSound()
    {
        // 在这里替换音效
        // audioSource.PlayOneShot(uiClickClip);
    }
}
