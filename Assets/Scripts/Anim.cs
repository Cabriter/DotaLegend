using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Anim : MonoBehaviour
{
    string resourceFolder;
    string folder;

    public Sprite[] heroSprites;

    ChaFile animation;

    public bool playOnce { get; set; }

    sAnimation currentAnimation = null;

    float currentTime = 0;
    readonly float frameSpeed = 0.05f;


    /// <summary>
    /// 索引所有的子图
    /// </summary>
    Dictionary<string, GameObject> spriteDic = new Dictionary<string, GameObject>();
    /// <summary>
    /// 索引所有的动画
    /// </summary>
    Dictionary<string, sAnimation> animations = new Dictionary<string, sAnimation>();

    void Awake()
    {
        ///初始化加载目录
        resourceFolder = "Resources/Animation/";
        resourceFolder += gameObject.name + "/";

        folder = "Animation" + "/";
        folder += gameObject.name + "/";
        string name = folder + "sheet";

        heroSprites = Resources.LoadAll<Sprite>(name);

    }

    void Start() 
    {
        //加载资源
        foreach (Sprite s in heroSprites)
        {
            GameObject go = new GameObject();
            go.name = s.name.Substring(0, s.name.IndexOf('.'));
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = s;

            go.transform.parent = this.transform;

            //存进字典方便查找
            spriteDic[go.name] = go;

            //加载旋转信息
            SpriteInfo si = go.AddComponent<SpriteInfo>();

            List<SpriteData> sd = PListFile.Parse(Application.dataPath + "/" + resourceFolder + "plist");

            for (int i = 0; i < sd.Count; i++)
            {
                if (sd[i].name.Equals(s.name))
                    if (sd[i].rotated == true)
                    {
                        si.needRotated = true;
                    }
            }

            //load animation file
            string path = Application.dataPath + "/" + resourceFolder + "cha";
            ChaLoader cl = new ChaLoader();
            animation = cl.Parse(path);

            //组织animation到Dic,方便索引 dictionary for fast search
            for (int i = 0; i < animation.animationList.Count; i++) 
            {
                animations[animation.animationList[i].name] = animation.animationList[i];
            }
        }

    }

    void Update() 
    {
        currentTime += Time.deltaTime;

        int frm = (int)(currentTime / frameSpeed);//当前播放的帧数

        if (currentAnimation == null) return;//没动画就跳出

        if (currentAnimation.frames.Count <= frm) //当前帧是最后一帧的话善后
        {
            if (playOnce) return;//如果是重复播放就重置值
            frm = 0;
            currentTime = 0;
        }

        foreach (KeyValuePair<string, GameObject> g in spriteDic) //全部骨骼隐藏
        {
            g.Value.SetActive(false);
        }

        frame currentFrame = currentAnimation.frames[frm];//当前帧的信息

        List<boneData> boneList = currentFrame.boneList;//当前帧所有骨骼的信息

        for (int i = 0; i < boneList.Count; i++)
        {
            float a = boneList[i].data[0];
            float b = boneList[i].data[1];
            float c = boneList[i].data[2];
            float d = boneList[i].data[3];
            float tx = boneList[i].data[4];
            float ty = boneList[i].data[5];
            //float a, b, c, d, tx, ty;   //test

            int index = boneList[i].boneIndex;

            string spriteName = string.Empty;

            for (int j = 0; j < animation.boneList.Count; j++) 
            {
                if (animation.boneList[j].index == index) 
                {
                    spriteName = animation.boneList[j].textureName;
                    spriteDic[spriteName].SetActive(true);
                    break;
                }
            }



            //构建矩阵，萃取位置，旋转和缩放信息
            Matrix4x4 matrix = new Matrix4x4();

            matrix.m00 = a; matrix.m01 = c; matrix.m02 = 0; matrix.m03 = tx;
            matrix.m10 = b; matrix.m11 = d; matrix.m12 = 0; matrix.m13 = ty;
            matrix.m20 = 0; matrix.m21 = 0; matrix.m22 = 1; matrix.m23 = 0;
            matrix.m30 = 0; matrix.m31 = 0; matrix.m32 = 0; matrix.m33 = 1;

            //calc position
            Vector3 pos = matrix.MultiplyPoint(Vector3.zero);

            //calc rotation
            Vector3 rotation = matrix.MultiplyPoint(new Vector3(0, 1, 0));
            rotation.x -= pos.x;
            rotation.y -= pos.y;

            float degree = GetAngle(rotation, new Vector2(0, 1));
            float angle = degree * 180f / Mathf.PI;

            //calc scale
            float sx = Mathf.Sign(a) * Mathf.Sqrt(Mathf.Pow(a, 2) + Mathf.Pow(b, 2));
            float sy = Mathf.Sign(d) * Mathf.Sqrt(Mathf.Pow(c, 2) + Mathf.Pow(d, 2));

            //赋值
            spriteDic[spriteName].transform.localPosition = new Vector3(pos.x / 100, pos.y / 100);
            spriteDic[spriteName].transform.localScale = new Vector3(sx,sy, 1);
            SpriteInfo si = spriteDic[spriteName].GetComponent<SpriteInfo>();
            float temp = si.needRotated ? 90f : 0;
            spriteDic[spriteName].transform.localEulerAngles = new Vector3(0, 0, 360 - angle + temp);

            SpriteRenderer sr = spriteDic[spriteName].GetComponent<SpriteRenderer>();
            sr.sortingLayerName = i.ToString();//控制渲染层级
        }

    }

    public void PlayAnimation(string name) 
    {
        playOnce = true;

        if (animations.ContainsKey(name)) 
        {
            sAnimation sa = animations[name];
            if (sa != currentAnimation) 
            {
                currentAnimation = sa;
            }
        }


    }

    public void PlayAnimationLoop(string name) 
    {
        playOnce = false;
        if (animations.ContainsKey(name)) 
        {
            sAnimation sa = animations[name];
            if (sa != currentAnimation) 
            {
                currentAnimation = sa;
            }
        }
    }

    float GetAngle(Vector2 lhs, Vector2 rhs) 
    {
        float c = Cross(rhs, lhs);
        float d = Dot(rhs, lhs);
        float angle = Mathf.Atan2(c, d);

        if (Mathf.Abs(angle) < Mathf.Epsilon) 
        {
            return 0.0f;
        }

        return angle;
    }

    /// <summary>
    /// 叉积
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    float Cross(Vector2 lhs, Vector2 rhs) 
    {
        return lhs.x * rhs.y - lhs.y * rhs.x;
    }

    /// <summary>
    /// 点积
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    float Dot(Vector2 lhs, Vector2 rhs) 
    {
        return lhs.x * rhs.x + lhs.y * rhs.y;
    }

}
