using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PoolLib
{
    /// <summary>
    /// Pool cho Unity GameObject/Prefab.
    /// Hỗ trợ auto-return sau X giây, IPoolable callback, auto-expand.
    ///
    /// USAGE:
    ///   var pool = new GameObjectPool(bulletPrefab, parent, initialSize: 10);
    ///
    ///   // Spawn
    ///   var go = pool.Get(position, rotation);
    ///
    ///   // Return thủ công
    ///   pool.Return(go);
    ///
    ///   // Auto-return sau 3 giây
    ///   pool.Get(position, Quaternion.identity, autoReturnAfter: 3f);
    /// </summary>
    public class GameObjectPool
    {
        public string PoolName      { get; }
        public int CountInactive    => _pool.Count;
        public int CountActive      => _active.Count;
        public int CountTotal       => CountInactive + CountActive;

        private readonly GameObject _prefab;
        private readonly Transform _parent;
        private readonly Stack<GameObject> _pool;
        private readonly HashSet<GameObject> _active;
        private readonly MonoBehaviour _coroutineHost; // để chạy auto-return coroutine

        public GameObjectPool(
            GameObject prefab,
            Transform parent,
            MonoBehaviour coroutineHost,
            int initialSize = 5,
            string poolName = null)
        {
            _prefab         = prefab;
            _parent         = parent;
            _coroutineHost  = coroutineHost;
            _pool           = new Stack<GameObject>(Mathf.Max(initialSize, 4));
            _active         = new HashSet<GameObject>();
            PoolName        = poolName ?? prefab.name;

            WarmUp(initialSize);
        }

        // ─── Spawn ────────────────────────────────────────────────

        /// <summary>Spawn object tại position/rotation</summary>
        public GameObject Get(Vector3 position = default, Quaternion rotation = default, float autoReturnAfter = 0f)
        {
            var go = _pool.Count > 0 ? _pool.Pop() : CreateNew();

            go.transform.SetPositionAndRotation(position, rotation);
            go.SetActive(true);
            _active.Add(go);

            // Gọi IPoolable.OnSpawn() trên tất cả component
            foreach (var poolable in go.GetComponentsInChildren<IPoolable>(true))
                poolable.OnSpawn();

            if (autoReturnAfter > 0f)
                _coroutineHost.StartCoroutine(AutoReturnRoutine(go, autoReturnAfter));

            return go;
        }

        /// <summary>Spawn với parent mới</summary>
        public GameObject Get(Transform parent, float autoReturnAfter = 0f)
        {
            var go = Get(parent.position, parent.rotation, autoReturnAfter);
            go.transform.SetParent(parent);
            return go;
        }

        /// <summary>Spawn và lấy component T luôn</summary>
        public T Get<T>(Vector3 position = default, Quaternion rotation = default, float autoReturnAfter = 0f)
            where T : Component
        {
            var go = Get(position, rotation, autoReturnAfter);
            return go.GetComponent<T>();
        }

        // ─── Return ───────────────────────────────────────────────

        public bool Return(GameObject go)
        {
            if (go == null || !_active.Contains(go)) return false;

            // Gọi IPoolable.OnDespawn() trước khi deactivate
            foreach (var poolable in go.GetComponentsInChildren<IPoolable>(true))
                poolable.OnDespawn();

            go.SetActive(false);
            go.transform.SetParent(_parent);
            _active.Remove(go);
            _pool.Push(go);
            return true;
        }

        public bool Return(Component component) => Return(component?.gameObject);

        /// <summary>Return tất cả object đang active</summary>
        public void ReturnAll()
        {
            var toReturn = new List<GameObject>(_active);
            foreach (var go in toReturn)
                Return(go);
        }

        // ─── Utility ─────────────────────────────────────────────

        public void WarmUp(int count)
        {
            for (int i = 0; i < count; i++)
                _pool.Push(CreateNew());
        }

        public void Clear()
        {
            ReturnAll();
            while (_pool.Count > 0)
                Object.Destroy(_pool.Pop());
        }

        public bool IsActive(GameObject go) => _active.Contains(go);

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
            if (_active.Contains(go)) // chưa bị return thủ công
                Return(go);
        }
    }
}
