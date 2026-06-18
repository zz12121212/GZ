using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform player;

    [Header("地图左右边界")]
    public float mapLeft;
    public float mapRight;

    [Header("三层设置：基准相机Y / 本层相机最高上限Y")]
    [Tooltip("相机基准高度")]
    public float camBase1;
    public float camBase2;
    public float camBase3;
    [Tooltip("相机能抬到的最高点（层分界线）")]
    public float camMax1;
    public float camMax2;
    public float camMax3;
    [Header("层间交界Y坐标")]
    public float border12; 
    public float border23; 

    [Header("平滑速度")]
    public float smoothX = 6f;
    public float smoothYFollowJump = 4f;
    public float smoothYFallBack = 1.8f;  

    private Camera cam;
    private float currentBaseY;   
    private float currentMaxY;   
    private Vector3 targetPos;

    void Start()
    {
        cam = GetComponent<Camera>();
        RefreshLayerData();
        targetPos = transform.position;
    }

    void LateUpdate()
    {
        if (player == null) return;
        RefreshLayerData();

        //x跟随
        float halfCamWidth = cam.orthographicSize * cam.aspect;
        float minClampX = mapLeft + halfCamWidth;
        float maxClampX = mapRight - halfCamWidth;
        float targetX = Mathf.Clamp(player.position.x, minClampX, maxClampX);
        //y跟随
        float wantedY = player.position.y;
        float targetY = Mathf.Clamp(wantedY, currentBaseY, currentMaxY);

        // 判断玩家是在空中跳跃还是落地，切换不同平滑速度
        float smoothY;
        if (player.GetComponent<Rigidbody2D>().velocity.y > 0)
        {
            smoothY = smoothYFollowJump;
        }
        else
        {
            smoothY = smoothYFallBack;
        }

        // 移动相机
        Vector3 pos = transform.position;
        float newX = Mathf.Lerp(pos.x, targetX, Time.deltaTime * smoothX);
        float newY = Mathf.Lerp(pos.y, targetY, Time.deltaTime * smoothY);

        transform.position = new Vector3(newX, newY, pos.z);
    }

    // 刷新数据
    void RefreshLayerData()
    {
        float pY = player.position.y;
        if (pY >=border12)
        {
            currentBaseY = camBase1;
            currentMaxY = camMax1;
        }
        else if (pY  >= border23 )
        {
            currentBaseY = camBase2;
            currentMaxY = camMax2;
        }
        else
        {
            currentBaseY = camBase3;
            currentMaxY = camMax3;
        }
    }
}