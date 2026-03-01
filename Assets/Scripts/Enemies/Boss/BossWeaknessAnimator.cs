using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum BossWeaknessAnimationState { Idle, Attack, Dead }

[System.Serializable]
class BossWeaknessSpriteMapping
{
    public int index;
    public BossWeaknessAnimationState state;
    public EightDirection direction;
    public int frame;
}

public class BossWeaknessAnimator : MonoBehaviour
{
    [Header("Sprite Animation Config")]
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private int attackFrames;
    [SerializeField] private int idleFrames;
    [SerializeField] private int deadFrames;
    [SerializeField] private float animationFrameRate;

    private List<BossWeaknessSpriteMapping> spriteMappings;
    private SpriteRenderer spriteRenderer;
    private BossWeaknessAnimationState animationState;
    private EightDirection animationDirection;

    private bool isAnimationDone;
    private int currentFrame;
    private float frameTimer;
    private bool stayInSpecificFrame;

    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteMappings = GenerateMappings(sprites, idleFrames, attackFrames, deadFrames);
        animationState = BossWeaknessAnimationState.Idle;
        animationDirection = EightDirection.Front;
        currentFrame = 0;
        frameTimer = 0f;
    }

    private List<BossWeaknessSpriteMapping> GenerateMappings(Sprite[] sprites, int idleFrames, int attackFrames, int deadFrames)
    {
        var mappings = new List<BossWeaknessSpriteMapping>();
        int index = 0;

        void AddStateMappings(BossWeaknessAnimationState state, int frames)
        {
            for (int frame = 0; frame < frames; frame++)
            {
                foreach (EightDirection dir in System.Enum.GetValues(typeof(EightDirection)))
                {
                    mappings.Add(new BossWeaknessSpriteMapping
                    {
                        index = index++,
                        state = state,
                        direction = dir,
                        frame = frame
                    });
                }
            }
        }
        AddStateMappings(BossWeaknessAnimationState.Idle, idleFrames);
        AddStateMappings(BossWeaknessAnimationState.Attack, attackFrames);
        AddStateMappings(BossWeaknessAnimationState.Dead, deadFrames);
        return mappings;
    }

    public void BeginAnimation(BossWeaknessAnimationState state)
    {
        isAnimationDone = false;
        if (animationState != state)
        {
            animationState = state;
            currentFrame = 0;
            frameTimer = 0f;
        }
        stayInSpecificFrame = false;
    }

    public void StayInSpecificFrame(int frame, BossWeaknessAnimationState state)
    {
        if (animationState != state)
        {
            animationState = state;
            frameTimer = 0f;
        }
        currentFrame = frame;
        stayInSpecificFrame = true;

    }
    void Update()
    {
        // sprite renderer always face the camera
        spriteRenderer.transform.forward = Camera.main.transform.forward;

        Vector3 toCamera = Camera.main.transform.position - transform.position;
        toCamera.y = 0f;
        if (toCamera.sqrMagnitude > 0.0001f)
        {
            float angle = Vector3.SignedAngle(transform.forward, toCamera, Vector3.up);
            EightDirection dir = AngleToDirection(angle);
            animationDirection = dir;
        }

        if (stayInSpecificFrame)
        {
            var frameMapping = spriteMappings.FirstOrDefault(m =>
                m.state == animationState &&
                m.direction == animationDirection &&
                m.frame == currentFrame);

            if (frameMapping != null)
                spriteRenderer.sprite = sprites[frameMapping.index];
                return;
        }

        // play animation
        frameTimer += Time.deltaTime;
        if (frameTimer > 1f / animationFrameRate)
        {
            frameTimer = 0f;

            // find the correct sprite for current state, direction, and frame
            var frameMapping = spriteMappings.FirstOrDefault(m =>
                m.state == animationState &&
                m.direction == animationDirection &&
                m.frame == currentFrame);

            if (frameMapping != null)
                spriteRenderer.sprite = sprites[frameMapping.index];

            /**
                if idle or walk or attack startup or hurt, loop animation
                if attack or dead, play once and stop at last frame
            **/
            if (animationState == BossWeaknessAnimationState.Idle)
            {
                int maxFrame = spriteMappings.Count(m => m.state == animationState && m.direction == animationDirection);
                currentFrame = (currentFrame + 1) % maxFrame;
            }
            else if (animationState == BossWeaknessAnimationState.Attack || animationState == BossWeaknessAnimationState.Dead)
            {
                currentFrame++;

                int maxFrame = spriteMappings.Count(m => m.state == animationState && m.direction == animationDirection);
                if (currentFrame >= maxFrame)
                {
                    currentFrame = maxFrame - 1;
                    // finish of attack animation
                    isAnimationDone = true;
                } 
            }
        }
    }

    public bool IsCurrentAnimationDone()
    {
        return isAnimationDone;
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