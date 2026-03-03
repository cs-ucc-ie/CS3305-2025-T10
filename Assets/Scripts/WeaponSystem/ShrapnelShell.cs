using UnityEngine;

public class SplitBullet : BulletFramework
{
    [Header("Split Settings")]
    [SerializeField] protected BulletFramework childBulletPrefab;  // 小子弹Prefab（必须挂具体子弹脚本，如 NormalBullet）
    [SerializeField] private int childCount = 6;                 // 分裂数量
    [SerializeField] private float splitDelay = 0.1f;            // 发射后多久分裂（秒）
    [SerializeField] private float splitAngle = 15f;             // 分裂散射角（度）
    [SerializeField] private float childSpeed = 35f;             // 小子弹速度
    [SerializeField] private float childLifeTime = 1.5f;         // 小子弹存活时间
    [SerializeField] private float childDamageMultiplier = 0.6f; // 小子弹伤害=damage*倍率

    [Header("Behavior")]
    [SerializeField] private bool destroyParentOnSplit = true;   // 分裂后是否销毁母弹
    [SerializeField] private bool childInheritParentDirection = true; // 用母弹当前飞行方向作为基准（更稳定）

    private bool hasSplit = false;

    protected override void OnEnable()
    {
        base.OnEnable();


        if (rb != null) rb.useGravity = false;

        hasSplit = false;


        CancelInvoke(nameof(DoSplit));
        Invoke(nameof(DoSplit), splitDelay);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        CancelInvoke(nameof(DoSplit));
    }

    public override void Launch(Vector3 velocity)
    {
        // 直线飞行
        rb.linearVelocity = velocity;
        rb.useGravity = false;
    }
    
protected override void OnHit(Collision collision)
    {
       
    }



private void DoSplit()
{
    if (hasSplit) return;
    hasSplit = true;

    if (childBulletPrefab == null || childCount <= 0)
    {
        if (destroyParentOnSplit) Kill();
        return;
    }

    Vector3 origin = transform.position;

    Vector3 baseDir = transform.forward;
    if (childInheritParentDirection && rb != null && rb.linearVelocity.sqrMagnitude > 0.001f)
        baseDir = rb.linearVelocity.normalized;

    Quaternion baseRot = Quaternion.LookRotation(baseDir, Vector3.up);

    float childDamage = damage * childDamageMultiplier;

    var spawnedColliders = new System.Collections.Generic.List<Collider>();

    for (int i = 0; i < childCount; i++)
    {
        float yaw = Random.Range(-splitAngle, splitAngle);
        float pitch = Random.Range(-splitAngle, splitAngle);
        Quaternion spreadRot = baseRot * Quaternion.Euler(pitch, yaw, 0f);

        Vector3 spawnPos = origin + Random.insideUnitSphere * 0.03f;
        spawnPos.y = origin.y;

        BulletFramework child = Instantiate(childBulletPrefab, spawnPos, spreadRot);

        var childCol = child.GetComponent<Collider>();
        if (childCol != null)
        {
            for (int j = 0; j < spawnedColliders.Count; j++)
            {
                if (spawnedColliders[j] != null)
                    Physics.IgnoreCollision(childCol, spawnedColliders[j], true);
            }
            spawnedColliders.Add(childCol);
        }

        child.Init(childLifeTime, childDamage);
        child.Launch(child.transform.forward * childSpeed);

        var childRb = child.GetComponent<Rigidbody>();
        if (childRb != null) childRb.useGravity = false;
    }

    if (destroyParentOnSplit) Kill();
}
}