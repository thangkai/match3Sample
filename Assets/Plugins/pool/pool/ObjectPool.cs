using System;
using System.Collections.Generic;
using UnityEngine;

namespace PoolLib
{
    /// <summary>
    /// Generic pool cho pure C# object (không phải GameObject).
    /// Dùng cho: particle data, damage numbers, AI state, v.v.
    ///
    /// USAGE:
    ///   var pool = new ObjectPool<BulletData>(
    ///       factory:   () => new BulletData(),
    ///       onSpawn:   b  => b.Reset(),
    ///       onDespawn: b  => b.Clear(),
    ///       initialSize: 10
    ///   );
    ///
    ///   var bullet = pool.Get();
    ///   pool.Return(bullet);
    ///   pool.Return(bullet, delay: 2f); // auto-return sau 2 giây (cần Coroutine host)
    /// </summary>
    public class ObjectPool<T> where T : class
    {
        public int CountInactive => _pool.Count;
        public int CountActive   => _countActive;
        public int CountTotal    => CountInactive + CountActive;

        private readonly Stack<T> _pool;
        private readonly Func<T> _factory;
        private readonly Action<T> _onSpawn;
        private readonly Action<T> _onDespawn;
        private readonly int _maxSize;
        private int _countActive;

        /// <param name="factory">Hàm tạo object mới khi pool rỗng</param>
        /// <param name="onSpawn">Gọi khi Get() — reset state</param>
        /// <param name="onDespawn">Gọi khi Return() — cleanup</param>
        /// <param name="initialSize">Số object tạo sẵn lúc đầu (warm up)</param>
        /// <param name="maxSize">0 = không giới hạn, >0 = giới hạn tối đa</param>
        public ObjectPool(
            Func<T> factory,
            Action<T> onSpawn   = null,
            Action<T> onDespawn = null,
            int initialSize     = 0,
            int maxSize         = 0)
        {
            _factory   = factory ?? throw new ArgumentNullException(nameof(factory));
            _onSpawn   = onSpawn;
            _onDespawn = onDespawn;
            _maxSize   = maxSize;
            _pool      = new Stack<T>(Mathf.Max(initialSize, 4));

            WarmUp(initialSize);
        }

        // ─── Public API ──────────────────────────────────────────

        /// <summary>Lấy object từ pool (hoặc tạo mới nếu rỗng)</summary>
        public T Get()
        {
            T item = _pool.Count > 0 ? _pool.Pop() : _factory();
            _countActive++;
            _onSpawn?.Invoke(item);

            if (item is IPoolable poolable)
                poolable.OnSpawn();

            return item;
        }

        /// <summary>Trả object về pool</summary>
        public bool Return(T item)
        {
            if (item == null) return false;

            // Nếu đã đạt maxSize thì bỏ object đi (không giữ lại)
            if (_maxSize > 0 && _pool.Count >= _maxSize)
            {
                _countActive = Mathf.Max(0, _countActive - 1);
                return false;
            }

            _onDespawn?.Invoke(item);

            if (item is IPoolable poolable)
                poolable.OnDespawn();

            _pool.Push(item);
            _countActive = Mathf.Max(0, _countActive - 1);
            return true;
        }

        /// <summary>Tạo sẵn N object vào pool</summary>
        public void WarmUp(int count)
        {
            for (int i = 0; i < count; i++)
                _pool.Push(_factory());
        }

        /// <summary>Xóa toàn bộ pool</summary>
        public void Clear()
        {
            _pool.Clear();
            _countActive = 0;
        }
    }
}
