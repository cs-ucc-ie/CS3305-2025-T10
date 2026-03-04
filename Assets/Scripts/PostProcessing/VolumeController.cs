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
    private Vignette vignette;

    [SerializeField] private float duration = 3f;
    private float elapsed = 0f;
    [SerializeField] private Color startColor = new Color(1f, 0.7f, 0.7f, 1f); // light red
    [SerializeField] private Color endColor = new Color(0.6f, 0f, 0f, 1f); // dark red
    private Vector3 defaultCameraLocalPosition;
    private bool hasDefaultCameraLocalPosition;

    private void HandleSlowTimeEnabled() => SetBlackWhite(true);
    private void HandleSlowTimeDisabled() => SetBlackWhite(false);

    void OnEnable()
    {
        AbilitySlowTime.OnSlowTimeEnabled += HandleSlowTimeEnabled;
        AbilitySlowTime.OnSlowTimeDisabled += HandleSlowTimeDisabled;
        SceneManager.sceneLoaded += OnSceneLoaded;
        PlayerStatsManager.OnPlayerDamaged += QuickRedFlash;
    }

    void OnDisable()
    {
        AbilitySlowTime.OnSlowTimeEnabled -= HandleSlowTimeEnabled;
        AbilitySlowTime.OnSlowTimeDisabled -= HandleSlowTimeDisabled;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        PlayerStatsManager.OnPlayerDamaged -= QuickRedFlash;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeVolumeReferences();
        ResetPostProcessing();
        ResetMainCameraPosition();
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
        InitializeVolumeReferences();
        ResetPostProcessing();
    }

    private void InitializeVolumeReferences()
    {
        volume = GetComponentInChildren<Volume>();
        if (volume == null || volume.profile == null)
        {
            colorAdjustments = null;
            vignette = null;
            return;
        }

        volume.profile.TryGet(out colorAdjustments);
        volume.profile.TryGet(out vignette);
    }

    public void SetBlackWhite(bool enabled)
    {
        if (colorAdjustments == null) return;

        if (enabled)
            colorAdjustments.saturation.value = -100f;
        else
            colorAdjustments.saturation.value = 0f;
    }

    public void ResetPostProcessing()
    {
        Debug.Log("Resetting post-processing effects to default.");
        if (colorAdjustments != null)
        {
            Debug.Log("Resetting Color Adjustments.");
            colorAdjustments.colorFilter.overrideState = false;
            colorAdjustments.colorFilter.value = Color.white;
            colorAdjustments.saturation.value = 0f;
        }
        if (vignette != null)
        {
            Debug.Log("Resetting Vignette.");
            vignette.color.value = Color.black;
            vignette.intensity.value = 0f;
        }
    }

    private void CacheMainCameraDefaultPosition()
    {
        if (hasDefaultCameraLocalPosition) return;

        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        defaultCameraLocalPosition = mainCamera.transform.localPosition;
        hasDefaultCameraLocalPosition = true;
    }

    private void ResetMainCameraPosition()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null || !hasDefaultCameraLocalPosition) return;

        mainCamera.transform.localPosition = defaultCameraLocalPosition;
        CameraBob cameraBob = mainCamera != null ? mainCamera.GetComponent<CameraBob>() : null;
        if (cameraBob != null)
        {
            cameraBob.enabled = true;
        }
    }

    public void QuickRedFlash(){
        StartCoroutine(QuickRedFlashCoroutine());
    }

    private IEnumerator QuickRedFlashCoroutine()
    {
        if (vignette == null) yield break;
        
        vignette.color.value = new Color(1f, 0.2f, 0.2f, 1f); // Red vignette color
        vignette.intensity.value = 0.5f; // Max intensity for the flash
        
        yield return new WaitForSeconds(0.1f);
        
        // Fade out the vignette
        float fadeTime = 0.3f;
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeTime;
            vignette.intensity.value = Mathf.Lerp(0.5f, 0f, t);
            yield return null;
        }
        
        vignette.intensity.value = 0f;
    }

    public void FadeToRed(System.Action onComplete)
    {
        StartCoroutine(FadeToRedCoroutine(onComplete));
    }

    private IEnumerator FadeToRedCoroutine(System.Action onComplete)
    {
        if (colorAdjustments == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        elapsed = 0f; // Reset timer
        colorAdjustments.colorFilter.overrideState = true;

        CacheMainCameraDefaultPosition();
        Camera mainCamera = Camera.main;
        Vector3 startPosition = mainCamera != null ? mainCamera.transform.localPosition : Vector3.zero;
        Vector3 endPosition = startPosition + Vector3.down * 0.5f; // Move down 0.5 unit
        CameraBob cameraBob = mainCamera != null ? mainCamera.GetComponent<CameraBob>() : null;
        if (cameraBob != null)
        {
            cameraBob.enabled = false; // Disable camera bobbing during fade
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            colorAdjustments.colorFilter.value = Color.Lerp(startColor, endColor, t);
            
            // Move camera down simultaneously
            if (mainCamera != null)
            {
                mainCamera.transform.localPosition = Vector3.Lerp(startPosition, endPosition, t);
            }

            yield return null;
        }

        colorAdjustments.colorFilter.value = endColor;
        onComplete?.Invoke();
    }
}
