using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarController : MonoBehaviour
{
    public float changeTime =1;
    public Image bar;
    private float fillAmount = 1f;
    void Start()
    {
        bar.type = Image.Type.Filled;
        bar.fillMethod = Image.FillMethod.Horizontal;
        bar.fillOrigin = 0;
        bar.fillAmount = 1f;
    }

    public void UpdateValue(float fullValue,float value) {
        float newFillAmount = value / fullValue;
        if (newFillAmount != fillAmount) {
            StartCoroutine(ChangeValue(fillAmount,newFillAmount));
            fillAmount = bar.fillAmount;
        }
    }

    private IEnumerator ChangeValue(float oldAmount,float newAmount) {
        float changeValueVarTime = (oldAmount - newAmount) / changeTime;
        while (Mathf.Abs(bar.fillAmount-newAmount) > 0.05) {
            bar.fillAmount -= changeValueVarTime*Time.deltaTime;
            yield return null;
        }
        bar.fillAmount = newAmount;

    }
}
