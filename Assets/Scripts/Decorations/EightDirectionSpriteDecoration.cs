using System;
using UnityEngine;

public class EightDirectionSpriteDecoration : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] sprites;

    void Start()
    {
         spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        // sprite renderer always face the camera
        spriteRenderer.transform.forward = Camera.main.transform.forward;
        Vector3 toCamera = Camera.main.transform.position - transform.position;
        toCamera.y = 0f;
        //Vector3 currentRotation = spriteRenderer.transform.rotation.eulerAngles;
        spriteRenderer.transform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
        if (toCamera.sqrMagnitude > 0.0001f)
        {
            float angle = Vector3.SignedAngle(transform.forward, toCamera, Vector3.up);
            EightDirection dir = AngleToDirection(angle);
            // Ensure the index is within bounds
            if ((int)dir >= 0 && (int)dir < sprites.Length)
                spriteRenderer.sprite = sprites[(int)dir];
        }
    }

    private EightDirection AngleToDirection(float angle)
    {
        if (angle >= -22.5f && angle < 22.5f)
            return EightDirection.Front;
        else if (angle >= 22.5f && angle < 67.5f)
            return EightDirection.FrontRight;
        else if (angle >= 67.5f && angle < 112.5f)
            return EightDirection.Right;
        else if (angle >= 112.5f && angle < 157.5f)
            return EightDirection.BackRight;
        else if (angle >= 157.5f || angle < -157.5f)
            return EightDirection.Back;
        else if (angle >= -157.5f && angle < -112.5f)
            return EightDirection.BackLeft;
        else if (angle >= -112.5f && angle < -67.5f)
            return EightDirection.Left;
        else
            return EightDirection.FrontLeft; // -67.5 ~ -22.5
    }
}
