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
    private bool debugMode = false;// 是否启用调试日志

    void Start()
    {
        // 自动获取组件，如果未设置
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
            if (debugMode) Debug.Log("Rigidbody2D组件未设置，已自动获取。");
        }
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (debugMode) Debug.Log("Animator组件未设置，已自动获取。");
        }
        if (rb != null)
        {
            rb.freezeRotation = true;
            if (debugMode) Debug.Log("Rigidbody2D组件已设置，冻结旋转。");
        }

        // 注册时间事件监听器
        EventBus.registerEvent(EventType.TimeFastForwardStart, OnTimeFastForwardStart);
        EventBus.registerEvent(EventType.TimeFastForwardEnd, OnTimeFastForwardEnd);
        EventBus.registerEvent(EventType.TimeSlowStart, OnTimeSlowStart);
        EventBus.registerEvent(EventType.TimeSlowEnd, OnTimeSlowEnd);
        EventBus.registerEvent(EventType.TimeRewindStart, OnTimeRewindStart);
        EventBus.registerEvent(EventType.TimeRewindEnd, OnTimeRewindEnd);
    }

    void OnDestroy()
    {
        // 注销时间事件监听器，防止内存泄漏
        EventBus.disRegisterEvent(EventType.TimeFastForwardStart, OnTimeFastForwardStart);
        EventBus.disRegisterEvent(EventType.TimeFastForwardEnd, OnTimeFastForwardEnd);
        EventBus.disRegisterEvent(EventType.TimeSlowStart, OnTimeSlowStart);
        EventBus.disRegisterEvent(EventType.TimeSlowEnd, OnTimeSlowEnd);
        EventBus.disRegisterEvent(EventType.TimeRewindStart, OnTimeRewindStart);
        EventBus.disRegisterEvent(EventType.TimeRewindEnd, OnTimeRewindEnd);
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
            if (debugMode) Debug.Log($"TimeScale: {Time.timeScale}, AnimatorSpeed: {animator.speed}");
        }
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            float timeScaleFactor = Time.timeScale > 0.0001f ? 1f / Time.timeScale : 1f;

            // 位移 = 输入 * 速度 * 固定时间步长 / 时间缩放因子
            Vector2 movement = moveInput * moveSpeed * Time.fixedDeltaTime / Time.timeScale;
            Vector2 targetPosition = rb.position + movement;

            rb.MovePosition(targetPosition);
        }
    }

    // 时间事件处理函数
    void OnTimeFastForwardStart()
    {
        animator.SetTrigger("AttackE");
        if (debugMode) Debug.Log("时间快进开始，玩家触发E技能动画");
    }
    void OnTimeFastForwardEnd()
    {
        
    }
    void OnTimeSlowStart()
    {
        animator.SetTrigger("AttackQ");
        if (debugMode) Debug.Log("时间慢速开始，玩家触发Q技能动画");
    }
    void OnTimeSlowEnd()
    {
        
    }
    void OnTimeRewindStart()
    {
        animator.SetTrigger("AttackR");
        if (debugMode) Debug.Log("时间倒流开始，玩家触发R技能动画");
    }
    void OnTimeRewindEnd()
    {
        
    }
}