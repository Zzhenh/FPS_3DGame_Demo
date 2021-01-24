# FPS-第一人称射击游戏

### 游戏场景

**游戏场景**

1.打开准备好的工程文件
2.选择三个场景模型，添加Mesh Collider

### 主角

**角色控制器**

1.创建一个空物体，将tag设置为Player
2.添加Character Controller组件，调整碰撞体的位置和大小
3.添加Rigidbody组件，取消Use Gravity，选中Is Kinematic。
4.创建脚本Player.cs
```c#
	public Transform m_transform;       //自身位置组件
    CharacterController m_ch;           //角色控制器组件
    public float m_moveSpeed = 3;       //移动速度
    public float m_gravity = 2;         //重力
    public int m_life = 5;              //生命值

    private void Start()
    {
        //初始化，获取相应组件
        m_transform = this.transform;
        m_ch = GetComponent<CharacterController>();
    }

    private void Update()
    {
        //生命值为0时什么也不做
        if (m_life == 0)
            return;

        Control();//角色移动
    }

    //控制主角移动的方法
    void Control()
    {
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
    }

    private void OnDrawGizmos()
    {
        //在Scene界面将主角显示为图标
        Gizmos.DrawIcon(m_transform.position, "Spawn.tif");
    }
```

**摄像机**

1.修改Player脚本，使摄像机跟随主角移动
```c#
	Transform m_camera;                 //摄像机位置组件
    Vector3 m_camRotation;              //摄像机旋转角度
    public float m_camHeight = 1.4f;    //摄像机高度
    
    //Start中的代码
		//通过Camera.main获得摄像机位置组件
        m_camera = Camera.main.transform;
        //使摄像机的位置与主角一致并固定高度
        m_camera.position = m_transform.TransformPoint(0, m_camHeight, 0);
        //初始化摄像机的旋转与主角一致
        m_camera.rotation = m_transform.rotation;
        m_camRotation = m_camera.eulerAngles;

        //锁定鼠标（书上的方法Screen.lockCursor被标记为过时，该方法不确定是否相同效果）
        Cursor.lockState = CursorLockMode.Locked;
    //
    
    //Control方法中的代码
    	//获得鼠标的移动距离
        float rh = Input.GetAxis("Mouse X");
        float rv = Input.GetAxis("Mouse Y");
        //计算摄像机旋转角度
        m_camRotation.x -= rv;
        m_camRotation.y += rh;
        //修改摄像机旋转
        m_camera.eulerAngles = m_camRotation;
        //保持主角方向与摄像机一致
        //主角的方向只改变y轴旋转，因为主角不是随摄像机动，只是朝向摄像机的方向
        //游戏体的方向和视角（摄像机）的方向要区别开
        Vector3 camRotation = m_camera.eulerAngles;
        camRotation.x = 0;
        camRotation.z = 0;
        m_transform.eulerAngles = camRotation;
        
        //保持摄像机的位置与主角一致
        m_camera.position = m_transform.TransformPoint(0, m_camHeight, 0);
    
        /*
         * 方法中的旋转是主角随摄像机改变，而位置是摄像机随主角改变
         * 键盘控制主角位置，摄像机跟随
         * 鼠标控制摄像机方向，主角跟随
         */
    //
```

**武器**

1.将摄像机的位置和旋转角度都归0
2.将摄像机的Clipping Planes/ Near设为0.1，使其可以看到更近处的物体
3.在场景中找到武器，位置和旋转设置为0，设置为摄像机的子物体，调整枪模型的位置和角度

### 敌人

**寻路**

1.确保场景模型的Navigation Static被选中
2.打开Window-Navigation窗口。Bake窗口定义地形对寻路的影响。
- Agent Radius和Height是寻路者的半径和高度
- Max Slope是最大坡度，超过这个坡度就无法通过
- Step Height是楼梯的最大高度
- Drop Height表示寻路者可以跳落的最大高度
- Jump Distance表示寻路者的跳跃距离极限
3.设置好数值后点击Bake进行计算。
4.把敌人prefab加入场景
5.为敌人添加Nav Mesh Agent。Speed是最大运动速度，Angular Speed是最大旋转速度。勾选Open Agent Settings打开Agents窗口，设置Radius和Height，为寻路者的半径和高度。
6.创建敌人脚本Enemy
```c#
	Transform m_transform;          //自身位置组件
    Player m_player;                //主角实例
    NavMeshAgent m_agent;           //寻路组件
    public float m_movSpeed = 2.5f; //移动速度

    private void Start()
    {
        //初始化，寻找组件和实例
        m_transform = this.transform;
        m_player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        m_agent = GetComponent<NavMeshAgent>();
        //设置寻路速度和目标
        m_agent.speed = m_speed;
        m_agent.SetDestination(m_player.m_transform.position);
    }
```

**设置动画**

1.取消敌人Animator组件中的Apply Root Motion
2.Window-Animator打开Animator窗口。点击+创建4个bool类型的数值
3.设置对应的动画过渡条件

