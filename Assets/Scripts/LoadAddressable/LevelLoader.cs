using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LevelLoader
{
    // public async UniTask<(T, AsyncOperationHandle<T>)> Load<T>(AssetReference reference)
    // {
    //     var handle = reference.LoadAssetAsync<T>();
    //
    //     await handle;
    //
    //     if (handle.Status != AsyncOperationStatus.Succeeded)
    //     {
    //         Addressables.Release(handle);
    //         return (default, handle);
    //     }
    //
    //     return (handle.Result, handle);
    // }
    //
    
    public async UniTask<(T, AsyncOperationHandle<T>)> Load<T>(AssetReference reference)
    {
        var handle = reference.LoadAssetAsync<T>();

        await handle.ToUniTask();

        return (handle.Result, handle);
    }
}