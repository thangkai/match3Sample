using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LevelImageTester : MonoBehaviour
{
    public LevelDatabase database;
    public List<Image> targetImages;

    public int startLevel = 1;

    List<AsyncOperationHandle<ImageLevelData>> handles = new();

    void Start()
    {
        LoadLevels(startLevel, targetImages.Count);
    }

    // void LoadLevels(int startLevel, int count)
    // {
    //     List<AssetReference> references = new();
    //
    //     for (int i = 0; i < count; i++)
    //     {
    //         int levelIndex = startLevel + i;
    //
    //         var levelRef = database.GetLevel(levelIndex);
    //
    //         references.Add(levelRef.levelAsset);
    //     }
    //
    //     handle = Addressables.LoadAssetsAsync<ImageLevelData>(
    //         references,
    //         OnSingleLevelLoaded
    //     );
    //
    //     handle.Completed += OnAllLevelsLoaded;
    // }
    
    
    void LoadLevels(int startLevel, int count)
    {
        
        
        // Release handle cũ
        foreach (var handle in handles)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }

        handles.Clear();
        
        
        
        ////
        for (int i = 0; i < count; i++)
        {
            int levelIndex = startLevel + i;

            var levelRef = database.GetLevel(levelIndex);

            var handle = Addressables.LoadAssetAsync<ImageLevelData>(levelRef.levelAsset);

            int uiIndex = i;

            handle.Completed += (h) =>
            {
                if (h.Status == AsyncOperationStatus.Succeeded)
                {
                    targetImages[uiIndex].sprite = h.Result.levelSprite;
                    Debug.Log("Loaded Level: " + h.Result.levelIndex);
                }
            };

            handles.Add(handle);
        }
    }
    
    

    void OnSingleLevelLoaded(ImageLevelData level)
    {
        int index = level.levelIndex - startLevel;

        if (index >= 0 && index < targetImages.Count)
        {
            targetImages[index].sprite = level.levelSprite;
        }

        Debug.Log("Loaded Level: " + level.levelIndex);
    }

    void OnAllLevelsLoaded(AsyncOperationHandle<IList<ImageLevelData>> handle)
    {
        Debug.Log("All levels loaded!");
    }

    void OnDestroy()
    {
        foreach (var handle in handles)
        {
            if (handle.IsValid())
                Addressables.Release(handle);
        }
    }
}