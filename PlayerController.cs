using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update

    private Rigidbody2D rb;             //刚体组件

    public LayerMask ground;            //地面层

    //身体位置
    public GameObject headPoint;        //头部位置点
    public GameObject leftHandPoint;    //左手位置点
    public GameObject rightHandPoint;   //右手位置点
    public GameObject leftHandAttack;
    public GameObject rightHandAttack;

    
    
    //跳跃相关参数
    public int JUMPCOUNT;               //连跳次数
    private int jumpCount;              //当前可以跳跃次数
    private bool jumpPressed;           //跳跃键是否按下

    //下落相关参数
    //private bool downPressed;           //下键是否按下
    private bool isDown;                //是否处于下落模式
    public float downDistance;          //下落检测距离
    private Collider2D downBlock;         //失效的砖块

    //速度相关参数
    public float horizontalSpeed;       //水平移动速度
    public float jumpForce;             //跳跃速度

    //悬挂相关参数
    public float hangDistance;              //左右悬挂判定距离
    private bool isHang,leftHang,rightHang; //是否处于悬挂状态
    private Vector2 hangPosition;

    //状态相关参数
    private bool isGround;                  //是否在地面
    private bool isAttack;
    private bool forward;                   //左右朝向  true为右，false为左
    

    //攻击相关参数
    public GameObject normalAttackTrigger;  //普通攻击判定范围
    public float triggerTime;               //判定时长
    public float NORMALATTACKCD;            //普通攻击冷却时长
    private float normalAttackCD=0;         //剩余攻击冷却时长
    private GameObject newAttack;           //创建的普通攻击判定

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        isHang = false;
        isDown = false;
        isAttack = false;
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
        CDUpdate();
        HangCheck();
        GroundCheck();
        AttackAction();
        HorizontalAction();
        JumpAction();
        FallAction();
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
    } 
    void AttackAction()
    {
        leftHandAttack.transform.position = leftHandPoint.transform.position;
        rightHandAttack.transform.position = rightHandPoint.transform.position;
        if (isHang == true) return;
        if (Input.GetButton("Fire1") && normalAttackCD == 0)
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
                Debug.Log("攻击");
                Destroy(newAttack, triggerTime);
                normalAttackCD = NORMALATTACKCD;
            }

        }
    }
    void HorizontalAction() //左右移动
    {
        if (isAttack)
        {
            return;
        }
        if (isHang)
        {
            //transform.position = hangPosition;
            rb.constraints = RigidbodyConstraints2D.FreezePosition|RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            float h = Input.GetAxis("Horizontal");
            if (h > 0)                  //向右运动
            {
                forward = true;
                //Debug.Log("forward 1");
            }
            else if (h<0)               //向左运动
            {
                forward = false;
                //Debug.Log("forward 0");  
            }
            transform.position += new Vector3(h * horizontalSpeed, 0, 0);
        }
    }

    void HangCheck()     //悬挂判定
    {
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
