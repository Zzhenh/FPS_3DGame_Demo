using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    public Transform m_enemy;           //敌人prefab
    public int m_enemyCount = 0;        //生成的敌人数
    public int m_maxEnemyCount = 3;     //最大敌人数
    public float m_enemyTime = 15;      //生成敌人最大间隔时间
    float m_enemyTimer = 0;             //生成敌人计时器
    protected Transform m_transform;    //自身位置组件

    private void Start()
    {
        //初始化
        m_transform = this.transform;
    }

    private void Update()
    {
        //当生成的敌人数达到最大时，不再生成
        if(m_enemyCount >= m_maxEnemyCount)
        {
            return;
        }

        m_enemyTimer += Time.deltaTime;

        if(m_enemyTimer >= m_enemyTime)
        {
            //间隔时间不等，在5到15秒
            m_enemyTimer = Random.value * m_enemyTime;
            if (m_enemyTimer > 10)
                m_enemyTimer = 10;

            //放置敌人prefab并进行初始化
            //初始化操作会记录敌人数
            Transform obj = Instantiate(m_enemy, m_transform.position, Quaternion.identity);
            Enemy enemy = obj.GetComponent<Enemy>();
            enemy.Init(this);
        }
    }

    //在场景中显示为图标
    //OnDrawGizmos中不能用m_transform（原理不明，于此记录）
    private void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "item.png", true);
    }
}
