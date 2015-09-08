using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CE.iPhone.PList;

/// <summary>
/// Sprite 的详细信息
/// </summary>
public class SpriteData
{
    public string name { get; set; }
    public float x { get; set; }
    public float y { get; set; }
    public float width { get; set; }
    public float height { get; set; }
    public bool rotated { get; set; }
}
/// <summary>
/// 读取Plist文件信息
/// </summary>
public class PListFile
{
    public static List<SpriteData> Parse(string path)
    {
        PListRoot root = PListRoot.Load(path);

        PListDict dic = (PListDict)root.Root;

        if (dic.ContainsKey("frames") && dic.ContainsKey("metadata"))
        {

            //second child content :the big picture information
            PListDict metaDic = (PListDict)dic["metadata"];

            PListString wh = (PListString)metaDic["size"];

            int w = GetFirstInter(wh);
            int h = GetSecondInter(wh);

            //-------------------------------------------------------------

            //first child content
            PListDict framesDic = (PListDict)dic["frames"];

            IEnumerable<string> keys = framesDic.Keys;

            List<SpriteData> sdList = new List<SpriteData>();

            foreach (string k in keys)
            {
                PListDict frameDic = (PListDict)framesDic[k];

                PListString temp = (PListString)frameDic["frame"];

                int x = GetFirstInter(temp);
                int y = GetSecondInter(temp);

                PListBool rotated = (PListBool)frameDic["rotated"];
                bool needRotate = rotated.Value;

                int width = !needRotate ? GetThirdInter(temp) : GetForthInter(temp);
                int height = !needRotate ? GetForthInter(temp) : GetThirdInter(temp);


                SpriteData sd = new SpriteData();
                sd.name = k;
                sd.x = x;
                sd.y = h - height - y;
                sd.width = width;
                sd.height = height;
                sd.rotated = rotated;

                sdList.Add(sd);
            }

            return sdList;
        }
        else
        {
            Debug.LogError("Not a Valid plist file:" + path);
            return null;
        }
    }

    /// <summary>
    /// 获取字符串的第一个数值
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    static int GetFirstInter(string s) 
    {
        string[] t = s.Split(',');
        string v = t[0];

        string result = "";
        int i = 0;

        while (v[i] < '0' || v[i] > '9') 
        {
            i++;
        }

        while (i < v.Length && v[i] >= '0' && v[i] <= '9') 
        {
            result += v[i];
            i++;
        }
        return int.Parse(result);
    }
    /// <summary>
    /// 获取字符串的第二个数值
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    static int GetSecondInter(string s) 
    {
        string[] t = s.Split(',');
        string v = t[1];

        string result = "";
        int i = 0;

        while (i < v.Length && v[i] >= '0' && v[i] <= '9')
        {
            result += v[i];
            i++;
        }
        return int.Parse(result);
    }

    static int GetThirdInter(string s) 
    {
        string[] t = s.Split(',');
        string v = t[2];

        string result = "";
        int i = 0;

        while (v[i] < '0' || v[i] > '9')
        {
            i++;
        }

        while (i < v.Length && v[i] >= '0' && v[i] <= '9')
        {
            result += v[i];
            i++;
        }
        return int.Parse(result);
    }

    static int GetForthInter(string s) 
    {
        string[] t = s.Split(',');
        string v = t[3];

        string result = "";
        int i = 0;

        while (i < v.Length && v[i] >= '0' && v[i] <= '9')
        {
            result += v[i];
            i++;
        }
        return int.Parse(result);
    }
}
