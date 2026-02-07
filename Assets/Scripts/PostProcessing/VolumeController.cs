using UnityEngine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumeController : MonoBehaviour
{
    public static VolumeController Instance;
    public Volume volume;
    private ColorAdjustments colorAdjustments;

    void OnEnable()
    {
        AbilitySlowTime.OnSlowTimeEnabled += () => SetBlackWhite(true);
        AbilitySlowTime.OnSlowTimeDisabled += () => SetBlackWhite(false);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
    }

    public void SetBlackWhite(bool enabled)
    {
        if (enabled)
            colorAdjustments.saturation.value = -100f;
        else
            colorAdjustments.saturation.value = 0f;
    }
}
