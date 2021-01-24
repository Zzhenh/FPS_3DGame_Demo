using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    Transform m_transform;          //自身位置组件
    Player m_player;                //主角实例
    NavMeshAgent m_agent;           //寻路组件
    public float m_movSpeed = 2.5f; //移动速度
    public float m_rotSpeed = 5;    //旋转速度
    Animator m_anim;                //动画组件
    public float m_timer = 1;       //计时器时间
    float m_time = 0;               //计时时间
    public int m_life = 15;         //生命值
    protected EnemySpawn m_spawn;

    private void Start()
    {
        //初始化，寻找组件和实例
        m_transform = this.transform;
        m_player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        m_agent = GetComponent<NavMeshAgent>();
        m_anim = GetComponent<Animator>();
        //设置寻路速度和目标
        m_agent.speed = m_movSpeed;
        m_agent.SetDestination(m_player.m_transform.position);
    }

    private void Update()
    {
        //当主角生命值为0时什么也不做
        if (m_player.m_life <= 0)
            return;
        
        //计时器
        m_time += Time.deltaTime;
        //动画的状态信息，通过GetCurrentAnimatorStateInfo方法可以获取当前动画
        AnimatorStateInfo info = m_anim.GetCurrentAnimatorStateInfo(0);
        
        //如果当前动画是待机动画且不在过渡状态
        if(info.fullPathHash == Animator.StringToHash("Base Layer.idle") 
            && !m_anim.IsInTransition(0))
        {

            m_anim.SetBool("idle", false);
            //待机时间
            if (m_time < m_timer)
                return;
            //距离主角1.5内时切换到攻击动画，否则切换到奔跑动画
            if (Vector3.Distance(m_transform.position, m_player.m_transform.position) < 1.5f)
            {
                m_agent.ResetPath();
                m_anim.SetBool("attack", true);
            }
            else
            {
                //重置定时器
                m_time = 0;
                //重新设置寻路
                m_agent.SetDestination(m_player.m_transform.position);
                m_anim.SetBool("run", true);
            }
        }
        //如果当前动画是跑步动画且不在过渡状态
        if (info.fullPathHash == Animator.StringToHash("Base Layer.run")
            && !m_anim.IsInTransition(0))
        {
            m_anim.SetBool("run", false);
            //每隔一段时间重新寻路并重置计时器
            if(m_time > m_timer)
            {
                m_agent.SetDestination(m_player.m_transform.position);
                m_time = 0;
            }
            //距离主角1.5内时切换到攻击动画
            if (Vector3.Distance(m_transform.position, m_player.m_transform.position) <= 1.5f)
            {
                m_agent.ResetPath();
                m_anim.SetBool("attack", true);
            }
        }
        //如果当前动画是攻击动画且不在过渡状态
        if (info.fullPathHash == Animator.StringToHash("Base Layer.attack")
            && !m_anim.IsInTransition(0))
        {
            //面向主角
            RotateTo();
            m_anim.SetBool("attack", false);
            //攻击动画播放完切换到待机状态,并对主角造成伤害
            if(info.normalizedTime >=1.0f)
            {
                m_anim.SetBool("idle", true);
                m_time = 0;
                m_player.OnDamage(1);
            }
        }
        //如果当前动画是死亡动画且不在过渡状态
        if (info.fullPathHash == Animator.StringToHash("Base Layer.death")
            && !m_anim.IsInTransition(0))
        {
            m_anim.SetBool("death", false);
            //死亡动画播放完加分并销毁自身
            if (info.normalizedTime >= 1.0f)
            {
                GameManager.instance.SetScore(100);
                Destroy(this.gameObject);
                //生成敌人数减一
                m_spawn.m_enemyCount--;
            }
        }
    }

    //初始化，记录生成器，生成敌人数加一
    public void Init(EnemySpawn spawn)
    {
        m_spawn = spawn;
        m_spawn.m_enemyCount++;
    }

    //受到伤害，生命值为0时播放死亡动画
    public void OnDamage(int damage)
    {
        m_life -= damage;

        if(m_life <= 0)
        {
            m_anim.SetBool("death", true);
            m_agent.ResetPath();
        }
    }

    //保持始终面向主角（不是很懂这个计算方法）
    void RotateTo()
    {
        Vector3 targetDir = m_player.m_transform.position - m_transform.position;
        Vector3 newDir = Vector3.RotateTowards(m_transform.forward, targetDir, m_rotSpeed * Time.deltaTime, 0);
        m_transform.rotation = Quaternion.LookRotation(newDir);
    }
}
