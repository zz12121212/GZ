using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class GlobalRewindManager : MonoBehaviour
{
    public static GlobalRewindManager Instance { get; private set; }// 单例模式，方便全局访问

    [Header("回溯参数")]
    public float rewindDuration = 3.0f;// 回溯持续时间
    public float rewindSpeed = 1.0f;// 回溯速度

    [Header("调试")]
    public bool isRewinding = false;// 是否正在回溯

    private List<RewindableEntity> allEntities = new List<RewindableEntity>();// 场景中所有可回溯实体的列表
    private Coroutine rewindCoroutine;// 回溯协程的引用

    void Awake()
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
        EventBus.registerEvent(EventType.TimeRewindStart, OnTimeRewindStart);
        EventBus.registerEvent(EventType.TimeRewindEnd, OnTimeRewindEnd);
    }

    public void OnTimeRewindStart()
    {

    }
    public void OnTimeRewindEnd()
    {
    }
}