**行为**

1.修改敌人脚本
```c#
	public float m_rotSpeed = 5;    //旋转速度
    Animator m_anim;                //动画组件
    public float m_timer = 2;       //计时器时间
    public int m_life = 15;         //生命值
    
    //Start中代码
    m_anim = GetComponent<Animator>();
    //
    
    //保持始终面向主角（不是很懂这个计算方法）
    void RotateTo()
    {
        Vector3 targetDir = m_player.m_transform.position - m_transform.position;
        Vector3 newDir = Vector3.RotateTowards(m_transform.forward, targetDir, m_rotSpeed * Time.deltaTime, 0);
        m_transform.rotation = Quaternion.LookRotation(newDir);
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
            //攻击动画播放完切换到待机状态
            if(info.normalizedTime >=1.0f)
            {
                m_anim.SetBool("idle", true);
                m_time = 0;
            }
        }
    }
```
2.这里对动画的处理能理解大概逻辑，但是有一些细节不是很懂。

### UI界面

**UI界面**

1.创建UI-Image-GUI Texture的2D图像控件，引用Health.png做贴图。
2.创建文字。txt_ammo弹药数量，txt_hiscore记录，txt_life生命值，txt_score得分。创建一个按钮Restart重新开始。
3.创建一个空游戏体GameManager，坐标设为0.创建脚本GameManager;
```c#
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
```

### 交互

**主角的射击**

1.修改Player脚本，添加射击功能。
```c#
	Transform m_muzzlePoint;            //枪口位置
    public LayerMask m_layer;           //射击判定碰撞层
    public Transform m_fx;              //射击特效
    public AudioClip m_shootAudio;      //射击音效
    public float m_shootTime = 0.1f;    //射击间隔
    float m_shootTimer = 0;             //射击计时器
    
    //Start中代码
    	//通过代码获得枪口位置
        m_muzzlePoint = m_camera.Find("M16/weapon/muzzlepoint").transform;
    //
    
    //Update中代码
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
            bool hit = Physics.Raycast(m_muzzlePoint.position, 
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
    //
    
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
```
2.添加两个碰撞层，enemy和level，分别指定给敌人和场景，再添加一个enemy的Tag
3.为主角添加AudioSource，将主角Player组件里的击中图层设置为enemy和level，表示可以击中的人和场景。将击中特效和声音特效给到对应的属性。
4.创建脚本AutoDestory，给到设计特效，让特效自动销毁。
```c#
	//一秒后自动删除射击特效
    public float m_destoryTime = 1;
    private void Start()
    {
        Destroy(this.gameObject, m_destoryTime);
    }
```

**敌人的进攻与死亡**

1.为敌人添加CapsuleCollider。如果需要精确碰撞检测，建模后，作为骨骼的子物体，取消显示。
2.修改Enemy脚本
```c#
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
    
    //Update中代码
    	//攻击动画结束后
    		m_player.OnDamage(1);//对主角造成伤害
    	//
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
            }
        }
    //
```

### 出生点

**出生点**

1.创建EnemySpawn脚本
```c#
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
        Gizmos.DrawIcon(m_transform.position, "item.png", true);
    }
```
2.修改Enemy脚本
```c#
	//初始化，记录生成器，生成敌人数加一
    public void Init(EnemySpawn spawn)
    {
        m_spawn = spawn;
        m_spawn.m_enemyCount++;
    }
    
    //敌人死亡后生成敌人数减一
    	//生成敌人数减一,在销毁自身后执行
        m_spawn.m_enemyCount--;
    //
```
4.创建一个空游戏体作为出生点并挂载EnemySpawn脚本，只作为Prefab

### 小地图

**小地图**

1.创建一个新的摄像机，使其在场景上方垂直向下，设置为Orthographic，取消透视并调整Size的值改变大小，调整Viewport Rect改变摄像机显示区域的位置和大小
2.创建一个球体dummy，材质设置为红色Self-Illumin/Diffuse，它将作为敌人的代替体只在小地图中显示，注意将球体的Sphere Collider取消。
3.创建一个新的Layer-Dummy，并设置球体的Layer为Dummy
4.将球体放入敌人的prefab中
5.主摄像机取消显示Dummy图层，小地图摄像机只显示Dummy图层和Level图层
6.使用相同的方法为主角创建一个不同颜色的Dummy球体
7.创建脚本MiniCamera，指定给小地图摄像机，使小地图摄像机的视图永远是正方形
```c#
	private void Start()
    {
        //屏幕分辨率比例
        float ratio = (float)Screen.width / (float)Screen.height;
        //摄像机视图永远是一个正方形
        //rect的前两个参数是XY参数，后两个参数是XY大小
        this.GetComponent<Camera>().rect = new Rect((1 - 0.2f), (1 - 0.2f * ratio), 0.2f, 0.2f * ratio);
    }
```
8.取消小地图摄像机的AudioListener。