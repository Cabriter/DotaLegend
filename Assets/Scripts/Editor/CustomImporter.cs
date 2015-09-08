using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 切割被导入的图集，根据PList
/// </summary>
public class CustomImporter : AssetPostprocessor
{
    /// <summary>
    /// 在有图片被导入进来的时候调用
    /// </summary>
    void OnPreprocessTexture() 
    {
        TextureImporter importer = assetImporter as TextureImporter;

        string folder = Path.GetDirectoryName(assetPath);

        string file = folder + "/plist";

        if (File.Exists(file)) 
        {
            UpdateSpriteMetaData(importer, file);
        }
    }

    /// <summary>
    /// 更新Sprite对应的Meta文件
    /// </summary>
    /// <param name="importer"></param>
    /// <param name="file"></param>
    void UpdateSpriteMetaData(TextureImporter importer, string file) 
    {
        if (importer.textureType == TextureImporterType.Advanced) 
        {
            importer.textureType = TextureImporterType.Sprite;
        }

        importer.maxTextureSize = 4096;

        importer.spriteImportMode = SpriteImportMode.Multiple;

        importer.textureFormat = TextureImporterFormat.RGBA32;

        //在路径中找到Plist文件，解析出SpriteData信息
        List<SpriteData> sdList = PListFile.Parse(file);

        List<SpriteMetaData> metaDataList = new List<SpriteMetaData>();

        for (int i = 0; i < sdList.Count; i++) 
        {
            SpriteMetaData smd = new SpriteMetaData();
            smd.name = sdList[i].name;
            float x = sdList[i].x;
            float y = sdList[i].y;
            float w = sdList[i].width;
            float h = sdList[i].height;

            smd.rect = new UnityEngine.Rect(x, y, w, h);

            smd.pivot = new UnityEngine.Vector2(0, 0);

            smd.alignment = (int)UnityEngine.SpriteAlignment.Center;

            metaDataList.Add(smd);
        }
        //将格式化后的数据送给Engine
        importer.spritesheet = metaDataList.ToArray();
    }


}
