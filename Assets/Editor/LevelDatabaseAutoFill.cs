using UnityEngine;
using UnityEditor;
using UnityEngine.AddressableAssets;
using System.Linq;
using System.Collections.Generic;

public class LevelDatabaseAutoFill
{
    [MenuItem("Tools/Auto Fill Level Database")]
    public static void FillDatabase()
    {
        string[] guids = AssetDatabase.FindAssets("t:ImageLevelData");

        List<ImageLevelData> levels = new List<ImageLevelData>();

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var level = AssetDatabase.LoadAssetAtPath<ImageLevelData>(path);
            levels.Add(level);
        }

        levels = levels.OrderBy(l => l.levelIndex).ToList();

        LevelDatabase database = Selection.activeObject as LevelDatabase;

        if (database == null)
        {
            Debug.LogError("Please select LevelDatabase asset first!");
            return;
        }

        database.levels = new LevelReference[levels.Count];

        for (int i = 0; i < levels.Count; i++)
        {
            string path = AssetDatabase.GetAssetPath(levels[i]);
            string guid = AssetDatabase.AssetPathToGUID(path);

            database.levels[i] = new LevelReference
            {
                levelIndex = levels[i].levelIndex,
                levelAsset = new AssetReferenceT<ImageLevelData>(guid)
            };
        }

        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();

        Debug.Log($"Database filled with {levels.Count} levels!");
    }
}