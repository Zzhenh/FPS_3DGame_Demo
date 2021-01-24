using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Transform m_transform;       //自身位置组件
    CharacterController m_ch;           //角色控制器组件
    public float m_moveSpeed = 3;       //移动速度
    public float m_gravity = 2;         //重力
    public int m_life = 5;              //生命值
    Transform m_camera;                 //摄像机位置组件
    Vector3 m_camRotation;              //摄像机旋转角度
    public float m_camHeight = 1.4f;    //摄像机高度
    Transform m_muzzlePoint;            //枪口位置
    public LayerMask m_layer;           //射击判定碰撞层
    public Transform m_fx;              //射击特效
    public AudioClip m_shootAudio;      //射击音效
    public float m_shootTime = 0.1f;    //射击间隔
    float m_shootTimer = 0;             //射击计时器

    private void Start()
    {
        //初始化，获取相应组件
        m_transform = this.transform;
        m_ch = GetComponent<CharacterController>();

        //通过Camera.main获得摄像机位置组件
        m_camera = Camera.main.transform;
        //使摄像机的位置与主角一致并固定高度
        m_camera.position = m_transform.TransformPoint(0, m_camHeight, 0);
        //初始化摄像机的旋转与主角一致
        m_camera.rotation = m_transform.rotation;
        m_camRotation = m_camera.eulerAngles;

        //锁定鼠标（书上的方法Screen.lockCursor被标记为过时，该方法不确定是否相同效果）
        Cursor.lockState = CursorLockMode.Locked;

        //通过代码获得枪口位置
        m_muzzlePoint = m_camera.Find("M16/weapon/muzzlepoint").transform;
    }

    private void Update()
    {
        //生命值为0时什么也不做
        if (m_life == 0)
            return;

        //射击计时
        m_shootTimer += Time.deltaTime;
        //射击
        if(Input.GetMouseButton(0) && m_shootTimer >= m_shootTime)
        {
            m_shootTimer = 0;//重置计时器
            //播放射击音效
            this.GetComponent<AudioSource>().PlayOneShot(m_shootAudio);
            //更新子弹数
            GameManager.instance.SetAmmo(1);

            //通过射线检测是否碰撞到敌人或场景
            RaycastHit info;
            bool hit = Physics.Raycast(m_camera.position, 
                m_camera.TransformDirection(Vector3.forward), out info, 100, m_layer);
            if(hit)
            {
                //打中敌人则计算伤害并添加粒子效果
                if(info.transform.tag == "Enemy")
                {
                    Enemy enemy = info.transform.GetComponent<Enemy>();
                    enemy.OnDamage(1);
                }

                Instantiate(m_fx, info.point, info.transform.rotation);
            }
        }

        Control();//角色移动
    }

    //控制主角移动的方法
    void Control()
    {
        //获得鼠标的移动距离
        float rh = Input.GetAxis("Mouse X");
        float rv = Input.GetAxis("Mouse Y");
        //计算摄像机旋转角度
        m_camRotation.x -= rv;
        m_camRotation.y += rh;
        //修改摄像机旋转
        m_camera.eulerAngles = m_camRotation;
        //保持主角方向与摄像机一致
        Vector3 camRotation = m_camera.eulerAngles;
        camRotation.x = 0;
        camRotation.z = 0;
        m_transform.eulerAngles = camRotation;

        //创建一个移动的方向
        //x轴为水平方向，z轴垂直方向，y轴重力
        //GetAxis方法获取按键输入的轴向，取值为-1~1.
        //重力手动实现，角色控制器会自动碰撞检测，不会一直下落
        Vector3 motion = Vector3.zero;
        motion.x = Input.GetAxis("Horizontal") * m_moveSpeed * Time.deltaTime;
        motion.z = Input.GetAxis("Vertical") * m_moveSpeed * Time.deltaTime;
        motion.y -= m_gravity * Time.deltaTime;

        //将方向转换为角色的本地坐标方向，然后用角色控制器提供的Move方法移动
        m_ch.Move(m_transform.TransformDirection(motion));

        //保持摄像机的位置与主角一致
        m_camera.position = m_transform.TransformPoint(0, m_camHeight, 0);
    
        /*
         * 方法中的旋转是主角随摄像机改变，而位置是摄像机随主角改变
         * 键盘控制主角位置，摄像机跟随
         * 鼠标控制摄像机方向，主角跟随
         */
    }

    //受到伤害
    public void OnDamage(int damage)
    {
        m_life -= damage;
        //更新UI
        GameManager.instance.SetLife(m_life);

        //取消锁定鼠标光标
        if(m_life <= 0)
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void OnDrawGizmos()
    {
        //在Scene界面将主角显示为图标
        Gizmos.DrawIcon(m_transform.position, "Spawn.tif");
    }
}
