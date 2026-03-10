using System;

using UnityEngine.AddressableAssets;

// [Serializable]
// public class LevelReference
// {
//     public int levelIndex;
//     public AssetReferenceT<ImageLevelData> levelAsset;
// }

[Serializable]
public struct LevelReference
{
    public int levelIndex;
    public AssetReference levelAsset;
}