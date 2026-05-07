using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class HorseController : MonoBehaviour
{
    [Header("判断参数")]
    [Tooltip("攻击判断")]
    private bool isAttack = false;

    [Header("组件")]
    [Tooltip("Horse动画器")]
    private Animator horse_Animtor;

    [Header("进度条")]
    [Tooltip("Horse检测进度条")]
    public Image progressBar;
    [Tooltip("Horse进度条长度")]
    [Range(0f, 1f)]
    public float barFillAmount = 0.0f;
    [Tooltip("进度条变化速度")]
    public float fillSpeed = 1.0f;

    void Start()
    {
        horse_Animtor = GetComponent<Animator>();
        progressBar.fillAmount = 0.0f;
        gameObject.tag = "Untagged";
    }

    void Update()
    {
        ProgressBarController();
        AnimatorController();
    }

    /* --- 方法：Horse动画控制器 --- */
    void AnimatorController()
    {
        horse_Animtor.SetBool("attack", isAttack);
    }

    /* --- 方法：Horse检测玩家进入攻击范围 --- */
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isAttack = true;
        }
    }

    /* --- 方法：Horse检测玩家离开攻击范围 --- */
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isAttack = false;
        }
    }

    /* --- 方法：Horse进度条 & Tag切换 --- */
    void ProgressBarController()
    {
        if(isAttack)
        {
            if (barFillAmount >= 1)
            {
                barFillAmount = 1;
                gameObject.tag = "SightDamage";
            }
            barFillAmount += Time.deltaTime * fillSpeed;
        }
        else
        {
            if (barFillAmount <= 0)
            {
                barFillAmount = 0;
                gameObject.tag = "Untagged";
            }
            barFillAmount -= Time.deltaTime * fillSpeed;
        }
        progressBar.fillAmount = barFillAmount;
    }
}
