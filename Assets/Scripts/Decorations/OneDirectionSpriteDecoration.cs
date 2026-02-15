using System;
using UnityEngine;

public class OneDirectionSpriteDecoration : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] sprite;
    [SerializeField] private float animationFrameRate = 6f;

    void Start()
    {
         spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if (sprite.Length > 0)
        {
            int frameIndex = (int)(Time.time * animationFrameRate) % sprite.Length;
            spriteRenderer.sprite = sprite[frameIndex];
        }
        // sprite renderer always face the camera
        spriteRenderer.transform.forward = Camera.main.transform.forward;
        //Vector3 currentRotation = spriteRenderer.transform.rotation.eulerAngles;
        spriteRenderer.transform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
    }
}