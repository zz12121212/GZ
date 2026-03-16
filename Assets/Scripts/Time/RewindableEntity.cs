// RewindableEntity.cs
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct EntityState
{
    public Vector3 position;// 实体位置
    public Quaternion rotation;// 实体旋转
    public Vector3 velocity;// 实体速度
    public Vector3 angularVelocity;// 实体角速度
}

public class RewindableEntity: MonoBehaviour
{
    [Header("设置")]
    public bool restoreVelocity = true;// 回溯时是否恢复速度
    public int maxFrames = 150;// 最大记录帧数，约等于3秒

    private Rigidbody2D rb;
    private Animator animator;

    private List<EntityState> history = new List<EntityState>();// 状态缓冲区

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // 方便外部查询当前历史记录数量
    public int GetHistoryCount()
    {
        return history.Count;
    }

    // 注册到全局管理器，确保在回溯过程中能被正确处理
    private void OnEnable()
    {
        if (GlobalRewindManager.Instance != null)
        {
            GlobalRewindManager.Instance.RegisterEntity(this);
        }

        EventBus.registerEvent(EventType.TimeRewindStart, OnTimeRewindStart);
        EventBus.registerEvent(EventType.TimeRewindEnd, OnTimeRewindEnd);
    }
    // 注销时从全局管理器移除，避免回溯过程中访问已销毁的实体
    private void OnDisable()
    {
        if (GlobalRewindManager.Instance != null)
        {
            GlobalRewindManager.Instance.UnregisterEntity(this);
        }

        EventBus.disRegisterEvent(EventType.TimeRewindStart, OnTimeRewindStart);
        EventBus.disRegisterEvent(EventType.TimeRewindEnd, OnTimeRewindEnd);
    }

    // 回溯开始和结束的事件处理函数
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
        EntityState state = new EntityState
        {
            position = transform.position,
            rotation = transform.rotation
        };

        // 需要恢复速度时记录速度相关信息，节省内存
        if (restoreVelocity)
        {
            state.velocity = new Vector3(rb.velocity.x, rb.velocity.y, 0);
            state.angularVelocity = new Vector3(0, 0, rb.angularVelocity);
        }

        history.Add(state);

        // 超出最大帧数时丢弃最旧的状态
        if (history.Count > maxFrames)
        {
            history.RemoveAt(0);
        }
    }

    // 回溯到指定历史状态，offset为0表示回到上一个状态，1表示上上个状态，以此类推
    public bool TryGetStateAtOffSet(int offset, out EntityState state)
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

    // 应用历史状态到实体
    public void ApplyState(EntityState state)
    {
        transform.position = state.position;
        transform.rotation = state.rotation;
        // 如果有刚体，必须同步刚体状态，否则物理引擎会把位置拉回去
        if (restoreVelocity)
        {
            if(rb != null)
            {
                rb.velocity = new Vector2(state.velocity.x, state.velocity.y);
                rb.angularVelocity = state.angularVelocity.z;
                // 直接设置位置，避免物理引擎干扰
                rb.MovePosition(state.position);
                rb.WakeUp();
            }
        }
    }

    public void SetRewindMode(bool isRewinding)
    {
        if (animator != null)
        {
            animator.enabled = !isRewinding;// 回溯时禁用动画，避免动画干扰位置
        }
    }

    public void ClearHistory()
    {
        history.Clear();
    }

}