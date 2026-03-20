// GlobalRewindManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public enum RewindPhase
{
    None,
    RewindBack,// 倒带阶段
    RewindForward// 复位阶段
}

public class GlobalRewindManager : MonoBehaviour
{
    public static GlobalRewindManager Instance { get; private set; }

    [Header("倒带参数")]
    public float rewindBackDuration = 3.0f;// 倒带阶段持续时间
    public float rewindBackSpeed = 0.7f;// 倒带速度倍率
    [Tooltip("倒带结束后停顿时间")]
    public float postRewindPause = 0.5f;// 倒带结束后停顿时间

    [Header("复位参数")]
    [Tooltip("复位阶段持续时间，默认为0表示自动计算为刚好回到初始点所需时间")]
    public float rewindForwardDuration = 0f;// 复位阶段持续时间
    public float rewindForwardSpeed = 1.0f;// 复位速度倍率

    [Header("调试选项")]
    public bool debugMode = true;// 是否启用调试日志
    public bool isRewinding = false;// 当前是否处于倒带状态
    public RewindPhase currentPhase = RewindPhase.None;// 当前回溯阶段

    private List<RewindableEntity> allEntities = new List<RewindableEntity>();// 所有可回溯实体的列表
    private Coroutine rewindCoroutine;// 当前正在运行的倒带协程

    // 记录回溯开始时的状态基准
    private int backTargetOffset = 0;// 倒带阶段需要回溯的帧数
    private int startHistoryDepth = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        EventBus.registerEvent(EventType.TimeRewindStart, OnTimeRewindStart);

    }

    private void FixedUpdate()
    {
        if (isRewinding || Instance == null )
        {
            return;
        }

        foreach (var entity in allEntities )
        {
            if(entity != null)
            {
                entity.RecordState();
            }
        }
    }

    public void OnTimeRewindStart()
    {
        StartRewind();
    }

    public void StartRewind()
    {
        if (isRewinding)
        {
            if (debugMode) Debug.LogWarning("已经在倒带中，无法再次启动倒带。");
            return;
        }
        if (allEntities.Count == 0)
        {
            if (debugMode) Debug.LogWarning("没有可回溯的实体，无法启动倒带。");
            return;
        }

        // 获取所有实体中最短的历史深度
        int minDepth = GetMinHistoryDepth();
        if(minDepth <= 0)
        {
            if (debugMode) Debug.LogWarning("所有实体的历史深度都不足，无法启动倒带。");
            return;
        }

        // 计算倒带阶段需要回溯的帧数
        float fixedDt = Time.fixedDeltaTime > 0 ? Time.fixedDeltaTime : 0.02f; // 默认0.02s
        int rewindBackFrames = Mathf.CeilToInt( rewindBackDuration / fixedDt); // 根据倒带持续时间和速度计算需要回溯的帧数
        
        // 确保不超过历史极限
        backTargetOffset = Mathf.Min(rewindBackFrames, minDepth - 1); // -1因为offset是从0开始的

        if (backTargetOffset <= 0)
        {
            if (debugMode) Debug.LogWarning("计算得到的倒带帧数为0，无法启动倒带。");
            return;
        }

        startHistoryDepth = minDepth;// 记录开始倒带时的历史深度，供复位阶段使用

        if (debugMode) Debug.Log($"[回溯准备]倒带阶段目标偏移：{backTargetOffset}帧，速度：{rewindBackSpeed},复位阶段速度：{rewindForwardSpeed}");

        isRewinding = true;
        currentPhase = RewindPhase.RewindBack;

        if(rewindCoroutine != null)
        {
            StopCoroutine(rewindCoroutine);
        }

        rewindCoroutine = StartCoroutine(RewindProcess());
    }

    private int GetMinHistoryDepth()
    {
        int minDepth = int.MaxValue;
        foreach (var entity in allEntities)
        {
            if (entity != null)
            {
                minDepth = Mathf.Min(minDepth, entity.GetHistoryCount());
            }
        }

        return minDepth == int.MinValue ? 0 : minDepth ;
    }

    private IEnumerator RewindProcess()
    {
        float fixedDt = Time.fixedDeltaTime > 0 ? Time.fixedDeltaTime : 0.02f; // 默认0.02s


        // ================ phase 1: 倒带阶段 ================
        if(debugMode) Debug.Log($"[回溯阶段1]开始倒带阶段");

        int step1 = Mathf.Max(1, Mathf.CeilToInt(rewindBackSpeed)); // 每次回溯的步长，至少1帧
        
        float waitTime1 = (fixedDt * step1) / rewindBackSpeed; // 根据速度调整等待时间

        int currentOffset = 0;

        while (currentOffset < backTargetOffset)
        {
            ApplyStateToAllEntities(currentOffset);
            currentOffset += step1;

            if(currentOffset > backTargetOffset)
            {
                currentOffset = backTargetOffset; // 确保不超过目标偏移
            }

            yield return new WaitForSeconds(waitTime1);
        }

        ApplyStateToAllEntities(backTargetOffset); // 确保最终状态正确

        if(debugMode) Debug.Log($"[回溯阶段1]完成倒带阶段，实际偏移：{currentOffset}帧");

        yield return new WaitForSeconds(postRewindPause); // 倒带阶段结束后短暂停顿

        // ================ phase 2: 复位阶段 ================
        if(debugMode) Debug.Log($"[回溯阶段2]开始复位阶段");
        currentPhase = RewindPhase.RewindForward;

        int step2 = Mathf.Max(1, Mathf.CeilToInt(rewindForwardSpeed)); // 每次复位的步长，至少1帧
        float waitTime2 = (fixedDt * step2) / rewindForwardSpeed; // 根据速度调整等待时间

        int forwardOffset = backTargetOffset; // 从倒带结束的偏移开始复位

        while (forwardOffset > 0)
        {
            forwardOffset -= step2;
            if(forwardOffset < 0)
            {
                forwardOffset = 0; // 确保不超过初始状态
            }
            ApplyStateToAllEntities(forwardOffset);

            yield return new WaitForSeconds(waitTime2);
        }

        ApplyStateToAllEntities(0); // 确保最终状态正确

        if(debugMode) Debug.Log($"[回溯阶段2]完成复位阶段，实际偏移：{forwardOffset}帧");

        // ================ 清理 ================
        isRewinding = false;
        currentPhase = RewindPhase.None;
        EventBus.publish(EventType.TimeRewindEnd);
    }

    private void ApplyStateToAllEntities(int offset)
    {
        foreach (var entity in allEntities)
        {
            if (entity != null)
            {
                if (entity.TryGetStateAtOffset(offset, out var state))
                {
                    entity.ApplyState(state);
                }
            }
        }
    }

    public void RegisterEntity(RewindableEntity entity)
    {
        if (!allEntities.Contains(entity))
        {
            allEntities.Add(entity);
            if (debugMode) Debug.Log($"实体注册到回溯管理器: {entity.gameObject.name}, 当前总数: {allEntities.Count}");
        }
    }
    public void UnregisterEntity(RewindableEntity entity)
    {
        if (allEntities.Contains(entity))
        {
            allEntities.Remove(entity);
            if (debugMode) Debug.Log($"实体从回溯管理器注销: {entity.gameObject.name}, 当前总数: {allEntities.Count}");
        }
    }
}