using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*说明：
 * 0.该脚本为AI输出，人为调试，挂载在DogMonster上，初始默认不激活
 * 1.WaveController挂载在敌人（如狗）身上，用于周期性向四周发射冲击波
 * 2.spawnPoint设为敌人子物体，位置置于敌人中心，作为波的发射原点
 * 3.冲击波预制体（projectilePrefab）默认朝向左边（180°），脚本已自动校正旋转方向
 * 4.每次发射数量、速度、最大飞行距离、销毁Tag均可在Inspector中配置
 * 5.冲击波通过添加ProjectileBehavior组件实现移动与碰撞销毁逻辑
 * 6.需要在对应物体上设置Player、Ground、Wall的tag
 * 7.DogMonster上的圆形碰撞器的Y偏移 = 波开始点的Y，圆形碰撞器的半径 = 脚本中的最大距离
 * 8.DogMonster上的圆形碰撞器设置为触发器
 */

public class WaveController : MonoBehaviour
{
    [Header("发射设置")]
    [Tooltip("冲击波预制体（素材默认朝左）")]
    public GameObject projectilePrefab;          // 冲击波预制体
    [Tooltip("波产生的起点（建议为子空物体）")]
    public Transform spawnPoint;                 // 波产生的起点
    [Tooltip("发射间隔时间（秒）")]
    public float interval = 2f;                  // 发射间隔（秒）
    [Tooltip("每次发射的波数量")]
    public int projectileCount = 12;             // 每次发射数量
    [Tooltip("波的初始飞行速度")]
    public float speed = 5f;                     // 初始速度
    [Tooltip("波的最大飞行距离")]
    public float maxDistance = 8f;               // 最大飞行距离

    [Header("销毁条件")]
    [Tooltip("碰到这些Tag的物体时，波会立即销毁")]
    public string[] validDestroyTags = { "Player", "Ground", "Wall" };

    private Coroutine emissionRoutine;           // 发射协程引用

    void Start()
    {
        // 初始化引用（仅一次）
        if (spawnPoint == null)
            spawnPoint = transform;

        if (projectilePrefab == null)
        {
            Debug.LogError("WaveController: projectilePrefab is not assigned!");
            return;
        }
        // 注意：不再在这里 StartEmission()
    }

    void OnEnable()
    {
        // 每次启用脚本时（包括首次和重新勾选），启动发射
        StartEmission();
    }

    void OnDisable()
    {
        // 每次禁用脚本时，停止发射
        StopEmission();
    }

    /* ----- 方法：开始发射波 ----- */
    public void StartEmission()
    {
        // 防止重复启动协程
        if (emissionRoutine != null)
            StopCoroutine(emissionRoutine);
        emissionRoutine = StartCoroutine(EmitWaves());
    }

    /* ----- 方法：停止发射波 ----- */
    public void StopEmission()
    {
        if (emissionRoutine != null)
        {
            StopCoroutine(emissionRoutine);
            emissionRoutine = null;
        }
    }

    /* ----- 协程：周期性发射波 ----- */
    IEnumerator EmitWaves()
    {
        while (true)
        {
            EmitOneWave();                      // 发射一波
            yield return new WaitForSeconds(interval); // 等待间隔时间
        }
    }

    /* ----- 方法：发射单次波阵 ----- */
    void EmitOneWave()
    {
        Vector3 spawnPos = spawnPoint.position;

        // 均匀分配角度，实现360°环绕发射
        for (int i = 0; i < projectileCount; i++)
        {
            float angle = i * 360f / projectileCount; // 当前分段角度（0~360）
            Vector2 direction = GetDirectionFromAngle(angle);

            // 关键：因素材朝左（180°），需将旋转角度 +180° 使其正面朝向飞行方向
            float rotationZ = angle + 180f;
            Quaternion rotation = Quaternion.Euler(0, 0, rotationZ);

            // 实例化冲击波并设置初始朝向
            GameObject proj = Instantiate(projectilePrefab, spawnPos, rotation);

            // 动态添加行为组件并初始化参数
            ProjectileBehavior pb = proj.AddComponent<ProjectileBehavior>();
            pb.Initialize(direction, speed, maxDistance, spawnPos, validDestroyTags);
        }
    }

    /* ----- 方法：根据角度获取单位方向向量 ----- */
    Vector2 GetDirectionFromAngle(float angleDegrees)
    {
        float rad = angleDegrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    /* ----- 方法：选中时绘制最大飞行范围 ----- */
    void OnDrawGizmosSelected()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(spawnPoint.position, maxDistance);
        }
    }
}

// 子组件：每个冲击波的独立行为逻辑
/*说明：
 * 0.该组件由WaveController动态添加，无需手动挂载到预制体
 * 1.负责控制冲击波的移动、距离限制与碰撞销毁
 * 2.同时监听OnTriggerEnter2D与OnCollisionEnter2D，确保兼容不同类型的障碍物
 * 3.使用MovePosition（Kinematic）方式移动，避免物理干扰
 */
public class ProjectileBehavior : MonoBehaviour
{
    private Vector2 direction;                   // 飞行方向（单位向量）
    private float speed;                         // 飞行速度
    private float maxDistance;                   // 最大飞行距离
    private Vector3 spawnPosition;               // 发射原点（用于距离判断）
    private string[] destroyTags;                // 触发销毁的有效Tag列表
    private Rigidbody2D rb;                      // 刚体引用

    /* ----- 方法：初始化冲击波参数 ----- */
    public void Initialize(Vector2 dir, float spd, float maxDist, Vector3 spawnPos, string[] tags)
    {
        direction = dir.normalized;
        speed = spd;
        maxDistance = maxDist;
        spawnPosition = spawnPos;
        destroyTags = tags;

        // 获取或添加Rigidbody2D，设为Kinematic以精确控制位置
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.isKinematic = true;
        }

        // 启动移动与检测协程
        StartCoroutine(MoveAndCheck());
    }

    /* ----- 协程：持续移动并检测销毁条件 ----- */
    IEnumerator MoveAndCheck()
    {
        float traveled = 0f;
        float fixedDeltaTime = Time.fixedDeltaTime;

        // 持续飞行直到达到最大距离
        while (traveled < maxDistance)
        {
            Vector2 moveStep = direction * speed * fixedDeltaTime;
            if (rb.isKinematic)
            {
                rb.MovePosition(rb.position + moveStep); // 精确移动
            }
            else
            {
                rb.velocity = direction * speed;        // 备用方案（非推荐）
            }

            traveled += moveStep.magnitude;

            // 额外安全检查：防止因浮点误差导致超距
            if (Vector3.Distance(transform.position, spawnPosition) >= maxDistance)
                break;

            yield return new WaitForFixedUpdate();
        }

        // 飞行结束，销毁自身
        Destroy(gameObject);
    }

    /* ----- 方法：触发器碰撞检测 ----- */
    void OnTriggerEnter2D(Collider2D other)
    {
        CheckAndDestroy(other.gameObject);
    }

    /* ----- 方法：实体碰撞检测 ----- */
    void OnCollisionEnter2D(Collision2D collision)
    {
        CheckAndDestroy(collision.gameObject);
    }

    /* ----- 方法：检查是否应销毁 ----- */
    void CheckAndDestroy(GameObject other)
    {
        // 若目标Tag在有效销毁列表中，则立即销毁
        if (destroyTags != null)
        {
            foreach (string tag in destroyTags)
            {
                if (other.CompareTag(tag))
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }
    }
}