using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class VolumeController : MonoBehaviour
{
    public static VolumeController Instance;
    private Volume volume;
    private ColorAdjustments colorAdjustments;

    [SerializeField] private float duration = 3f;
    private float elapsed = 0f;
    [SerializeField] private Color startColor = new Color(1f, 0.7f, 0.7f, 1f); // light red
    [SerializeField] private Color endColor = new Color(0.6f, 0f, 0f, 1f); // dark red

    void OnEnable()
    {
        AbilitySlowTime.OnSlowTimeEnabled += () => SetBlackWhite(true);
        AbilitySlowTime.OnSlowTimeDisabled += () => SetBlackWhite(false);
        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayerStatsManager.OnPlayerDamaged += QuickRedFlash;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        PlayerStatsManager.OnPlayerDamaged -= QuickRedFlash;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset post-processing when a new scene is loaded
        if (colorAdjustments != null)
        {
            ResetPostProcessing();
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        volume = GetComponentInChildren<Volume>();
        volume.profile.TryGet(out colorAdjustments);
        ResetPostProcessing();
    }

    public void SetBlackWhite(bool enabled)
    {
        if (enabled)
            colorAdjustments.saturation.value = -100f;
        else
            colorAdjustments.saturation.value = 0f;
    }

    public void ResetPostProcessing()
    {
        if (colorAdjustments != null)
        {
            colorAdjustments.colorFilter.overrideState = false;
            colorAdjustments.colorFilter.value = Color.white;
            colorAdjustments.saturation.value = 0f;
        }
    }

    public void QuickRedFlash(){
        StartCoroutine(QuickRedFlashCoroutine());
    }

    private IEnumerator QuickRedFlashCoroutine()
    {
        colorAdjustments.colorFilter.overrideState = true;
        colorAdjustments.colorFilter.value = new Color(1f, 0.8f, 0.8f, 0.01f);
        yield return new WaitForSeconds(0.1f);
        // if already in the middle of a fade to red, don't reset to white
        if (colorAdjustments.colorFilter.value.a > 0.01f) yield break;
        colorAdjustments.colorFilter.value = Color.white;
        colorAdjustments.colorFilter.overrideState = false;
    }

    public void FadeToRed(System.Action onComplete)
    {
        StartCoroutine(FadeToRedCoroutine(onComplete));
    }

    private IEnumerator FadeToRedCoroutine(System.Action onComplete)
    {
        elapsed = 0f; // Reset timer
        colorAdjustments.colorFilter.overrideState = true;

        Camera mainCamera = Camera.main;
        Vector3 startPosition = mainCamera != null ? mainCamera.transform.position : Vector3.zero;
        Vector3 endPosition = startPosition + Vector3.down * 0.5f; // Move down 0.5 unit
        CameraBob cameraBob = mainCamera.GetComponent<CameraBob>();
        cameraBob.enabled = false; // Disable camera bobbing during fade

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            colorAdjustments.colorFilter.value = Color.Lerp(startColor, endColor, t);
            
            // Move camera down simultaneously
            if (mainCamera != null)
            {
                mainCamera.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            }

            yield return null;
        }

        colorAdjustments.colorFilter.value = endColor;
        onComplete?.Invoke();
    }
}
