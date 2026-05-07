using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

/*说明：
 * 0.该脚本为人工手写，AI辅助，挂载在Player上，初始默认激活
 * 1.脚本中的groundedCheckPoint是一个空物体，位置调整到角色底部，作为碰撞的检测点
 * 2.需要添加HammerDamage、WaveDamage、SightDamage、TrapDamage四个tag，保持命名此处一致
 * 3.StatusJudgment()、PlayerState()、PlayerAnimator()三个方法均用isDie进行判断
 * 4.伤害系统1处理角色与伤害源发生碰撞，设置isHurt为true；伤害系统2处理角色与伤害源分离，设置isHurt为false
 * 5.地面图层要设置为Ground、伤害源要设置对应的tag
 * 6.分为Player_Hurt和Player_Hurt_Sight两个受伤动画片段，对应相应的参数，
 *     其中Player_Hurt动画片段初始帧和结束帧添加事件LockInput()和UnlockInput()
 * ---角色跳跃需要适配、伤害CD与受伤动画需要适配
 */

public class PlayerController : MonoBehaviour
{
    [Header("角色属性")]
    [Tooltip("角色行走速度")]
    public float walkSpeed = 2f;
    [Tooltip("角色奔跑速度")]
    public float runSpeed = 5f;
    [Tooltip("角色跳跃高度")]
    public float jumpForce = 5f;
    [Tooltip("角色生命值")]
    public float health = 100.0f;

    [Header("获取角色组件")]
    [Tooltip("角色2D刚体")]
    private Rigidbody2D rb;
    [Tooltip("角色2D碰撞体")]
    private Collider2D playerCollider;
    [Tooltip("角色动画器")]
    private Animator playerAnimator;

    [Header("判断参数")]
    [Tooltip("判断角色行走")]
    private bool isWalk = false;
    [Tooltip("判断角色奔跑")]
    private bool isRun = false;
    [Tooltip("判断角色着地")]
    private bool isGrounded = true;
    [Tooltip("判断角色跳跃")]
    private bool isJump = false;
    [Tooltip("判断角色受伤")]
    private bool isHurt = false;   //isHurt用来触发动画
    [Tooltip("判断角色受到视线伤害")]
    private bool isSightHurt = false;   //专门用于视线伤害
    [Tooltip("记录上次受伤时间")]
    private float lastDamageTime = -1f;
    [Tooltip("受伤时间CD")]
    public float damageCooldown = 1f;    //受伤CD时间用时间戳实现
    [Tooltip("判断角色死亡")]
    private bool isDie = false;
    [Tooltip("判断锁定角色行动")]
    private bool isLockInput = false;

    [Header("2D物理材质")]
    [Tooltip("有摩擦材质")]
    public PhysicsMaterial2D material_Friction;
    [Tooltip("无摩擦材质")]
    public PhysicsMaterial2D material_NotFriction;

    [Header("碰撞检测")]
    [Tooltip("碰撞检测点")]
    public Transform groundedCheckPoint;
    [Tooltip("碰撞检测图层")]
    public LayerMask ground;

