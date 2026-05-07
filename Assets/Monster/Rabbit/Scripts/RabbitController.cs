using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*说明：
 * 0.该脚本挂载在Rabbit上
 * 1.兔子会跟随玩家的动作（行走、奔跑、跳跃），速度和跳跃高度与玩家相同
 * 2.兔子只有触碰TrapDamage时才会死亡
 * 3.兔子需要挂载Rigidbody2D、BoxCollider2D（普通碰撞器）、BoxCollider2D（触发器，用于检测玩家）
 * 4.需要将兔子的触发器大小调整好，用于检测玩家进入范围
 * 5.兔子的物理材质会跟随玩家切换（有摩擦/无摩擦）
 */

public class RabbitController : MonoBehaviour
{
    [Header("组件")]
    [Tooltip("兔子刚体")]
    private Rigidbody2D rb;
    [Tooltip("兔子2D碰撞器")]
    private Collider2D rabbitCollider;
    [Tooltip("兔子动画器")]
    private Animator rabbitAnimator;
    [Tooltip("兔子渲染器")]
    private SpriteRenderer rabbitRenderer;

    [Header("跟随设置")]
    [Tooltip("要跟随的玩家")]
    private Transform player;
    [Tooltip("玩家控制器（获取速度和跳跃数据）")]
    private PlayerController playerController;
    [Tooltip("玩家刚体（获取速度）")]
    private Rigidbody2D playerRb;
    [Tooltip("玩家的碰撞器（用于获取物理材质）")]
    private Collider2D playerCollider;

    [Header("2D物理材质")]
    [Tooltip("有摩擦材质")]
    public PhysicsMaterial2D material_Friction;
    [Tooltip("无摩擦材质")]
    public PhysicsMaterial2D material_NotFriction;

    [Header("兔子状态")]
    [Tooltip("是否在跟随模式")]
    private bool isFollowing = false;
    [Tooltip("是否着地")]
    private bool isGrounded = true;
    [Tooltip("是否跳跃（只在起跳瞬间为true）")]
    private bool isJump = false;
    [Tooltip("是否死亡")]
    private bool isDie = false;

    [Header("地面检测")]
    [Tooltip("地面检测点")]
    public Transform groundedCheckPoint;
    [Tooltip("地面图层")]
    public LayerMask ground;

    [Header("死亡淡出设置")]
    [Tooltip("死亡淡出持续时间（秒）")]
    public float fadeOutDuration = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rabbitCollider = GetComponent<Collider2D>();
        rabbitAnimator = GetComponent<Animator>();
        rabbitRenderer = GetComponent<SpriteRenderer>();

