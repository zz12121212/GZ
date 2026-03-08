// SkillManager.cs
using UnityEngine;

/// <summary>
///  技能管理器，负责监听玩家输入并发布相应的时间控制事件。
/// </summary>
public class SkillManager : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            EventBus.publish(EventType.TimeFastForwardStart);
            Debug.Log("E技能触发：时间快进,广播已发出");
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            EventBus.publish(EventType.TimeSlowStart);
            Debug.Log("Q技能触发：时间慢速,广播已发出");
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            EventBus.publish(EventType.TimeRewindStart);
            Debug.Log("R技能触发：时间倒流,广播已发出");
        }
    }
}
