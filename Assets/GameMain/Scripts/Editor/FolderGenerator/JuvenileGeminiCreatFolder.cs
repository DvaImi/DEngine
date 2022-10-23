// ========================================================
// 描述：
// 作者：JuvenileGemini 
// 创建时间：2022-04-10 10:12:12
// 版 本：1.0
// ========================================================

using System.IO;
using UnityEditor;
using UnityEngine;
namespace Juvenile.Editor
{
    public class JuvenileGeminiCreatFolder
    {
        const string gameMainfolder = "Assets/GameMain/";
        const string gameEntitiesFolder = gameMainfolder + "Entities";
        const string gameMaterialsFolder = gameMainfolder + "Materials";
        const string gameMeshsFolder = gameMainfolder + "Meshs";
        const string gameMusicFolder = gameMainfolder + "Music";
        const string gameScenesFolder = gameMainfolder + "Scenes";
        const string gameSoundsFolder = gameMainfolder + "Sounds";
        const string gameShadersFolder = gameMainfolder + "Shaders";
        const string gameSpritesFolder = gameMainfolder + "Sprites";
        const string gameTexturesFolder = gameMainfolder + "Textures";


        [MenuItem("JuvenileGemini/Generator/Folder Generator/Generator Standard")]
        public static void CreatAllFolder()
        {
            if (!Directory.Exists(gameEntitiesFolder))
            {
                Directory.CreateDirectory(gameEntitiesFolder);
            }
            if (!Directory.Exists(gameMaterialsFolder))
            {
                Directory.CreateDirectory(gameMaterialsFolder);
            }
            if (!Directory.Exists(gameMeshsFolder))
            {
                Directory.CreateDirectory(gameMeshsFolder);
            }
            if (!Directory.Exists(gameMusicFolder))
            {
                Directory.CreateDirectory(gameMusicFolder);
            }
            if (!Directory.Exists(gameScenesFolder))
            {
                Directory.CreateDirectory(gameScenesFolder);
            }
            if (!Directory.Exists(gameSoundsFolder))
            {
                Directory.CreateDirectory(gameSoundsFolder);
            }
            if (!Directory.Exists(gameShadersFolder))
            {
                Directory.CreateDirectory(gameShadersFolder);
            }
            if (!Directory.Exists(gameSpritesFolder))
            {
                Directory.CreateDirectory(gameSpritesFolder);
            }
            if (!Directory.Exists(gameTexturesFolder))
            {
                Directory.CreateDirectory(gameTexturesFolder);
            }
            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }
            AssetDatabase.Refresh();
        }
    }
}


