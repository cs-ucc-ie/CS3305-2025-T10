using UnityEngine;

public class ExplodeVisualEffect : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] sprite;
    [SerializeField] private float animationFrameRate = 6f;
    private float timer = 0f;
    private int frameIndex = 0;
    void Start()
    {
         spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteRenderer.sprite = sprite[frameIndex];

    }

    void Update()
    {     
        timer += Time.deltaTime;
        if (timer >= 1f / animationFrameRate)
            {
                timer = 0f;
                frameIndex++;
                if (frameIndex >= sprite.Length)
                {
                    Destroy(gameObject);
                    return;
                }
                spriteRenderer.sprite = sprite[frameIndex];
        }
        // sprite renderer always face the camera
        spriteRenderer.transform.forward = Camera.main.transform.forward;
        //Vector3 currentRotation = spriteRenderer.transform.rotation.eulerAngles;
        spriteRenderer.transform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
    }
}
