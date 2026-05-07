using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;

/*0.该脚本为人为书写，AI辅助，挂载在DogMonster上，初始默认激活
 * 1.DogMonster需要添加盒体碰撞器，添加并设置DogMonster图层，在项目设置中取消DogMonster与WaveDamage的碰撞
 */

public class DogController : MonoBehaviour
{
    [Header("动画")]
    [Tooltip("Dog动画器")]
    private Animator dogAnimator;

    [Header("控制攻击")]
    [Tooltip("波控制脚本")]
    public WaveController waveController;

    [Header("判断参数")]
    [Tooltip("攻击判断")]
    private bool isAttack = false;
    [Tooltip("死亡判断")]
    private bool isDie = false;

    [Header("碰撞器")]
    [Tooltip("GogMonster盒体碰撞器")]
    public BoxCollider2D dieDetectionBoxCollider;

    void Start()
    {
        dogAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        AnimatorState();
    }

    /* ----- 方法：玩家进入检测范围DogMonster开始攻击 ----- */
    private void OnTriggerStay2D(Collider2D collision)
    {
        //DogMonster死亡则返回，避免每帧轮流持续调用
        if (isDie) return;
        if(collision.gameObject.CompareTag("Player"))
        {
            isAttack = true;
            //当DogMonster播放持续动画时才会从嘴的位置产生波
            if(dogAnimator.GetCurrentAnimatorStateInfo(0).IsName("Dog_AttackContinue"))
            {
                waveController.enabled = true;
            }
        }
    }

    /* ----- 方法：对应上述方法，玩家离开或不在检测范围DogMonster停止攻击 ----- */
    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            waveController.enabled = false;
            isAttack = false;
        }
    }

    /* ----- 方法：玩家靠近碰到DogMonster后者死亡 ----- */
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            isDie = true;
        }
    }

    /* ----- 方法：DogMonster动画状态机 ----- */
    void AnimatorState()
    {
        if(isDie)
        {
            dieDetectionBoxCollider.enabled = false;
            waveController.enabled = false;
            dogAnimator.Play("Dog_Die");
            return;
        }
        dogAnimator.SetBool("attack", isAttack);
    }
}
