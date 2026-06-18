using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundParallax : MonoBehaviour
{

  
    public static BackGroundParallax Instance;

    // 三层背景
    public Transform farBackground;   
    public Transform midBackground;    
    public Transform nearBackground;   

    // 各自速度
    public float farSpeed = 0.1f;
    public float midSpeed = 0.4f;
    public float nearSpeed = 0.8f;

    private Camera mainCam;
    private Vector3 lastCamPos;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        mainCam = Camera.main;
        lastCamPos = mainCam.transform.position;
    }

    void LateUpdate()
    {
        Vector3 delta = mainCam.transform.position - lastCamPos;

        // 控制三层移动
        farBackground.position += delta * farSpeed;
        midBackground.position += delta * midSpeed;
        nearBackground.position += delta * nearSpeed;

        lastCamPos = mainCam.transform.position;
    }
}
