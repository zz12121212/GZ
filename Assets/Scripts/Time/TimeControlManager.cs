// TimeControlManager.cs
using UnityEngine;
using System.Collections;

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

    private bool debugMode = false;// 是否启用调试日志

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
        EventBus.registerEvent(EventType.TimeSlowStart, OnTimeSlowStart);
        EventBus.registerEvent(EventType.TimeRewindStart, OnTimeRewindStart);
    }
    void OnDestroy()
    {
        // 注销时间事件监听器，防止内存泄漏
        EventBus.disRegisterEvent(EventType.TimeFastForwardStart, OnTimeFastForwardStart);
        EventBus.disRegisterEvent(EventType.TimeSlowStart, OnTimeSlowStart);
        EventBus.disRegisterEvent(EventType.TimeRewindStart, OnTimeRewindStart);
    }

    // 时间控制事件处理方法
    void OnTimeFastForwardStart()
    {
        Time.timeScale = fastForwardScale;
        if (debugMode) Debug.Log("时间快进开始，当前timeScale: " + Time.timeScale);

        if (gameObject.activeSelf) // 确保对象处于激活状态
        {
            StopCoroutine(nameof(FastForwardRoutine)); // 停止之前的协程，防止叠加
        }
        
        StartCoroutine(nameof(FastForwardRoutine)); // 启动快进持续时间的协程
    }
    private IEnumerator FastForwardRoutine()
    {
        yield return new WaitForSecondsRealtime(skillDuration); // 等待技能持续时间（使用实时等待，忽略timeScale）
        OnTimeFastForwardEnd(); // 快进结束
    }
    void OnTimeFastForwardEnd()
    {
        Time.timeScale = defaultTimeScale;
        if (debugMode) Debug.Log("时间快进结束，当前timeScale: " + Time.timeScale);
        EventBus.publish(EventType.TimeFastForwardEnd); 
    }
    void OnTimeSlowStart()
    {
        Time.timeScale = slowScale;
        if (debugMode) Debug.Log("时间减速开始，当前timeScale: " + Time.timeScale);

        if(gameObject.activeSelf) // 确保对象处于激活状态
        {
            StopCoroutine(nameof(SlowRoutine)); // 停止之前的协程，防止叠加
        }
        StartCoroutine(nameof(SlowRoutine)); // 启动减速持续时间的协程
    }
    private IEnumerator SlowRoutine()
    {
        yield return new WaitForSecondsRealtime(skillDuration); // 等待技能持续时间（使用实时等待，忽略timeScale）
        OnTimeSlowEnd(); // 减速结束
    }
    void OnTimeSlowEnd()
    {
        Time.timeScale = defaultTimeScale;
        if (debugMode) Debug.Log("时间减速结束，当前timeScale: " + Time.timeScale);
        EventBus.publish(EventType.TimeSlowEnd);
    }
    void OnTimeRewindStart()
    {
        
    }
    void OnTimeRewindEnd()
    {
        
    }

}