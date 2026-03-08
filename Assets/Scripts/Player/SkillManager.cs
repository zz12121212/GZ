// SkillManager.cs
using UnityEngine;

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
