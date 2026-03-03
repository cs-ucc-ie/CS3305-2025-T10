using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BossAnimationState { Walk, WeaponAttackStartUp, WeaponAttackOnce, WeaponAttackEnd, Dash, Dead }

[System.Serializable]
class BossSpriteMapping
{
    public int index;
    public BossAnimationState state;
    public EightDirection direction;
    public int frame;
}

public class BossAnimator : MonoBehaviour
{
    [Header("Sprite Animation Config")]
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private int walkFrames;
    [SerializeField] private int weaponAttackStartUpFrames;
    [SerializeField] private int weaponAttackFrames;
    [SerializeField] private int weaponAttackEndFrames;
    [SerializeField] private int dashFrames;
    [SerializeField] private int deadFrames;
    [SerializeField] private float animationFrameRate;
    [SerializeField] private BossAnimationState animationState;

    private List<BossSpriteMapping> spriteMappings;
    private SpriteRenderer spriteRenderer;
    private EightDirection animationDirection;

    private bool isAnimationDone;
    private int currentFrame;
    private float frameTimer;
    private bool stayInSpecificFrame;
    private int loopCount;

    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteMappings = GenerateMappings(sprites, walkFrames, weaponAttackStartUpFrames, weaponAttackFrames,
            weaponAttackEndFrames, dashFrames, deadFrames);
        animationState = BossAnimationState.Walk;
        animationDirection = EightDirection.Front;
        currentFrame = 0;
        frameTimer = 0f;
    }

    private List<BossSpriteMapping> GenerateMappings(Sprite[] sprites,
    int walkFrames, int weaponAttackStartUpFrames, int weaponAttackFrames, int weaponAttackEndFrames,
    int dashFrames, int deadFrames)
    {
        var mappings = new List<BossSpriteMapping>();
        int index = 0;

        void AddStateMappings(BossAnimationState state, int frames)
        {
            for (int frame = 0; frame < frames; frame++)
            {
                foreach (EightDirection dir in System.Enum.GetValues(typeof(EightDirection)))
                {
                    mappings.Add(new BossSpriteMapping
                    {
                        index = index++,
                        state = state,
                        direction = dir,
                        frame = frame
                    });
                }
            }
        }

        AddStateMappings(BossAnimationState.Walk, walkFrames);
        AddStateMappings(BossAnimationState.WeaponAttackStartUp, weaponAttackStartUpFrames);
        AddStateMappings(BossAnimationState.WeaponAttackOnce, weaponAttackFrames);
        // weapon attack end share the last frame of weapon attack animation, so start from frame - 1
        index -= 8;
        AddStateMappings(BossAnimationState.WeaponAttackEnd, weaponAttackEndFrames);
        AddStateMappings(BossAnimationState.Dash, dashFrames);
        AddStateMappings(BossAnimationState.Dead, deadFrames);
        return mappings;
    }

    public void BeginAnimation(BossAnimationState state)
    {
        isAnimationDone = false;
        animationState = state;
        currentFrame = 0;
        frameTimer = 0f;
        stayInSpecificFrame = false;
        loopCount = 0;
    }

    public void StayInSpecificFrame(int frame, BossAnimationState state)
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
                if walk or attack startup or attack end or dash, loop animation
                if attack or dead, play once and stop at last frame
            **/
            if (animationState == BossAnimationState.Walk || 
            animationState == BossAnimationState.WeaponAttackStartUp ||
            animationState == BossAnimationState.WeaponAttackEnd ||
            animationState == BossAnimationState.Dash)
            {
                int maxFrame = spriteMappings.Count(m => m.state == animationState && m.direction == animationDirection);
                currentFrame = (currentFrame + 1) % maxFrame;

                if (currentFrame == 0)
                {
                    loopCount++;
                    if (loopCount >= 1)
                    {
                        isAnimationDone = true;
                    }
                }
            }
            else if (animationState == BossAnimationState.WeaponAttackOnce || animationState == BossAnimationState.Dead)
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

    public BossAnimationState GetCurrentAnimationState()
    {
        return animationState;
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