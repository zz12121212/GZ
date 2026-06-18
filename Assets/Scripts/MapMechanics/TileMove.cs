using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMove : MonoBehaviour
{
    [Header("三层视差背景")]
    public Transform bgFar;      // 远景
    public Transform bgMidFar;   // 中远景
    public Transform bgMidNear;  // 中近景

    [Header("视差系数")]
    public float farFactor = 0.25f;
    public float midFarFactor = 0.55f;
    public float midNearFactor = 0.85f;

    private Vector3 oldCamPos;

    void Start()
    {
        oldCamPos = transform.position;
    }

    void LateUpdate()
    {
        float deltaX = transform.position.x - oldCamPos.x;

        bgFar.position += new Vector3(deltaX * farFactor, 0, 0);
        bgMidFar.position += new Vector3(deltaX * midFarFactor, 0, 0);
        bgMidNear.position += new Vector3(deltaX * midNearFactor, 0, 0);

        oldCamPos = transform.position;
    }
}