        // 冻结兔子的旋转，防止倒下
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // 查找玩家
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerController = playerObj.GetComponent<PlayerController>();
            playerRb = playerObj.GetComponent<Rigidbody2D>();
            playerCollider = playerObj.GetComponent<Collider2D>();
        }
    }

    void Update()
    {
        if (isDie) return;

        StatusJudgment();
        FollowPlayer();
        RabbitAnimator();
        SwitchPhysicsMaterial();
    }

    /* ----- 方法：兔子状态判断（模仿Player）----- */
    void StatusJudgment()
    {
        // 死亡时不再处理状态
        if (isDie) return;

        // 只有在跟随模式下才同步玩家的状态
        if (isFollowing && playerController != null)
        {
            // 同步isGrounded（用兔子的地面检测）
            isGrounded = Physics2D.OverlapCircle(groundedCheckPoint.position, 0.1f, ground);

            // 同步跳跃：检测玩家是否在跳跃
            if (playerRb.velocity.y > 0 && rb.velocity.y <= 0 && isGrounded)
            {
                isJump = true;
                // 执行跳跃
                rb.velocity = new Vector2(rb.velocity.x, playerController.jumpForce);
            }
            else
            {
                isJump = false;
            }
        }
        else
        {
            // 不跟随时的地面检测
            isGrounded = Physics2D.OverlapCircle(groundedCheckPoint.position, 0.1f, ground);
            isJump = false;
        }
    }

    /* ----- 方法：跟随玩家移动（同步水平速度）----- */
    void FollowPlayer()
    {
        if (!isFollowing || player == null || playerController == null) return;
        if (isDie) return;

        // 完全同步玩家的水平速度
        if (playerRb != null)
        {
            float playerXVelocity = playerRb.velocity.x;
            rb.velocity = new Vector2(playerXVelocity, rb.velocity.y);

            // 转向同步：根据玩家水平速度方向同步转向
            if (playerXVelocity < 0)
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
            }
            else if (playerXVelocity > 0)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
            }
        }
    }

    /* ----- 方法：兔子动画器（模仿Player）----- */
    void RabbitAnimator()
    {
        if (rabbitAnimator == null) return;

        /*兔子死亡动画*/
        if (isDie)
        {
            rabbitAnimator.Play("Rabbit_Die");
            return;
        }

        /*兔子跳跃动画（只在起跳瞬间播放一次）*/
        if (isJump)
        {
            rabbitAnimator.Play("Rabbit_Jump");
        }

        // 跟随模式下同步行走/奔跑动画
        if (isFollowing && playerController != null && !isDie)
        {
            float playerXVelocity = playerRb != null ? playerRb.velocity.x : 0f;
            bool isPlayerMoving = Mathf.Abs(playerXVelocity) > 0.1f;
            bool isPlayerRunning = Input.GetKey(KeyCode.LeftShift) && isPlayerMoving;
            bool isPlayerWalking = isPlayerMoving && !isPlayerRunning;

            // 着地时才能播放行走/奔跑动画
            if (isGrounded)
            {
                rabbitAnimator.SetBool("walk", isPlayerWalking);
                rabbitAnimator.SetBool("run", isPlayerRunning);
            }
            else
            {
                rabbitAnimator.SetBool("walk", false);
                rabbitAnimator.SetBool("run", false);
            }
        }
        else
        {
            rabbitAnimator.SetBool("walk", false);
            rabbitAnimator.SetBool("run", false);
        }
    }

    /* ----- 方法：物理材质跟随玩家切换 ----- */
    void SwitchPhysicsMaterial()
    {
        if (rabbitCollider == null || playerCollider == null) return;
        if (isDie) return;

        // 同步玩家的物理材质
        if (playerCollider.sharedMaterial == material_Friction)
        {
            rabbitCollider.sharedMaterial = material_Friction;
        }
        else if (playerCollider.sharedMaterial == material_NotFriction)
        {
            rabbitCollider.sharedMaterial = material_NotFriction;
        }
    }

    /* ----- 方法：玩家进入触发器范围，兔子开始跟随 ----- */
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isFollowing = true;
        }
    }

    /* ----- 方法：玩家离开触发器范围，兔子停止跟随 ----- */
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isFollowing = false;
            // 停止移动
            rb.velocity = new Vector2(0f, rb.velocity.y);
            isJump = false;
        }
    }

    /* ----- 方法：兔子死亡（碰到TrapDamage时触发，播放死亡动画并淡出）----- */
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDie) return;

        if (collision.gameObject.CompareTag("TrapDamage"))
        {
            Die();
        }
    }

    /* ----- 方法：兔子死亡处理（播放死亡动画，透明度平滑变化后销毁，不删除碰撞器）----- */
    void Die()
    {
        if (isDie) return;

        isDie = true;
        isFollowing = false;
        isJump = false;

        // 停止移动
        rb.velocity = Vector2.zero;

        // 注意：不删除碰撞器，只禁用触发器/刚体物理影响
        if (rb != null)
        {
            rb.isKinematic = true;  // 让刚体不再受物理影响
        }

        // 播放死亡动画
        if (rabbitAnimator != null)
        {
            rabbitAnimator.Play("Rabbit_Die");
        }

        // 开始淡出协程
        StartCoroutine(FadeOutAndDestroy());
    }

    /* ----- 协程：透明度平滑变化后销毁 ----- */
    IEnumerator FadeOutAndDestroy()
    {
        float elapsedTime = 0f;
        Color originalColor = rabbitRenderer != null ? rabbitRenderer.color : Color.white;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);

            if (rabbitRenderer != null)
            {
                Color newColor = originalColor;
                newColor.a = alpha;
                rabbitRenderer.color = newColor;
            }

            yield return null;
        }

        // 确保最终透明度为0
        if (rabbitRenderer != null)
        {
            Color finalColor = originalColor;
            finalColor.a = 0f;
            rabbitRenderer.color = finalColor;
        }

        // 销毁兔子
        Destroy(gameObject);
    }
}