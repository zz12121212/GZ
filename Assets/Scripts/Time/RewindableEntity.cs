// RewindableEntity.cs
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct EntityState
{
    public Vector2 position;// 实体位置
    public float rotation;// 实体旋转
    public Vector2 velocity;// 实体速度
    public float angularVelocity;// 实体角速度


}

public class RewindableEntity: MonoBehaviour
{
    [Header("设置")]
    public bool restoreVelocity = true;// 回溯时是否恢复速度
    public int maxFrames = 150;// 最大记录帧数，约等于3秒

    private Rigidbody2D rb;
    private Animator animator; 

    private List<EntityState> history;// 状态历史缓冲区
    private bool isRegistered = false;// 是否已注册到全局管理器

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        history = new List<EntityState>(maxFrames);
    }

    public int GetHistoryCount() => history.Count;

    private void OnEnable()
    {
        TryRegister();// 注册到全局管理器

        EventBus.registerEvent(EventType.TimeRewindStart, OnTimeRewindStart);
        EventBus.registerEvent(EventType.TimeRewindEnd, OnTimeRewindEnd);
    }

    private void OnDisable()
    {
        GlobalRewindManager.Instance?.UnregisterEntity(this);// 注销
        EventBus.disRegisterEvent(EventType.TimeRewindStart, OnTimeRewindStart);
        EventBus.disRegisterEvent(EventType.TimeRewindEnd, OnTimeRewindEnd);
    }

    private void OnDestroy()
    {
        if (GlobalRewindManager.Instance != null)
        {
            GlobalRewindManager.Instance.UnregisterEntity(this);// 注销
        }
    }

    private void Start()
    {
        if (!isRegistered)
        {
            TryRegister();
        }
    }

    private void TryRegister()
    {
        if (GlobalRewindManager.Instance != null)
        {
            GlobalRewindManager.Instance.RegisterEntity(this);
            isRegistered = true;
        }
    }

    private void OnTimeRewindStart()
    {
        SetRewindMode(true);
    }
    private void OnTimeRewindEnd()
    {
        SetRewindMode(false);
    }

    // 记录当前状态到历史缓冲区，应该在FixedUpdate中调用，确保物理状态同步
    public void RecordState()
    {
        EntityState state;

        if (restoreVelocity && rb != null)
        {
            state = new EntityState
            {
                position = rb.position, // 直接使用刚体位置，避免 Transform 和 Rigibody 不同步
                rotation = rb.rotation,
                velocity = rb.velocity,
                angularVelocity = rb.angularVelocity
            };
        }
        else
        {
            state = new EntityState
            {
                position = rb ? rb.position : transform.position,
                rotation = rb ? rb.rotation : transform.eulerAngles.z,
                velocity = Vector2.zero,
                angularVelocity = 0
            };
        }

        history.Add(state);

        if (history.Count > maxFrames)
        {
            history.RemoveAt(0);
        }
    }

    public bool TryGetStateAtOffset(int offset, out EntityState state)
    {
        int index = history.Count - 1 - offset;
        if (index >= 0 && index < history.Count)
        {
            state = history[index];
            return true;
        }
        state = default;
        return false;
    }

    public void ApplyState(EntityState state)
    {
        if (rb != null)
        {
            // 直接设置刚体位置和旋转
            rb.position = state.position;
            rb.rotation = state.rotation;

            // 设置速度
            if (restoreVelocity)
            {
                rb.velocity = state.velocity;
                rb.angularVelocity = state.angularVelocity;
            }
            else
            {
                // 不回溯速度时，必须清零，防止残留速度导致漂移
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            // 唤醒刚体 (确保它不是 Sleep 状态)
            rb.WakeUp();

            // 注意：这里不需要手动设置 transform.position/rotation
            // 因为 rb.position 赋值后，Unity 会自动同步到 transform (除非 rb.simulated=false 时同步有延迟，但在下一帧渲染前通常会同步)
            // 为了视觉绝对同步，可以强制刷一次 transform，但通常不需要
            transform.position = state.position;
            transform.rotation = Quaternion.Euler(0, 0, state.rotation);
        }
        else
        {
            // 如果没有刚体，直接操作 Transform
            transform.position = state.position;
            transform.rotation = Quaternion.Euler(0, 0, state.rotation);
        }
    }

    public void SetRewindMode(bool isRewinding)
    {

        if (animator != null)
            animator.enabled = !isRewinding;

        if (rb != null)
        {
            if (isRewinding)
            {
                // 完全停止物理模拟，刚体变成静态，不受重力、碰撞影响
                rb.simulated = false;

            }
            else
            {
                // 回溯结束，恢复物理模拟
                rb.simulated = true;
                rb.WakeUp();
            }
        }
    }

    // 提供给外部清理
    public void ClearHistory() => history.Clear();

}