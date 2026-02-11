using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum HumanFormEnemyAnimationState { Idle, Walk, WeaponAttackStartUp, WeaponAttack, MeleeAttack, Hurt, Dead }
public enum EightDirection { Front, FrontLeft, Left, BackLeft, Back, BackRight, Right, FrontRight }

[System.Serializable]
class HumanFormEnemySpriteMapping
{
    public int index;
    public HumanFormEnemyAnimationState state;
    public EightDirection direction;
    public int frame;
}

public class HumanFormEnemyAnimator : MonoBehaviour
{
    [Header("Sprite Animation Config")]
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private int walkFrames;
    [SerializeField] private int weaponAttackStartUpFrames;
    [SerializeField] private int weaponAttackFrames;
    [SerializeField] private int meleeAttackFrames;
    [SerializeField] private int idleFrames;
    [SerializeField] private int hurtFrames;
    [SerializeField] private int deadFrames;
    [SerializeField] private float animationFrameRate;

    private List<HumanFormEnemySpriteMapping> spriteMappings;
    private SpriteRenderer spriteRenderer;
    private HumanFormEnemyAnimationState animationState;
    private EightDirection animationDirection;

    private bool isAnimationDone;
    private int currentFrame;
    private float frameTimer;

    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        spriteMappings = GenerateMappings(sprites, walkFrames, weaponAttackStartUpFrames, weaponAttackFrames,
            meleeAttackFrames, idleFrames, hurtFrames, deadFrames);
        animationState = HumanFormEnemyAnimationState.Idle;
        animationDirection = EightDirection.Front;
        currentFrame = 0;
        frameTimer = 0f;
    }

    private List<HumanFormEnemySpriteMapping> GenerateMappings(Sprite[] sprites,
    int walkFrames, int weaponAttackStartUpFrames, int weaponAttackFrames, int meleeAttackFrames,
    int idleFrames, int hurtFrames, int deadFrames)
    {
        var mappings = new List<HumanFormEnemySpriteMapping>();
        int index = 0;

        void AddStateMappings(HumanFormEnemyAnimationState state, int frames)
        {
            for (int frame = 0; frame < frames; frame++)
            {
                foreach (EightDirection dir in System.Enum.GetValues(typeof(EightDirection)))
                {
                    mappings.Add(new HumanFormEnemySpriteMapping
                    {
                        index = index++,
                        state = state,
                        direction = dir,
                        frame = frame
                    });
                }
            }
        }

        AddStateMappings(HumanFormEnemyAnimationState.Walk, walkFrames);
        AddStateMappings(HumanFormEnemyAnimationState.WeaponAttackStartUp, weaponAttackStartUpFrames);
        AddStateMappings(HumanFormEnemyAnimationState.WeaponAttack, weaponAttackFrames);
        AddStateMappings(HumanFormEnemyAnimationState.MeleeAttack, meleeAttackFrames);
        AddStateMappings(HumanFormEnemyAnimationState.Idle, idleFrames);
        AddStateMappings(HumanFormEnemyAnimationState.Hurt, hurtFrames);
        AddStateMappings(HumanFormEnemyAnimationState.Dead, deadFrames);
        return mappings;
    }

    public void BeginAnimation(HumanFormEnemyAnimationState state)
    {
     isAnimationDone = false;
        if (animationState != state)
        {
            animationState = state;
            currentFrame = 0;
            frameTimer = 0f;
        }
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
            if (animationState == HumanFormEnemyAnimationState.Idle ||
             animationState == HumanFormEnemyAnimationState.Walk ||
              animationState == HumanFormEnemyAnimationState.WeaponAttackStartUp ||
              animationState == HumanFormEnemyAnimationState.Hurt)
            {
                int maxFrame = spriteMappings.Count(m => m.state == animationState && m.direction == animationDirection);
                currentFrame = (currentFrame + 1) % maxFrame;
            }
            else if (animationState == HumanFormEnemyAnimationState.WeaponAttack || animationState == HumanFormEnemyAnimationState.MeleeAttack || animationState == HumanFormEnemyAnimationState.Dead)
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