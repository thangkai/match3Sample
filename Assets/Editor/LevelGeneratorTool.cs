using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

public class LevelGeneratorTool
{
    [MenuItem("Tools/Generate 100 Image Levels")]
    public static void GenerateLevels()
    {
        string spriteFolder = "Assets/Sprites";
        string levelFolder = "Assets/Levels";

        var sprites = AssetDatabase.FindAssets("t:Sprite", new[] { spriteFolder });

        List<LevelReference> levelRefs = new List<LevelReference>();

        for (int i = 0; i < 100; i++)
        {
            string spritePath = AssetDatabase.GUIDToAssetPath(
                sprites[i % sprites.Length]);

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

            ImageLevelData levelData =
                ScriptableObject.CreateInstance<ImageLevelData>();

            levelData.levelIndex = i + 1;
            levelData.levelSprite = sprite;

            string assetPath = $"{levelFolder}/Level_{i + 1}.asset";

            AssetDatabase.CreateAsset(levelData, assetPath);

            AssetDatabase.SaveAssets();

            AddressableAssetSettings settings =
                AddressableAssetSettingsDefaultObject.Settings;

            var guid = AssetDatabase.AssetPathToGUID(assetPath);

            var entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup);

            levelRefs.Add(new LevelReference
            {
                levelIndex = i + 1,
                levelAsset = new AssetReferenceT<ImageLevelData>(guid)
            });
        }

        Debug.Log("Generated 100 Levels!");
    }
}