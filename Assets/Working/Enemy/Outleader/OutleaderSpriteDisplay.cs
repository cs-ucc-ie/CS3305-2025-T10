using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public enum AnimationState { Idle, Walk, Attack, Dead }
public enum EnemyDir { Front, FrontLeft, Left, BackLeft, Back, BackRight, Right, FrontRight }

[System.Serializable]
public class SpriteMapping
{
    public int index;
    public AnimationState state;
    public EnemyDir direction;
    public int frame;
}

public class OutleaderSpriteDisplay : MonoBehaviour
{
    /*
        This component handles the sprite display and animation for the Outleader enemy.
        It uses a sprite sheet organized by state and direction to show the correct sprite.
    */
    public SpriteRenderer sr;
    private OutleaderAI ai;

    [Header("Sprite Sheet Settings")]
    //public string resourcesFolder = "Enemy/Outleader/OutleaderSprites";
    public int idleFrames = 1;
    public int walkFrames = 4;
    public int attackFrames = 3;
    public int deadFrames = 6;

    public Sprite[] sprites;
    private List<SpriteMapping> mappings;

    [Header("Current State")]
    public AnimationState animationState = AnimationState.Idle;
    public EnemyDir currentDir = EnemyDir.Front;

    private int currentFrame = 0;
    private float timer = 0f;
    public float fps = 6f;

    void Start()
    {
        ai = GetComponent<OutleaderAI>();
        // generate mappings
        mappings = GenerateMappings(sprites, idleFrames, walkFrames, attackFrames, deadFrames);
    }

    void Update()
    {
        // sprite renderer always face the camera
        sr.transform.forward = Camera.main.transform.forward;

        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0f;

        float angle = Vector3.SignedAngle(camForward, ai.facingDir, Vector3.up);
        EnemyDir dir = AngleToDirection(angle);
        currentDir = dir;

        // play animation
        timer += Time.deltaTime;
        if (timer > 1f / fps)
        {
            timer = 0f;

            // find the correct sprite for current state, direction, and frame
            var frameMapping = mappings.FirstOrDefault(m =>
                m.state == animationState &&
                m.direction == currentDir &&
                m.frame == currentFrame);

            if (frameMapping != null)
                sr.sprite = sprites[frameMapping.index];

            /**
                if idle or walk, loop animation
                if attack or dead, play once and stop at last frame
            **/
            if (animationState == AnimationState.Idle || animationState == AnimationState.Walk)
            {
                int maxFrame = mappings.Count(m => m.state == animationState && m.direction == currentDir);
                currentFrame = (currentFrame + 1) % maxFrame;
            }
            else if (animationState == AnimationState.Attack || animationState == AnimationState.Dead)
            {
                currentFrame++;

                int maxFrame = mappings.Count(m => m.state == animationState && m.direction == currentDir);

                if (currentFrame >= maxFrame)
                {
                    currentFrame = maxFrame - 1;
                    ai.engageState = OutleaderAI.EngageState.MoveToRandLoc;
                }
            }
        }
    }

    public void SetState(AnimationState state)
    {
        if (animationState != state)
        {
            animationState = state;
            currentFrame = 0;
        }
    }

    private List<SpriteMapping> GenerateMappings(Sprite[] sprites, int idleFrames, int walkFrames, int attackFrames, int deadFrames)
    {
        var mappings = new List<SpriteMapping>();
        int index = 0;

        void AddStateMappings(AnimationState state, int frames)
        {
            for (int frame = 0; frame < frames; frame++)
            {
                foreach (EnemyDir dir in System.Enum.GetValues(typeof(EnemyDir)))
                {
                    mappings.Add(new SpriteMapping
                    {
                        index = index++,
                        state = state,
                        direction = dir,
                        frame = frame
                    });
                }
            }
        }

        AddStateMappings(AnimationState.Walk, walkFrames);
        AddStateMappings(AnimationState.Attack, attackFrames);
        AddStateMappings(AnimationState.Idle, idleFrames);
        int deathFrame0 = index;
        foreach (EnemyDir dir in System.Enum.GetValues(typeof(EnemyDir)))
        {
            for (int frame = 0; frame < deadFrames; frame++)
            {
                mappings.Add(new SpriteMapping
                {
                    index = deathFrame0++,
                    state = AnimationState.Dead,
                    direction = dir,
                    frame = frame,
                });
            }
            deathFrame0 = index;
        }
        // Debug.Log("Generated " + mappings.Count + " sprite mappings.");
        return mappings;
    }

    private EnemyDir AngleToDirection(float angle)
    {
        if (angle >= -22.5f && angle < 22.5f)
            return EnemyDir.Back;
        else if (angle >= 22.5f && angle < 67.5f)
            return EnemyDir.BackRight;
        else if (angle >= 67.5f && angle < 112.5f)
            return EnemyDir.Right;
        else if (angle >= 112.5f && angle < 157.5f)
            return EnemyDir.FrontRight;
        else if (angle >= 157.5f || angle < -157.5f)
            return EnemyDir.Front;
        else if (angle >= -157.5f && angle < -112.5f)
            return EnemyDir.FrontLeft;
        else if (angle >= -112.5f && angle < -67.5f)
            return EnemyDir.Left;
        else
            return EnemyDir.BackLeft; // -67.5 ~ -22.5
    }
}