    [Header("伤害来源Tag")]
    [Tooltip("陷阱伤害")]
    public string trapDamage = "TrapDamage";
    [Tooltip("锤子伤害")]
    public string hammerDamage = "HammerDamage";
    [Tooltip("冲击波伤害")]
    public string waveDamage = "WaveDamage";
    [Tooltip("视线追踪伤害")]
    public string sightDamage = "SightDamage";

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();     //获取角色刚体
        playerCollider = rb.GetComponent<Collider2D>();     //获取角色碰撞器
        playerAnimator = GetComponent<Animator>();     //获取角色动画器
    }

    void Update()
    {
        StatusJudgment();
        PlayerState();
        PlayerAnimator();
        PhysicalMaterialSwitch();
        Debug.Log("生命值：" + health);
    }

    /* ----- 方法：角色状态判断 ----- */
    void StatusJudgment()
    {
        /*判断isDie*/
        //角色死亡则设置其他运动判断参数为false
        if (health <= 0)
        {
            health = 0;
            isDie = true;
            isWalk = false;
            isRun = false;
            isJump = false;
            isHurt = false;
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
        //死亡和一般受伤导致的禁止输入都会取消状态判断
        if (isDie) return;
        if (isLockInput) return;
        /*判断isGrounded*/
        isGrounded = Physics2D.OverlapCircle(groundedCheckPoint.position, 0.1f, ground);
        /*判断isWalk和isRun*/
        float dirX = Input.GetAxisRaw("Horizontal");
        if (dirX != 0)
        {
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                isWalk = true;
                isRun = false;
            }
            else if (Input.GetKey(KeyCode.LeftShift))
            {
                isWalk = false;
                isRun = true;
            }
        }
        else if (dirX == 0)
        {
            isWalk = false;
            isRun = false;
        }
        /*判断isJump*/
        if (isGrounded && Input.GetKeyDown(KeyCode.W))
        {
            isJump = true;
        }
        else isJump = false;
    }
    

    /* ----- 方法：角色状态机 ----- */
    void PlayerState()
    {
        //死亡和一般受伤导致的禁止输入都会取消角色状态
        if (isDie) return;
        /*角色受伤*/
        if(isHurt)
        {
            rb.velocity = new Vector2(0f, 0f);
        }
        if (isLockInput) return;
        /*角色行走和奔跑*/
        float dirX = Input.GetAxisRaw("Horizontal");
        if ((isWalk || isRun))
        {
            float speed = isRun ? runSpeed : walkSpeed;
            rb.velocity = new Vector2(dirX * speed, rb.velocity.y);
        }
        else
        {
            //角色异常滑动bug修复：当不移动时，手动将水平速度归零
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
        /*角色跳跃*/
        if (isJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    /* ----- 方法：角色动画器 -----*/
    void PlayerAnimator()
    {
        /*角色死亡动画*/
        if (isDie)
        {
            playerAnimator.Play("Player_Die");
            return;
        }
        /*角色受伤动画*/
        if(isSightHurt)
        {
            playerAnimator.Play("Player_Hurt_Sight");
        }
        if (isHurt)
        {
            playerAnimator.Play("Player_Hurt");
        }
        //死亡和一般受伤导致的禁止输入都会取消角色动画
        if (isLockInput) return;
        /*角色转向*/
        float dirX = Input.GetAxisRaw("Horizontal");
        if (dirX < 0)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else if (dirX > 0)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        /*角色行走和奔跑动画*/
        //当角色着地时才会播放行走或者奔跑动画
        if (isGrounded)
        {
            playerAnimator.SetBool("walk", isWalk);
            playerAnimator.SetBool("run", isRun);
        }
        else
        {
            playerAnimator.SetBool("walk", false);
            playerAnimator.SetBool("run", false);
        }
        /*角色跳跃动画*/
        if (isJump)
        {
            playerAnimator.Play("Player_Jump");
        }
    }
    
    /* ----- 方法：伤害系统1 ----- */
    //伤害系统1处理角色与锤子、冲击波、陷阱这类不持续在伤害源碰撞器内的交互
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //如果还在CD中，直接忽略
        if (Time.time - lastDamageTime < damageCooldown) return;
        //1.锤子每次伤害10，伤害间隔0.5s
        if (collision.gameObject.CompareTag(hammerDamage))
        {
            ApplyDamage(10);
        }
        //2.冲击波每次伤害20
        else if (collision.gameObject.CompareTag(waveDamage))
        {
            ApplyDamage(20);
        }
        //3.陷阱伤害100，角色直接死亡
        else if (collision.gameObject.CompareTag(trapDamage))
        {
            isDie = true;
            health = 0;
        }
    }

    /* ----- 方法：伤害系统2 ----- */
    //伤害系统处理角色与视线这类持续在伤害源触发器内的交互
    private void OnTriggerStay2D(Collider2D collision)
    {
        {
            //如果还在CD中，直接忽略
            if (Time.time - lastDamageTime < damageCooldown) return;
            //4.视线追踪每次伤害25，伤害间隔0.5s
            if (collision.gameObject.CompareTag(sightDamage))
            {
                ApplySightDamage(25);
            }
        }
    }

    /* ----- 方法：一般伤害处理 ----- */
    void ApplyDamage(int damageAmount)
    {
        health -= damageAmount;
        isHurt = true;
        lastDamageTime = Time.time;
        //启动协程
        StartCoroutine(ResetHurtStateAfterDelay());
    }

    /* ----- 协程：一般伤害CD结束后重置isHurt -----*/
    IEnumerator ResetHurtStateAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        isHurt = false;
    }

    /* ----- 方法：视线伤害处理 ----- */
    void ApplySightDamage(int damageAmount)
    {
        health -= damageAmount;
        isSightHurt = true;
        lastDamageTime = Time.time;
        //启动协程
        StartCoroutine(ResetSightHurtStateAfterDelay());
    }

    /* ----- 协程：视线伤害CD结束后重置isSightHurt -----*/
    IEnumerator ResetSightHurtStateAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        isSightHurt = false;
    }

    /* ----- 方法：禁止输入 -----*/
    //玩家受到特定伤害时受伤动画期间无法操作，用于动画片段中的事件
    public void LockInput()
    {
        isLockInput = true;
    }

    /* ----- 方法：重启输入 -----*/
    //玩家受伤动画结束后重启可操作，用于动画片段中的事件
    public void UnlockInput()
    {
        isLockInput = false;
    }

    /* ----- 方法：角色物理材质切换 ----- */
    void PhysicalMaterialSwitch()
    {
        if(isGrounded)
        {
            playerCollider.sharedMaterial = material_Friction;
        }
        else if(!isGrounded)
        {
            playerCollider.sharedMaterial = material_NotFriction;
        }
    }
}
