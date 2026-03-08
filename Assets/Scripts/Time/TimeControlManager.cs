// TimeControlManager.cs
using UnityEngine;

/// <summary>
///  时间控制管理器，负责处理时间快进、减速和回溯的逻辑。
/// </summary>
public class TimeControlManager : MonoBehaviour
{
    public static TimeControlManager Instance { get; private set; }// 单例模式，方便全局访问

    [Header("时间控制参数")]
    [Range(0.1f, 1.0f)] public float slowScale = 0.5f; // 减速比例
    [Range(1.0f, 5.0f)] public float fastForwardScale = 2.0f; // 快进比例
    public float defaultTimeScale = 1.0f; // 默认时间比例

    [Header("技能持续时间")]
    public float skillDuration = 3.0f;

    private void Awake()
    {
        // 实现单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 切换场景时不销毁
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
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

    // 时间控制事件处理方法
    void OnTimeFastForwardStart()
    {
        Time.timeScale = fastForwardScale;
        Debug.Log("时间快进开始，当前timeScale: " + Time.timeScale);
        Invoke(nameof(OnTimeFastForwardEnd), skillDuration);
    }
    void OnTimeFastForwardEnd()
    {
        Time.timeScale = defaultTimeScale;
        Debug.Log("时间快进结束，当前timeScale: " + Time.timeScale);
    }
    void OnTimeSlowStart()
    {
        Time.timeScale = slowScale;
        Debug.Log("时间减速开始，当前timeScale: " + Time.timeScale);
        Invoke(nameof(OnTimeSlowEnd), skillDuration);
    }
    void OnTimeSlowEnd()
    {
        Time.timeScale = defaultTimeScale;
        Debug.Log("时间减速结束，当前timeScale: " + Time.timeScale);
    }
    void OnTimeRewindStart()
    {
        
    }
    void OnTimeRewindEnd()
    {
        
    }

}