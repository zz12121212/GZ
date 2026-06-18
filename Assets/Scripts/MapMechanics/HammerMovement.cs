using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerMovement : MonoBehaviour
{
    private bool moveToLeft = true;

    public float rotateSpeed = 5f;
    public float leftLimit = -60f;
    public float rightLimit = 60f;

    void FixedUpdate()
    {
        // 获取旋转角度
        float currentZ = transform.localEulerAngles.z;
        currentZ = Mathf.DeltaAngle(0, currentZ);

        if (moveToLeft)
        {
            // 向左旋转
            transform.localEulerAngles = new Vector3(0, 0, currentZ - rotateSpeed);

            if (currentZ <= leftLimit)
            {
                moveToLeft = false;
            }
        }
        else
        {
            // 向右旋转
            transform.localEulerAngles = new Vector3(0, 0, currentZ + rotateSpeed);

            if (currentZ >= rightLimit)
            {
                moveToLeft = true;
            }
        }
    }
}