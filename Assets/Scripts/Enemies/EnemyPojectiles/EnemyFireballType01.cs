using NUnit.Framework;
using UnityEngine;

//TODO 增加光源效果？
public class EnemyFireballType01 : MonoBehaviour
{
    [SerializeField] private Sprite[] fireballSprites;
    [SerializeField] private float fireballSpeed = 10f;
    [SerializeField] private float ttl = 20f;
    [SerializeField] private float fps = 6f;
    [SerializeField] private int damage = 10;
    private Camera mainCam;
    private SpriteRenderer sr;
    private int currentFrame = 0;
    private float spriteTimer = 0f;
    private float ttlTimer;
    

    void Start()
    {
        mainCam = Camera.main;
        ttlTimer = ttl;
        sr = GetComponentInChildren<SpriteRenderer>();
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * fireballSpeed;
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
        
        // // 忽略自己 / 其他子弹
        // if (HasParentWithTag(other.transform, "Enemy") || HasParentWithTag(other.transform, "EnemyProjectile"))
        //     return;

        if (HasParentWithTag(other.transform, "EnemyProjectile"))
        {
            Debug.Log("Fireball hit enemy projectile: " + other.name);
            return;
        }
            

        if(HasParentWithTag(other.transform, "Enemy"))
        {
            Debug.Log("Fireball hit enemy: " + other.name);
            EnemyAI enemyAI = other.GetComponentInParent<EnemyAI>();
            if(enemyAI != null)            {
                enemyAI.TakeDamage(damage); 
                //Destroy(gameObject);
                return;

        }}

        // 命中玩家
        if(other.CompareTag("Player"))
        {
            Debug.Log("Fireball hit player: " + other.name);
            PlayerStatsManager.Instance.TakeDamage(damage);
        }

        Debug.Log("Fireball hit: " + other.name);
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
