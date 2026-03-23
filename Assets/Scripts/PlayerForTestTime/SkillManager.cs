// SkillManager.cs
using UnityEngine;

/// <summary>
///  技能管理器，负责监听玩家输入并发布相应的时间控制事件。
///  编写互斥锁机制，防止技能叠加
/// </summary>
public class SkillManager : MonoBehaviour
{
    private bool isSkillActive = false;

    void Start()
    {
        EventBus.registerEvent(EventType.TimeFastForwardEnd, OnTimeFastForwardEnd);
        EventBus.registerEvent(EventType.TimeSlowEnd, OnTimeSlowEnd);
        EventBus.registerEvent(EventType.TimeRewindEnd, OnTimeRewindEnd);
    }

    void OnDestroy()
    {
        EventBus.disRegisterEvent(EventType.TimeFastForwardEnd, OnTimeFastForwardEnd);
        EventBus.disRegisterEvent(EventType.TimeSlowEnd, OnTimeSlowEnd);
        EventBus.disRegisterEvent(EventType.TimeRewindEnd, OnTimeRewindEnd);
    }

    private void Update()
    {
        if(isSkillActive)
        {
            return; // 如果技能正在进行中，忽略新的输入
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            isSkillActive = true;
            EventBus.publish(EventType.TimeFastForwardStart);
            Debug.Log("E技能触发：时间快进,广播已发出");
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            isSkillActive = true;
            EventBus.publish(EventType.TimeSlowStart);
            Debug.Log("Q技能触发：时间慢速,广播已发出");
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            isSkillActive = true;
            EventBus.publish(EventType.TimeRewindStart);
            Debug.Log("R技能触发：时间倒流,广播已发出");
        }
    }

    private void OnTimeFastForwardEnd()
    {
        Debug.Log("时间快进结束，技能状态重置");
        isSkillActive = false;
    }
    private void OnTimeSlowEnd()
    {
        Debug.Log("时间慢速结束，技能状态重置");
        isSkillActive = false;
    }
    private void OnTimeRewindEnd()
    {
        Debug.Log("时间倒流结束，技能状态重置");
        isSkillActive = false;
    }
}
