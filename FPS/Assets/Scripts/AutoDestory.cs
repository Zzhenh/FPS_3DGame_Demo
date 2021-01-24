using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestory : MonoBehaviour
{
    //一秒后自动删除射击特效
    public float m_destoryTime = 1;
    private void Start()
    {
        Destroy(this.gameObject, m_destoryTime);
    }
}
