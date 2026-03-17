// GlobalRewindManager.cs
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

    // 每帧记录所有实体状态
    private void FixedUpdate()
    {
        if (!isRewinding)
        {
            foreach (var entity in allEntities)
            {
                if (entity != null)
                {
                    entity.RecordState();
                }
                else
                {
                    // 如果实体被销毁了，从列表中移除
                    allEntities.Remove(entity);
                }
            }
        }

    }

    public void OnTimeRewindStart()
    {
        StartRewind();
    }
    public void OnTimeRewindEnd()
    {

    }

    // 开始回溯
    public void StartRewind()
    {
        if (isRewinding)
        {
            Debug.Log("已经在回溯中，无法再次开始回溯");
            return;
        }
        
        if (allEntities.Count == 0)
        {
            Debug.Log("没有可回溯的实体，无法开始回溯");
            return;
        }

        // 计算深度
        int minDepth = GetMinHistoryDepth();// 获取所有实体中最短的历史记录深度，确保回溯时不会越界
        int totalFrames = Mathf.CeilToInt(rewindDuration / Time.fixedDeltaTime);// 计算总共需要回溯的帧数

        Debug.Log($"[倒流启动] 涉及物体数：{allEntities.Count}");
        Debug.Log($"[倒流启动] 需要回溯帧数：{totalFrames}帧");
        Debug.Log($"[倒流启动] 实际可用最小历史帧数：{minDepth}帧");

        if (minDepth <= 0)
        {
            Debug.Log("没有足够的历史记录，无法开始回溯");
            return;
        }

        isRewinding = true;

        // 启动回溯协程
        if (rewindCoroutine != null)
        {
            StopCoroutine(rewindCoroutine);
        }
        rewindCoroutine = StartCoroutine(RewindProcess(minDepth));
    }

    private int GetMinHistoryDepth()
    {
        int minDepth = int.MaxValue;// 初始化为最大值

        foreach (var entity in allEntities)
        {
            if (entity != null)
            {
                int historyCount = entity.GetHistoryCount();
                minDepth = Mathf.Min(minDepth, historyCount);
            }
        }

        return minDepth == int.MaxValue ? 0 : minDepth;// 如果没有实体，返回0
    }

    private IEnumerator RewindProcess(int availableFrames)
    {
        Debug.Log($"[倒流过程] 开始回溯，实际可用帧数：{availableFrames}帧");

        int step = Mathf.Max(1,Mathf.CeilToInt(rewindSpeed));// 计算每次回溯的步长，确保每次至少回溯1帧
        int currentOffset = 0;

        int maxOffset = availableFrames;
        Debug.Log($"[倒流过程] 每次回溯步长：{step}帧，最大偏移：{maxOffset}帧");

        if(maxOffset <= 0)
        {
            Debug.Log("没有足够的历史记录，无法开始回溯");
            isRewinding = false;
            yield break;
        }

        float waitTime = Time.fixedDeltaTime / rewindSpeed;// 计算每次回溯之间的等待时间，确保回溯速度正确
        int loopCount = 0;

        while (currentOffset < maxOffset)
        {
            foreach (var entity in allEntities)
            {
                if (entity != null)
                {
                    entity.TryGetStateAtOffSet(currentOffset, out EntityState state);
                    entity.ApplyState(state);
                }
            }
            currentOffset += step;
            loopCount++;
            // Debug.Log($"[倒流过程] 回溯第{loopCount}次，当前偏移：{currentOffset}帧");
            yield return new WaitForSecondsRealtime(waitTime);// 使用WaitForSecondsRealtime确保回溯过程不受Time.timeScale影响
        }
        Debug.Log($"[倒流过程] 回溯完成，总共回溯了{currentOffset}帧，实际回溯时间：{loopCount * waitTime:F2}秒");

        // 确保最后停在最远的那一帧
        if (maxOffset > 0)
        {
            foreach (var entity in allEntities)
            {
                if (entity != null)
                {
                    entity.TryGetStateAtOffSet(maxOffset - 1, out EntityState state);
                    entity.ApplyState(state);
                }
            }
        }

        isRewinding = false;
        Debug.Log("[倒流过程] 回溯结束，所有实体状态已恢复到目标帧");
        EventBus.publish(EventType.TimeRewindEnd);// 发布回溯结束事件，通知其他系统回溯已完成

    }

    // 当有实体被动态创建或销毁时，调用以下方法进行注册或注销
    public void RegisterEntity(RewindableEntity entity)
    {
        if (entity != null && !allEntities.Contains(entity))
        {
            allEntities.Add(entity);
        }
    }
    public void UnregisterEntity(RewindableEntity entity)
    {
        if (allEntities.Contains(entity))
        {
            allEntities.Remove(entity);
        }
    }
}