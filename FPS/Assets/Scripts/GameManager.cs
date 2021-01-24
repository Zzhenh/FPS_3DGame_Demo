using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;//自身实例

    public int m_score = 0;             //分数
    public static int m_hiscore = 0;    //最高分
    public int m_ammo = 100;            //弹匣容量
    Player m_player;                    //玩家实例

    //UI组件
    Text text_life;
    Text text_ammo;
    Text text_score;
    Text text_hiscore;
    Button button_restart;

    private void Start()
    {
        //单例模式
        instance = this;

        //初始化
        m_player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        
        //通过名称遍历查找组件
        GameObject uiCanvas = GameObject.Find("Canvas");
        //GetComponent和GetComponects不同，区别是返回一个和返回多个
        foreach(Transform t in uiCanvas.transform.GetComponentsInChildren<Transform>())
        {
            if(t.name.CompareTo("Text_Ammo") == 0)
            {
                text_ammo = t.GetComponent<Text>();
            }
            else if (t.name.CompareTo("Text_Life") == 0)
            {
                text_life = t.GetComponent<Text>();
            }
            else if (t.name.CompareTo("Text_Score") == 0)
            {
                text_score = t.GetComponent<Text>();
            }
            else if (t.name.CompareTo("Text_Hiscore") == 0)
            {
                text_hiscore = t.GetComponent<Text>();
            }
            else if(t.name.CompareTo("Button_Restart") == 0)
            {
                button_restart = t.GetComponent<Button>();
                //添加回调方法
                button_restart.onClick.AddListener(delegate ()
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                });
                //隐藏显示
                button_restart.gameObject.SetActive(false);
            }
        }
    }

    //更新分数和最高分
    public void SetScore(int score)
    {
        m_score += score;
        if (m_hiscore < m_score)
            m_hiscore = m_score;

        text_score.text = "Score <color=yellow>" + m_score + "</color>";
        text_hiscore.text = "Hiscore" + m_hiscore;
    }

    //更新子弹数
    public void SetAmmo(int ammo)
    {
        m_ammo -= ammo;
        if(m_ammo <= 0)
        {
            m_ammo = 100 - m_ammo;
        }
        text_ammo.text = m_ammo.ToString() + "/100";
    }

    //更新生命值
    public void SetLife(int life)
    {
        text_life.text = life.ToString();
        if(life <= 0)
        {
            button_restart.gameObject.SetActive(true);
        }
    }
}
