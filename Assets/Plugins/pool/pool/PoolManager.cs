using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace PoolLib
{
    /// <summary>
    /// Quản lý tất cả pool tập trung.
    /// Hỗ trợ cả direct Prefab và Addressable.
    ///
    /// USAGE:
    ///   // Register pool
    ///   PoolManager.Instance.RegisterPrefab("bullet", bulletPrefab, initialSize: 10);
    ///   await PoolManager.Instance.RegisterAddressable("explosion", "prefabs/explosion");
    ///
    ///   // Spawn
    ///   PoolManager.Instance.Get("bullet", position, rotation);
    ///   PoolManager.Instance.Get<Bullet>("bullet", position);
    ///   PoolManager.Instance.Get("bullet", position, autoReturnAfter: 3f);
    ///
    ///   // Return
    ///   PoolManager.Instance.Return("bullet", gameObject);
    ///   PoolManager.Instance.ReturnAll("bullet");
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        private readonly Dictionary<string, GameObjectPool> _prefabPools     = new();
        private readonly Dictionary<string, AddressablePool> _addressablePools = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ─── Register ─────────────────────────────────────────────

        /// <summary>Đăng ký pool từ Prefab reference trực tiếp</summary>
        public GameObjectPool RegisterPrefab(string key, GameObject prefab, int initialSize = 5)
        {
            if (_prefabPools.ContainsKey(key))
            {
                Debug.LogWarning($"[PoolManager] Pool already exists: {key}");
                return _prefabPools[key];
            }

            var poolParent = new GameObject($"Pool_{key}").transform;
            poolParent.SetParent(transform);

            var pool = new GameObjectPool(prefab, poolParent, this, initialSize, key);
            _prefabPools[key] = pool;

            Debug.Log($"[PoolManager] Registered pool: {key} (size: {initialSize})");
            return pool;
        }

        /// <summary>Đăng ký pool từ Addressable key — async</summary>
        public async Task<AddressablePool> RegisterAddressable(string key, string addressableKey, int initialSize = 5)
        {
            if (_addressablePools.ContainsKey(key))
            {
                Debug.LogWarning($"[PoolManager] Addressable pool already exists: {key}");
                return _addressablePools[key];
            }

            var poolParent = new GameObject($"Pool_{key}").transform;
            poolParent.SetParent(transform);

            var pool = new AddressablePool(addressableKey, poolParent, this, initialSize);
            await pool.InitializeAsync();

            _addressablePools[key] = pool;
            return pool;
        }

        // ─── Spawn ────────────────────────────────────────────────

        public GameObject Get(string key, Vector3 position = default, Quaternion rotation = default, float autoReturnAfter = 0f)
        {
            if (_prefabPools.TryGetValue(key, out var prefabPool))
                return prefabPool.Get(position, rotation, autoReturnAfter);

            if (_addressablePools.TryGetValue(key, out var addrPool))
                return addrPool.Get(position, rotation, autoReturnAfter);

            Debug.LogError($"[PoolManager] Pool not found: {key}");
            return null;
        }

        public T Get<T>(string key, Vector3 position = default, Quaternion rotation = default, float autoReturnAfter = 0f)
            where T : Component
            => Get(key, position, rotation, autoReturnAfter)?.GetComponent<T>();

        // ─── Return ───────────────────────────────────────────────

        public bool Return(string key, GameObject go)
        {
            if (_prefabPools.TryGetValue(key, out var prefabPool))
                return prefabPool.Return(go);

            if (_addressablePools.TryGetValue(key, out var addrPool))
                return addrPool.Return(go);

            Debug.LogError($"[PoolManager] Pool not found: {key}");
            return false;
        }

        public void ReturnAll(string key)
        {
            if (_prefabPools.TryGetValue(key, out var prefabPool))   { prefabPool.ReturnAll(); return; }
            if (_addressablePools.TryGetValue(key, out var addrPool)) { addrPool.ReturnAll(); return; }
            Debug.LogError($"[PoolManager] Pool not found: {key}");
        }

        public void ReturnAllPools()
        {
            foreach (var pool in _prefabPools.Values)     pool.ReturnAll();
            foreach (var pool in _addressablePools.Values) pool.ReturnAll();
        }

        // ─── Utility ─────────────────────────────────────────────

        public bool HasPool(string key)
            => _prefabPools.ContainsKey(key) || _addressablePools.ContainsKey(key);

        public (int inactive, int active) GetStats(string key)
        {
            if (_prefabPools.TryGetValue(key, out var p))     return (p.CountInactive, p.CountActive);
            if (_addressablePools.TryGetValue(key, out var a)) return (a.CountInactive, a.CountActive);
            return (0, 0);
        }

        private void OnDestroy()
        {
            foreach (var pool in _addressablePools.Values)
                pool.Dispose();
        }
    }
}
