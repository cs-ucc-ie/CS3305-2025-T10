using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip bgmClip;
    //玩家点击 UI音效
    [SerializeField] private AudioClip uiClickClip;
    //玩家交互音效
    [SerializeField] private AudioClip InteractPressedClip;
    //玩家打开/关闭背包音效
    [SerializeField] private AudioClip InventoryToggleClip;
    //玩家使用冲刺音效
    [SerializeField] private AudioClip DashClip;
    //玩家使用子弹时间音效
    [SerializeField] private AudioClip SlowTimeEnableClip;
    //玩家停止子弹时间音效
    [SerializeField] private AudioClip SlowTimeDisableClip;
    private AudioSource audioSource;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        InventorySlotUI.OnQuickSlotClicked += PlayUIClickSound;      // 点击 UI
        QuickSlotUI.OnQuickSlotClicked += PlayUIClickSound;          // 点击 UI
        InputManager.OnInteractPressed += PlayInteractPressedSound;          // 玩家交互
        InputManager.OnInventoryTogglePressed += PlayInventoryToggleSound;   // 玩家打开/关闭背包
        AbilityDash.OnDashUsed += PlayDashSound;                  // 玩家使用冲刺
        AbilitySlowTime.OnSlowTimeEnabled += PlaySlowTimeEnableSound;       // 玩家使用子弹时间
        AbilitySlowTime.OnSlowTimeDisabled += PlaySlowTimeDisableSound;      // 玩家停止子弹时间
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        InventorySlotUI.OnQuickSlotClicked -= PlayUIClickSound;      // 点击 UI
        QuickSlotUI.OnQuickSlotClicked -= PlayUIClickSound;          // 点击 UI
        InputManager.OnInteractPressed -= PlayInteractPressedSound;          // 玩家交互
        InputManager.OnInventoryTogglePressed -= PlayInventoryToggleSound;   // 玩家打开/关闭背包
        AbilityDash.OnDashUsed -= PlayDashSound;                  // 玩家使用冲刺
        AbilitySlowTime.OnSlowTimeEnabled -= PlaySlowTimeEnableSound;       // 玩家使用子弹时间
        AbilitySlowTime.OnSlowTimeDisabled -= PlaySlowTimeDisableSound;      // 玩家停止子弹时间
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
        audioSource.PlayOneShot(uiClickClip);
    }

    private void PlayInteractPressedSound() 
    {
        audioSource.PlayOneShot(InteractPressedClip);
        //audioSource.Play(InteractPressedClip);
    }

    private void PlayInventoryToggleSound()
    {
        audioSource.PlayOneShot(InventoryToggleClip);
    }

    private void PlayDashSound()
    {
        audioSource.PlayOneShot(DashClip);
    }

    private void PlaySlowTimeEnableSound()
    {
        audioSource.PlayOneShot(SlowTimeEnableClip);
    }

    private void PlaySlowTimeDisableSound()
    {
        audioSource.PlayOneShot(SlowTimeDisableClip);
    }
}
