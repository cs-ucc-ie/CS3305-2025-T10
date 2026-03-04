using UnityEngine;

public class FlashVisualEffect : MonoBehaviour
{
    private Light pointLight;
    private float flashDuration = 0.1f;
    private float flashTimer = 0f;

    void Start()
    {
        pointLight = GetComponent<Light>();
    }

    void Update()
    {
        if (pointLight == null) return;

        flashTimer += Time.deltaTime;
        if (flashTimer >= flashDuration)
        {
            pointLight.enabled = false;
            Destroy(gameObject);
        }
    }
}
