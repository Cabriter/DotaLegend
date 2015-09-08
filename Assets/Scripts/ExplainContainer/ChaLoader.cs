using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

#region 骨骼动画所有的数据结构

/// <summary>
/// 骨骼信息
/// </summary>
public class BoneS 
{
    public int nameLength;
    public string name;
    public int textureNameLength;
    public string textureName;
    public int index;//可以找到每个骨骼具体的数据（boneData）
}

/// <summary>
/// 对应骨骼的详细信息
/// </summary>
public class boneData 
{
    public short boneIndex;
    public byte transparent;
    public float[] data = new float[6];
}

/// <summary>
/// 一帧所含的数据，类型和骨骼数据
/// </summary>
public class frame 
{
    public int type;
    public List<boneData> boneList = new List<boneData>();
}

/// <summary>
/// 一个动画中含有所有帧的数据
/// </summary>
public class sAnimation 
{
    public string name;
    public List<frame> frames = new List<frame>();
}

/// <summary>
/// 一个Cha文件的信息，名字，所有的动画信息，所有的骨骼信息
/// </summary>
public class ChaFile 
{
    public string name;
    public List<sAnimation> animationList = new List<sAnimation>();
    public List<BoneS> boneList = new List<BoneS>();

}
#endregion

/// <summary>
/// 解析Cha文件
/// </summary>
public class ChaLoader
{

    public ChaFile Parse(string path)
    {
        FileInfo sourceFile = null;
        sourceFile = new FileInfo(path);
        if (sourceFile == null || !sourceFile.Exists)
        {
            Debug.LogError("File not exist or error:" + path);
            return null;
        }


        BinaryReader reader = new BinaryReader(sourceFile.OpenRead());

        ChaFile cf = new ChaFile();

        #region Read ChaFile

        //1.4 bytes,length of the name
        int cfNameLength = reader.ReadInt32();

        //2.name
        char[] name = new char[cfNameLength];
        reader.Read(name, 0, cfNameLength);
        cf.name = new string(name);
        //3.bone count
        int cfBoneNumber = reader.ReadInt32();

        //4.all bone infomation
        for (int i = 0; i < cfBoneNumber; i++)
        {
            BoneS bs = new BoneS();

            #region Read BoneS

            //4.1 bone name Length
            int boneNameLength = reader.ReadInt32();
            #region 其他方法
            //    reader.Read(boneNameLength, 0, 4);
            //    int nameLength = boneNameLength + (boneNameLength[1] << 8) + (boneNameLength[2] << 16) + (boneNameLength[3] << 24);
            #endregion
            bs.nameLength = boneNameLength;

            //bone name 
            char[] boneName = new char[boneNameLength];
            reader.Read(boneName, 0, boneNameLength);
            bs.name = new string(boneName);

            //texture name length
            int textureNameLength = reader.ReadInt32();
            bs.textureNameLength = textureNameLength;

            //texture name
            char[] textureName = new char[textureNameLength];
            reader.Read(textureName, 0, textureNameLength);
            bs.textureName = new string(textureName);

            //bone id
            int id = reader.ReadInt32();
            bs.index = id;
            //add
            cf.boneList.Add(bs);
            #endregion
        }

        int cfAniamtionNumber = reader.ReadInt32();

        for (int i = 0; i < cfAniamtionNumber; i++)
        {
            sAnimation sa = new sAnimation();

            #region Read sAnimation

            cf.animationList.Add(sa);

            //name length
            int nameLength = reader.ReadInt32();

            //name
            char[] animationName = new char[nameLength];
            reader.Read(animationName, 0, nameLength);
            sa.name = new string(animationName);

            //4bytes unknovn.0Xc0
            byte[] c = reader.ReadBytes(4);

            //frame count
            int frameCount = reader.ReadInt32();

            //frame
            for (int index = 0; index < frameCount; index++)
            {
                frame f = new frame();

                #region Read frame

                sa.frames.Add(f);

                //frame type
                int ft = reader.ReadInt32();
                f.type = ft;

                #region 处理掉不需要的数据
                if (ft == 1)
                {
                    int ftTag = reader.ReadInt32();
                    int ftNameLength = reader.ReadInt32();
                    if (ft == 0)
                    {
                        byte[] ftName = reader.ReadBytes(4);
                    }
                    else
                    {
                        byte[] ftName = reader.ReadBytes(ftNameLength);
                    }

                    byte[] ftAudioInfo = reader.ReadBytes(32);

                    if (ft == 1)
                    {
                        ftTag = reader.ReadInt32();
                    }
                }
                #endregion

                //bone data count
                int bc = reader.ReadInt32();

                for (int j = 0; j < bc; j++)
                {
                    boneData bd = new boneData();

                    #region Read boneData

                    f.boneList.Add(bd);

                    //bone index
                    byte[] bIndex = reader.ReadBytes(2);
                    bd.boneIndex = (short)(bIndex[0] + (bIndex[1] << 8));

                    //bone transparent
                    bd.transparent = reader.ReadByte();

                    //bone data(float:6)
                    bd.data[0] = reader.ReadSingle();
                    bd.data[1] = reader.ReadSingle();
                    bd.data[2] = reader.ReadSingle();
                    bd.data[3] = reader.ReadSingle();
                    bd.data[4] = reader.ReadSingle();
                    bd.data[5] = reader.ReadSingle();

                    //offset
                    bd.data[1] *= -1.0f;
                    bd.data[2] *= -1.0f;
                    bd.data[4] *= 0.1f * 4f;
                    bd.data[5] *= -0.1f * 4f;

                    #endregion

                }
                #endregion

            }
            #endregion

        }
        #endregion 
        return cf;
    }
}
