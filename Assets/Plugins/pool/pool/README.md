# 🎱 PoolLib — Unity Object Pool System

Pool system cho cả C# object và Unity GameObject, hỗ trợ Addressable.

---

## 3 loại pool

| | ObjectPool\<T\> | GameObjectPool | AddressablePool |
|---|---|---|---|
| Dùng cho | Pure C# class | Prefab reference | Prefab từ Addressable |
| Cần MonoBehaviour | ❌ | ✅ (coroutine host) | ✅ |
| Auto-return | ❌ | ✅ | ✅ |
| Async init | ❌ | ❌ | ✅ |

---

## Setup

Tạo GameObject trong scene → Add Component → **PoolManager**

---

## Sử dụng

### Đăng ký pool
```csharp
// Từ Prefab trực tiếp
PoolManager.Instance.RegisterPrefab("bullet", bulletPrefab, initialSize: 20);

// Từ Addressable (async)
await PoolManager.Instance.RegisterAddressable("explosion", "prefabs/vfx_explosion");
```

### Spawn
```csharp
// Spawn GameObject
PoolManager.Instance.Get("bullet", position, rotation);

// Spawn và lấy component luôn
var bullet = PoolManager.Instance.Get<Bullet>("bullet", position, rotation);

// Auto-return sau 2 giây
PoolManager.Instance.Get("explosion", position, Quaternion.identity, autoReturnAfter: 2f);
```

### Return
```csharp
// Return thủ công
PoolManager.Instance.Return("bullet", gameObject);

// Return tất cả
PoolManager.Instance.ReturnAll("bullet");
PoolManager.Instance.ReturnAllPools();
```

### C# Object Pool
```csharp
var pool = new ObjectPool<BulletData>(
    factory:     () => new BulletData(),
    onSpawn:     b  => b.Reset(),
    onDespawn:   b  => b.Clear(),
    initialSize: 10
);

var data = pool.Get();
pool.Return(data);
```

---

## IPoolable — thay thế Awake/Destroy

```csharp
public class Bullet : MonoBehaviour, IPoolable
{
    public void OnSpawn()   { /* reset state — gọi khi Get()    */ }
    public void OnDespawn() { /* cleanup    — gọi khi Return()  */ }
}
```

---

## Tích hợp với EventBus

```csharp
// Return tất cả bullet khi level kết thúc
EventBus<OnLevelCompleted>.On(_ => PoolManager.Instance.ReturnAll("bullet"), this);

// Spawn explosion khi player chết
EventBus<OnPlayerDied>.On(_ =>
    PoolManager.Instance.Get("explosion", playerPos, Quaternion.identity, autoReturnAfter: 2f),
this);
```

---

## Cấu trúc file

```
PoolSystem/
├── Core/
│   ├── IPoolable.cs          ← OnSpawn / OnDespawn interface
│   ├── ObjectPool.cs         ← Generic C# object pool
│   ├── GameObjectPool.cs     ← Unity Prefab pool
│   ├── AddressablePool.cs    ← Addressable async pool
│   └── PoolManager.cs        ← Entry point tập trung
└── Example/
    └── PoolableExample.cs    ← Bullet, VFX, DamageNumber examples
```
