 using System.Collections.Generic;
// using UnityEngine;
//
// [CreateAssetMenu(menuName = "Game/Level Database")]
// public class LevelDatabase : ScriptableObject
// {
//     public LevelReference[] levels;
//
//     private Dictionary<int, LevelReference> levelMap;
//
//     void OnEnable()
//     {
//         Initialize();
//     }
//
//     void Initialize()
//     {
//         levelMap = new Dictionary<int, LevelReference>();
//
//         foreach (var level in levels)
//         {
//             if (levelMap.ContainsKey(level.levelIndex))
//             {
//                 Debug.LogError($"Duplicate level index {level.levelIndex}");
//                 continue;
//             }
//
//             levelMap.Add(level.levelIndex, level);
//         }
//     }
//
//     public LevelReference GetLevel(int index)
//     {
//         int wrappedIndex = (index - 1) % levels.Length;
//
//         return levels[wrappedIndex];
//     }
//
//     public int LevelCount => levels.Length;
// }



using UnityEngine;

[CreateAssetMenu(menuName = "Game/Level Database")]
public class LevelDatabase : ScriptableObject
{
    // public LevelReference[] levels;
    //
    // public LevelReference GetLevel(int index)
    // {
    //     int wrappedIndex = (index - 1) % levels.Length;
    //     return levels[wrappedIndex];
    // }
    //
    //
    
    public LevelReference[] levels;

    Dictionary<int, LevelReference> map;

    public void Initialize()
    {
        map = new Dictionary<int, LevelReference>();

        foreach (var level in levels)
        {
            map[level.levelIndex] = level;
        }
    }

    public LevelReference GetLevel(int index)
    {
        if (map == null)
            Initialize();

        int wrappedIndex = ((index - 1) % levels.Length) + 1;

        return map[wrappedIndex];
    }
}
