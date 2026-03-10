using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LevelCache<T>
{
    
    
    Dictionary<int, T> cache = new();
    Dictionary<int, AsyncOperationHandle> handles = new();

    public void Add(int levelIndex, T data, AsyncOperationHandle handle)
    {
        cache[levelIndex] = data;
        handles[levelIndex] = handle;
    }

    public bool TryGet(int index, out T data)
    {
        return cache.TryGetValue(index, out data);
    }

    public bool Contains(int index)
    {
        return cache.ContainsKey(index);
    }

    public void Remove(int index)
    {
        if (handles.TryGetValue(index, out var handle))
        {
            UnityEngine.AddressableAssets.Addressables.Release(handle);
        }

        cache.Remove(index);
        handles.Remove(index);
    }
    // private Dictionary<int, (T, AsyncOperationHandle<T>)> cache =
    //     new Dictionary<int, (T, AsyncOperationHandle<T>)>();
    //
    // public void Add(int levelIndex, T data, AsyncOperationHandle<T> handle)
    // {
    //     cache[levelIndex] = (data, handle);
    // }
    //
    // public bool TryGet(int levelIndex, out T data)
    // {
    //     if (cache.TryGetValue(levelIndex, out var entry))
    //     {
    //         data = entry.Item1;
    //         return true;
    //     }
    //
    //     data = default;
    //     return false;
    // }
    //
    // public AsyncOperationHandle<T> GetHandle(int levelIndex)
    // {
    //     return cache[levelIndex].Item2;
    // }
    //
    // public void Remove(int levelIndex)
    // {
    //     if (!cache.ContainsKey(levelIndex))
    //         return;
    //
    //     Addressables.Release(cache[levelIndex].Item2);
    //     cache.Remove(levelIndex);
    // }
    //
    // public bool Contains(int levelIndex)
    // {
    //     return cache.ContainsKey(levelIndex);
    // }
}