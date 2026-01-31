using NUnit.Framework;
using UnityEngine;

public class OutleaderFireball : MonoBehaviour
{
    private Camera mainCam;
    public Sprite[] fireballSprites;
    public SpriteRenderer sr;
    private int currentFrame = 0;
    private float spriteTimer = 0f;
    private float ttlTimer = 20f;
    public float fps = 6f;
    public int damage = 9;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        // 火球渲染器始终面向摄像机
        if (mainCam != null)
        {
            Vector3 lookDir = mainCam.transform.position - transform.position;
            lookDir.y = 0f; 
            if (lookDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookDir);
        }

        // 循环播放火焰动画
        spriteTimer += Time.deltaTime;
        if (spriteTimer > 1f / fps)
        {
            spriteTimer = 0f;
            currentFrame = (currentFrame + 1) % fireballSprites.Length;
            sr.sprite = fireballSprites[currentFrame];
        }

        // 足够长时间后销毁
        ttlTimer -= Time.deltaTime;
        if (ttlTimer <= 0f)
        {
            Destroy(gameObject);
            return;
        }
        
    }

    // 碰到墙壁或玩家后销毁
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Fireball hit: " + other.name);
        
        // 忽略自己 / 其他子弹
        if (HasParentWithTag(other.transform, "Enemy") || HasParentWithTag(other.transform, "EnemyProjectile"))
            return;

        // 命中玩家
        if(other.CompareTag("Player"))
        {
            PlayerStatsManager.Instance.TakeDamage(damage);
        }
        Destroy(gameObject);
    }

private bool HasParentWithTag(Transform start, string tag)
{
    Transform t = start;
    while (t != null)
    {
        if (t.CompareTag(tag))
            return true;
        t = t.parent;
    }
    return false;
}

}
