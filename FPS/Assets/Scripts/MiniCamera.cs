using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniCamera : MonoBehaviour
{
    private void Start()
    {
        //屏幕分辨率比例
        float ratio = (float)Screen.width / (float)Screen.height;
        //摄像机视图永远是一个正方形
        //rect的前两个参数是XY参数，后两个参数是XY大小
        this.GetComponent<Camera>().rect = new Rect((1 - 0.2f), (1 - 0.2f * ratio), 0.2f, 0.2f * ratio);
    }
}
