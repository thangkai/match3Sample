using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class LevelStreamingManager<T> : MonoBehaviour where T : class, ILevelData
{
    public static LevelStreamingManager<T> Instance;

    [SerializeField] private LevelDatabase database;

    private LevelLoader loader = new LevelLoader();
    private LevelCache<T> cache = new LevelCache<T>();

    private Queue<int> preloadQueue = new Queue<int>();

    private int currentLevel;

    [SerializeField]
    private int preloadCount = 3;

    public T CurrentLevelData { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public async UniTask Initialize(int startLevel)
    {
        currentLevel = startLevel;

        await PreloadLevels(startLevel);

        LoadCurrentFromCache();
    }

    async UniTask PreloadLevels(int startLevel)
    {
        for (int i = 0; i <= preloadCount; i++)
        {
            int levelIndex = startLevel + i;

            await LoadLevelInternal(levelIndex);

            preloadQueue.Enqueue(levelIndex);
        }
    }

    async UniTask LoadLevelInternal(int levelIndex)
    {
        if (cache.Contains(levelIndex))
            return;

        var levelRef = database.GetLevel(levelIndex);

        var result = await loader.Load<T>(levelRef.levelAsset);

        cache.Add(levelIndex, result.Item1, result.Item2);
        Debug.Log("Load Level " + levelIndex);
      
    }

    void LoadCurrentFromCache()
    {
        cache.TryGet(currentLevel, out var levelData);

        CurrentLevelData = levelData;
    }

    public async UniTask NextLevel()
    {
        int oldLevel = preloadQueue.Dequeue();

        cache.Remove(oldLevel);
        Debug.Log("Unload Level " + oldLevel);
        currentLevel++;

        LoadCurrentFromCache();

        int newLevel = currentLevel + preloadCount;

        await LoadLevelInternal(newLevel);

        preloadQueue.Enqueue(newLevel);
    }
}