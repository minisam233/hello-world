using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RangeAttackData;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update

    private Rigidbody2D rb;             //刚体组件

    public LayerMask ground;            //地面层

    public RangeAttackData rad;

    //身体位置
    public GameObject headPoint;        //头部位置点
    public GameObject leftHandPoint;    //左手位置点
    public GameObject rightHandPoint;   //右手位置点
    //public GameObject leftHandAttack;
    //public GameObject rightHandAttack;

    //数值状态相关参数
    public float MAXHEALTH;
    public float MAXMAGICPOINT;
    private float health;
    private float magicPoint;
    
    //跳跃相关参数
    public int JUMPCOUNT;               //连跳次数
    private int jumpCount;              //当前可以跳跃次数
    private bool jumpPressed;           //跳跃键是否按下

    //下落相关参数
    //private bool downPressed;           //下键是否按下
    private bool isDown;                  //是否处于下落模式
    public float downDistance;            //下落检测距离
    private Collider2D downBlock;         //失效的砖块

    //速度相关参数
    public float horizontalSpeed;       //水平移动速度
    public float jumpForce;             //跳跃速度

    //悬挂相关参数
    public float hangDistance;              //左右悬挂判定距离
   
    private Vector2 hangPosition;           //悬挂位置

    //状态相关参数 
    private bool isHang,leftHang,rightHang; //是否处于悬挂状态
    private bool isGround;                  //是否在地面
    private bool isAttack;                  //是否在攻击
    private bool isInvincible;              //是否处于无敌状态
    private bool isDodge;                   //是否在翻滚
    private bool forward;                   //左右朝向  true为右，false为左
    private bool isBlock;                   //是否在格挡
    private bool canBlock;                  //此时能否举盾 
    private bool isBounce;                  //是否处于弹反
    private bool isMagic;                   //是否处于施法状态
    private bool canShoot;                  //此时能否射击

    //闪避相关参数
    public float DODGEINVINCIBLETIMER;       //翻滚无敌时间
    public float DODGETIMER;                 //翻滚时间
    public float dodgeSpeed;                 //翻滚速度
    private int dodgeForward;                //翻滚方向，1为向右，-1为向左
    private float dodgeInvincibleTimer;      //翻滚无敌时间计时器
    private float dodgeTimer;                //翻滚时间计时器

    //格挡相关参数
    public float BOUNCETIMER;                 //弹反时间常数
    public float BLOCKCD;                     //举盾冷却时间
    public float blockSpeedDebuff;            //举盾速度减益
    private float blockCDTimer;               //举盾CD计时器
    private float bounceTimer;                //弹反时间计时器

    //投掷相关参数
    public GameObject stone;                     //石头物体
    public float stoneSpeed;                     //石头初速度
    public GameObject knife;                     //小刀（施术）物体
    public float knifeSpeed;                     //小刀（施术）初速度
    public float SHOOTCD;                        //射击冷却时间

    private GameObject rangeAttack;              //远程攻击物体
    private float rangeAttackSpeed;              //远程攻击初速度
    private GameObject newShoot;                 //创建的新射击
    private float shootCDTimer;                  //射击冷却时间计时器

    //攻击相关参数
    public GameObject normalAttackTrigger;  //普通攻击判定范围
    public float triggerTime;               //判定时长
    public float NORMALATTACKCD;            //普通攻击冷却时长
    public float NORMALATTACKSTATUSTIMER;   //普通攻击状态重置时长常数
    public int NORMALATTACKSTATUS;          //普通攻击状态常熟
    private int normalAttackStatus;         //普通攻击状态
    private float normalAttackStatusTimer;  //普通攻击状态时长
    private float normalAttackCD=0;         //剩余攻击冷却时长
    private GameObject newAttack;           //创建的普通攻击判定

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        isHang = false;
        isDown = false;
        isAttack = false;
        isDodge = false;
        isInvincible = false;
        isMagic = false;
        canBlock = true;
        canShoot = true;
    }

    // Update is called once per frame
    void Update()
    {
        //跳跃按键状态更新
        if (Input.GetButtonDown("Jump") && jumpCount > 0)
        {
            jumpPressed = true;
        }

    }
    void FixedUpdate()
    {
        CDUpdate();              //冷却更新
        HangCheck();             //悬挂判定
        GroundCheck();           //落地判定
        MagicCheck();            //施法判定
        DodgeAction();           //执行闪避
        AttackAction();          //执行攻击
        ShootAction();           //执行射击
        BlockAction();           //执行格挡
        HorizontalAction();      //水平移动
        JumpAction();            //跳跃移动
        FallAction();            //落下移动
    }
    
    void CDUpdate()     //冷却更新
    {
        if (normalAttackCD > 0)
        {
            normalAttackCD -= Time.deltaTime;
            if (normalAttackCD <= 0)
            {
                isAttack = false;
                normalAttackCD = 0;
            }
        }
        if (normalAttackStatusTimer > 0)
        {
            normalAttackStatusTimer -= Time.deltaTime;
            if (normalAttackStatusTimer <= 0)
            {
                normalAttackStatus = 0;
            }
        }
        if (dodgeInvincibleTimer > 0)
        {
            dodgeInvincibleTimer -= Time.deltaTime;
            if (dodgeInvincibleTimer <= 0)
            {
                isInvincible = false;
            }
        }
        if (dodgeTimer > 0)
        {
            dodgeTimer -= Time.deltaTime;
            if (dodgeTimer <= 0)
            {
                isDodge = false;
            }
        }
        if (bounceTimer > 0)
        {
            bounceTimer -= Time.deltaTime;
            if (bounceTimer <= 0)
            {
                isBounce = false;
            }
        }
        if (blockCDTimer > 0)
        {
            blockCDTimer -= Time.deltaTime;
            if (blockCDTimer <= 0)
            {
                canBlock = true;
            }
        }
        if (shootCDTimer > 0)
        {
            shootCDTimer -= Time.deltaTime;
            if (shootCDTimer <= 0)
            {
                canShoot = true;
            }
        }
    } 
    
    void MagicCheck() //检查是否在施法
    {
        if (Input.GetButton("Magic") == true)
        {
            Debug.Log("施法中");
            isMagic = true;
        }
        else isMagic = false;
    }

    void DodgeMove(int dodgeForward) //翻滚移动方向
    {
        transform.position += new Vector3(dodgeForward * dodgeSpeed, 0, 0);
    }

    void DodgeAction()              //翻滚判定
    {
        if (isHang || isDown || !isGround) return;
        if (isDodge) DodgeMove(dodgeForward);
        if (Input.GetButton("Dodge"))
        {
            float h = Input.GetAxis("Horizontal");
            if ( h == 0) { 
                if (forward == true)
                {
                    dodgeForward = -1;
                }
                else
                {
                    dodgeForward = 1;
                }
            }
            else
            {
                dodgeForward = (int)(h / Mathf.Abs(h));
            }
            isBlock = false;
            isInvincible = true;
            isDodge = true;
            dodgeInvincibleTimer = DODGEINVINCIBLETIMER;
            //Debug.DrawLine(transform.position, leftHandPoint.transform.position, Color.red,DODGEINVINCIBLETIMER);
            //Debug.DrawLine(transform.position, rightHandPoint.transform.position, Color.green, DODGETIMER);
            dodgeTimer = DODGETIMER;
            DodgeMove(dodgeForward);
        }
    }

    void AttackAction()           //攻击判定
    {
        //leftHandAttack.transform.position = leftHandPoint.transform.position;
        //rightHandAttack.transform.position = rightHandPoint.transform.position;
        if (isHang == true || isDodge==true || isDown==true) return;
        //新的攻击
        if (Input.GetButton("Attack") && normalAttackCD == 0)      
        {
            if (isGround)
            {
                isAttack = true;
                if (forward == true)
                {
                    newAttack = (GameObject)Instantiate(normalAttackTrigger, rightHandPoint.transform.position, Quaternion.identity);

                }
                else
                {
                    newAttack = (GameObject)Instantiate(normalAttackTrigger,leftHandPoint.transform.position, Quaternion.identity);
                }
                //Debug.Log(normalAttackStatus);
                //if (normalAttackStatus==0)
                //    Debug.DrawLine(transform.position, leftHandPoint.transform.position, Color.blue, NORMALATTACKSTATUSTIMER);
                //else if (normalAttackStatus==1)
                //    Debug.DrawLine(transform.position, leftHandPoint.transform.position, Color.red, NORMALATTACKSTATUSTIMER);
                //else
                //    Debug.DrawLine(transform.position, leftHandPoint.transform.position, Color.green, NORMALATTACKSTATUSTIMER);
                isBlock = false;
                normalAttackStatus = (normalAttackStatus + 1) % NORMALATTACKSTATUS;
                normalAttackStatusTimer = NORMALATTACKSTATUSTIMER;
                Destroy(newAttack, triggerTime);
                normalAttackCD = NORMALATTACKCD;
            }

        }
    }

    void ShootAction()         //射击判定
    {
        if (isHang == true || isAttack == true || isDodge == true || isDown == true || isGround == false)
        {
            return;
        }
        bool s = Input.GetButton("Shoot");
        if (s==true && canShoot == true)
        {
            Debug.Log("射击");
            if (isMagic == true)
            {
                rangeAttack = knife;
                rangeAttackSpeed = knifeSpeed;
            }
            else
            {
                rangeAttack = stone;
                rangeAttackSpeed = stoneSpeed;
            }
            
            if (forward == true)
            {
                Rigidbody2D rangAttackRb;
                
                newShoot = Instantiate(rangeAttack, rightHandPoint.transform.position, Quaternion.identity);
                rangAttackRb = newShoot.GetComponent<Rigidbody2D>();
                rangAttackRb.velocity=Vector2.right*rangeAttackSpeed;
            }
            else
            {
                Rigidbody2D rangeAttackRb;
                
                newShoot = Instantiate(rangeAttack, leftHandPoint.transform.position, Quaternion.identity);
                rangeAttackRb = newShoot.GetComponent<Rigidbody2D>();
                rangeAttackRb.velocity=Vector2.left*rangeAttackSpeed;
                
            }
            Destroy(newShoot, 5);
            canShoot = false;
            shootCDTimer = SHOOTCD;
        }
    }

    void BlockAction()        //格挡判定
    {
        if (isHang == true || isAttack == true || isDodge == true || isDown==true  || isGround==false)
        {
            return;
        }
        bool b = Input.GetButton("Block");
        //if (isBlock) Debug.DrawLine(transform.position,leftHandPoint.transform.position, Color.black, BOUNCETIMER);
        if (isBlock == false && b==true && canBlock==true) //新的格挡
        {
            isBlock = true;
            isBounce = true;
            bounceTimer = BOUNCETIMER;
            //Debug.DrawLine(transform.position, rightHandPoint.transform.position, Color.yellow, BOUNCETIMER);
        }
        else if (isBlock ==true && b == false)             //不再格挡
        {
            isBlock = false;
            blockCDTimer = BLOCKCD;
            canBlock = false;
        }
    } 

    void HorizontalAction() //左右移动
    {
        if (isAttack==true || isDodge==true)
        {
            return;
        }
        if (isHang)  //悬挂时改变刚体状态，锁定位置和旋转
        {
            //transform.position = hangPosition;
            rb.constraints = RigidbodyConstraints2D.FreezePosition|RigidbodyConstraints2D.FreezeRotation;
        }
        else         //改变方向
        {
            float h = Input.GetAxis("Horizontal");
            if (isBlock == false)  //格挡时朝向不变
            {
                if (h > 0)                  //向右运动
                {
                    forward = true;
                    //Debug.Log("forward 1");
                }
                else if (h < 0)               //向左运动
                {
                    forward = false;
                    //Debug.Log("forward 0");  
                }
            }
            float speed;
            speed = h * horizontalSpeed *(1- System.Convert.ToInt32(isBlock) * blockSpeedDebuff);
            //else speed = h * horizontalSpeed - System.Convert.ToInt32(isBlock) * blockSpeedDebuff;
            transform.position += new Vector3(speed, 0, 0);
        }
    }

    void HangCheck()     //悬挂判定
    {
        //头部向左悬挂判定
        RaycastHit2D handHitInfo,headHitInfo;
        handHitInfo = Physics2D.Raycast(transform.position, Vector2.left, hangDistance, ground);
        headHitInfo = Physics2D.Raycast(headPoint.transform.position, Vector2.left, hangDistance, ground);
        if (headHitInfo==false && handHitInfo!=false && handHitInfo.collider.tag == "ground")  
        {
            leftHang = true;
        }
        else
        {
            leftHang = false;
        }

        //头部向有悬挂判定
        handHitInfo = Physics2D.Raycast(transform.position, Vector2.right, hangDistance, ground);
        headHitInfo = Physics2D.Raycast(headPoint.transform.position, Vector2.right, hangDistance, ground);

        if (headHitInfo == false && handHitInfo != false && handHitInfo.collider.tag == "ground")
        {
            rightHang = true;
        }
        else
        {
            rightHang = false;
        }
        if (!isHang && (rightHang || leftHang) && !isDown)
        {
            hangPosition = transform.position;
            //Debug.Log("开始悬挂");
        }
        isHang = (rightHang || leftHang) && !isDown;
    }

    void GroundCheck()      //检查是否在地面
    {
        RaycastHit2D hitInfo;
        hitInfo=Physics2D.Raycast(transform.position, Vector2.down, 2.05f,ground);
        //Debug.Log(hitInfo.transform.position);
        //Debug.DrawLine(transform.position, hitInfo.transform.position, Color.red);
        if (hitInfo ==true &&  hitInfo.collider.tag == "ground" && isDown==false)
        {
            isGround = true;
            isHang = false;
            //Debug.Log("在地面");
        }
        else
        {
            isGround = false;
        }
     
    }
    void JumpAction()       //跳跃判定
    {
        if (isAttack) return;
        if (isGround == true || isHang==true)
        {
            jumpCount = JUMPCOUNT;
        }
        if (jumpPressed && isGround == true)
        {
            //Debug.Log("地面跳");
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpPressed = false;
            jumpCount--;
        }
        else if (jumpPressed && isHang)
        {
            //Debug.Log("爬墙跳");
            isHang = false;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpCount--;
            jumpPressed = false;
        }
        else if (jumpPressed && jumpCount>0 && !isGround)
        {
            //Debug.Log("空中跳");
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpPressed = false;
            jumpCount--;
        }
        
    }
    void FallAction()
    {
        if (isAttack) return;
        if (isGround && Input.GetButton("Down"))
        {
            //Debug.Log("开始下落");
            RaycastHit2D hitInfo;
            hitInfo = Physics2D.Raycast(transform.position, Vector2.down, 2.05f, ground);
            if (hitInfo && hitInfo.collider.tag == "ground")
            {
                downBlock = hitInfo.collider;
            }
            downBlock.isTrigger = true;
            isDown = true;
            isGround = false;
            jumpCount--;
        }
        if (isDown)
        {
            //Debug.Log("下落判定");
            RaycastHit2D hitInfo;
            hitInfo = Physics2D.Raycast(headPoint.transform.position, Vector2.down, downDistance, ground);
            if (!hitInfo)
            {
                //Debug.Log("下落结束");
                downBlock.isTrigger = false;
                isDown = false;
            }
        }
    }
}
