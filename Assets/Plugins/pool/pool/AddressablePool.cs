using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace PoolLib
{
    /// <summary>
    /// Pool load Prefab từ Addressable — không cần giữ reference Prefab trong scene.
    /// Load async, sau khi ready mới có thể Get().
    ///
    /// USAGE:
    ///   var pool = new AddressablePool("prefabs/bullet", parent, host, initialSize: 5);
    ///   await pool.InitializeAsync();
    ///
    ///   var go = pool.Get(position, rotation);
    ///   pool.Return(go);
    ///   pool.Get(position, rotation, autoReturnAfter: 2f);
    /// </summary>
    public class AddressablePool
    {
        public string AddressableKey { get; }
        public bool IsReady          { get; private set; }
        public int CountInactive     => _pool.Count;
        public int CountActive       => _active.Count;

        private readonly string _key;
        private readonly Transform _parent;
        private readonly MonoBehaviour _coroutineHost;
        private readonly int _initialSize;

        private GameObject _prefab;
        private AsyncOperationHandle<GameObject> _loadHandle;
        private readonly Stack<GameObject> _pool   = new();
        private readonly HashSet<GameObject> _active = new();

        public AddressablePool(
            string addressableKey,
            Transform parent,
            MonoBehaviour coroutineHost,
            int initialSize = 5)
        {
            _key           = addressableKey;
            AddressableKey = addressableKey;
            _parent        = parent;
            _coroutineHost = coroutineHost;
            _initialSize   = initialSize;
        }

        // ─── Init ─────────────────────────────────────────────────

        /// <summary>Load prefab và warm up pool. Gọi 1 lần khi khởi động.</summary>
        public async Task InitializeAsync()
        {
            if (IsReady) return;

            _loadHandle = Addressables.LoadAssetAsync<GameObject>(_key);
            await _loadHandle.Task;

            if (_loadHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[AddressablePool] Failed to load: {_key}");
                return;
            }

            _prefab = _loadHandle.Result;
            WarmUp(_initialSize);
            IsReady = true;

            Debug.Log($"[AddressablePool] Ready: {_key} ({_initialSize} pre-spawned)");
        }

        /// <summary>Coroutine version nếu không dùng async/await</summary>
        public IEnumerator InitializeCoroutine()
        {
            _loadHandle = Addressables.LoadAssetAsync<GameObject>(_key);
            yield return _loadHandle;

            if (_loadHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[AddressablePool] Failed to load: {_key}");
                yield break;
            }

            _prefab = _loadHandle.Result;
            WarmUp(_initialSize);
            IsReady = true;
        }

        // ─── Spawn ────────────────────────────────────────────────

        public GameObject Get(Vector3 position = default, Quaternion rotation = default, float autoReturnAfter = 0f)
        {
            if (!IsReady)
            {
                Debug.LogError($"[AddressablePool:{_key}] Not ready yet — call InitializeAsync() first");
                return null;
            }

            var go = _pool.Count > 0 ? _pool.Pop() : CreateNew();
            go.transform.SetPositionAndRotation(position, rotation);
            go.SetActive(true);
            _active.Add(go);

            foreach (var poolable in go.GetComponentsInChildren<IPoolable>(true))
                poolable.OnSpawn();

            if (autoReturnAfter > 0f)
                _coroutineHost.StartCoroutine(AutoReturnRoutine(go, autoReturnAfter));

            return go;
        }

        public T Get<T>(Vector3 position = default, Quaternion rotation = default, float autoReturnAfter = 0f)
            where T : Component
            => Get(position, rotation, autoReturnAfter)?.GetComponent<T>();

        // ─── Return ───────────────────────────────────────────────

        public bool Return(GameObject go)
        {
            if (go == null || !_active.Contains(go)) return false;

            foreach (var poolable in go.GetComponentsInChildren<IPoolable>(true))
                poolable.OnDespawn();

            go.SetActive(false);
            go.transform.SetParent(_parent);
            _active.Remove(go);
            _pool.Push(go);
            return true;
        }

        public bool Return(Component component) => Return(component?.gameObject);

        public void ReturnAll()
        {
            var toReturn = new List<GameObject>(_active);
            foreach (var go in toReturn) Return(go);
        }

        // ─── Utility ─────────────────────────────────────────────

        public void WarmUp(int count)
        {
            for (int i = 0; i < count; i++)
                _pool.Push(CreateNew());
        }

        public void Dispose()
        {
            ReturnAll();
            while (_pool.Count > 0)
                Object.Destroy(_pool.Pop());

            if (_loadHandle.IsValid())
                Addressables.Release(_loadHandle);

            IsReady = false;
        }

        // ─── Private ─────────────────────────────────────────────

        private GameObject CreateNew()
        {
            var go = Object.Instantiate(_prefab, _parent);
            go.SetActive(false);
            return go;
        }

        private IEnumerator AutoReturnRoutine(GameObject go, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_active.Contains(go)) Return(go);
        }
    }
}
