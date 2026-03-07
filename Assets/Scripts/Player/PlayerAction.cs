// PlayerAction.cs
using UnityEngine;

/// <summary>
/// 玩家行为控制脚本，负责处理玩家的移动、攻击等行为。
/// 挂载在玩家角色的GameObject上，使用Rigidbody2D组件进行物理移动。
/// </summary>
public class PlayerAction : MonoBehaviour
{
    [Header("组件引用")]
    public Rigidbody2D rb;
    public Animator animator;

    [Header("移动参数")]
    public float moveSpeed = 5f;

    private Vector2 moveInput;// 玩家输入的移动方向

    void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
            Debug.Log("Rigidbody2D组件未设置，已自动获取。");
        }
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            Debug.Log("Animator组件未设置，已自动获取。");
        }

        if (rb != null)
        {
            rb.freezeRotation = true;
            Debug.Log("Rigidbody2D组件已设置，冻结旋转。");
        }
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");

        moveInput = new Vector2(moveX, 0);

        // 更新动画参数
        if (animator != null)
        {
            animator.SetFloat("Speed", moveInput.magnitude);

            // 抵消time.timeScale对动画的影响,即使得玩家动画保持正常速度
            if (Time.timeScale > 0.0001)
            {
                animator.speed = 1f / Time.timeScale;
            }
            else
            {
                animator.speed = 1f;
            }
        }
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            float timeScaleFactor = Time.timeScale > 0.0001f ? 1f / Time.timeScale : 1f;

            // 位移 = 输入 * 速度 * (固定时间步长 * 时间缩放因子)
            Vector2 movement = moveInput * moveSpeed * (Time.fixedDeltaTime * Time.timeScale);
            Vector2 targetPosition = rb.position + movement;

            rb.MovePosition(targetPosition);
        }
    }
}