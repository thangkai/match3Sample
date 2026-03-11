using UnityEngine;
using PoolLib;
using EventLib;
using EventLib.Example;

namespace PoolLib.Example
{
    // ═══════════════════════════════════════════════════════════
    //  VÍ DỤ 1: Bullet — IPoolable trên MonoBehaviour
    // ═══════════════════════════════════════════════════════════

    public class Bullet : MonoBehaviour, IPoolable
    {
        [SerializeField] private float speed = 10f;
        private string _poolKey = "bullet";
        private Vector3 _direction;

        public void SetDirection(Vector3 dir) => _direction = dir.normalized;

        public void OnSpawn()
        {
            // Reset state mỗi lần spawn
            GetComponent<Rigidbody2D>()?.velocity.Equals(Vector2.zero);
            gameObject.layer = LayerMask.NameToLayer("Projectile");
        }

        public void OnDespawn()
        {
            // Cleanup trước khi về pool
            _direction = Vector3.zero;
        }

        private void Update()
        {
            transform.Translate(_direction * speed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Return về pool thay vì Destroy
            PoolManager.Instance.Return(_poolKey, gameObject);
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  VÍ DỤ 2: VFX Explosion — auto-return sau X giây
    // ═══════════════════════════════════════════════════════════

    public class VFXExplosion : MonoBehaviour, IPoolable
    {
        private ParticleSystem _ps;

        private void Awake() => _ps = GetComponent<ParticleSystem>();

        public void OnSpawn()  => _ps?.Play();
        public void OnDespawn() => _ps?.Stop();
    }

    // ═══════════════════════════════════════════════════════════
    //  VÍ DỤ 3: C# Object Pool — DamageNumber data
    // ═══════════════════════════════════════════════════════════

    public class DamageNumber
    {
        public int value;
        public Vector3 position;
        public bool isCritical;

        public void Reset()
        {
            value      = 0;
            position   = Vector3.zero;
            isCritical = false;
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  VÍ DỤ 4: GameplayManager — tích hợp PoolManager + EventBus
    // ═══════════════════════════════════════════════════════════

    public class GameplayManager : MonoBehaviour
    {
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private GameObject explosionPrefab;

        // C# object pool không cần MonoBehaviour
        private ObjectPool<DamageNumber> _damagePool;

        private async void Start()
        {
            // 1. Register pool từ Prefab trực tiếp
            PoolManager.Instance.RegisterPrefab("bullet", bulletPrefab, initialSize: 20);

            // 2. Register pool từ Addressable
            await PoolManager.Instance.RegisterAddressable("explosion", "prefabs/vfx_explosion", initialSize: 5);

            // 3. C# Object Pool
            _damagePool = new ObjectPool<DamageNumber>(
                factory:     () => new DamageNumber(),
                onSpawn:     d  => d.Reset(),
                initialSize: 10
            );

            // 4. Lắng nghe event để spawn effect (tích hợp EventBus)
            EventBus<OnPlayerDied>.On(OnPlayerDied, this);
        }

        // ─── Spawn Examples ───────────────────────────────────────

        public void FireBullet(Vector3 position, Vector3 direction)
        {
            var bullet = PoolManager.Instance.Get<Bullet>("bullet", position, Quaternion.identity);
            bullet?.SetDirection(direction);
        }

        public void SpawnExplosion(Vector3 position)
        {
            // Auto-return sau 2 giây
            PoolManager.Instance.Get("explosion", position, Quaternion.identity, autoReturnAfter: 2f);
        }

        public void ShowDamage(Vector3 position, int damage, bool isCrit)
        {
            var data       = _damagePool.Get();
            data.value     = damage;
            data.position  = position;
            data.isCritical = isCrit;

            // ... hiển thị UI ...

            _damagePool.Return(data); // trả về sau khi xong animation
        }

        // ─── Event Handlers ───────────────────────────────────────

        private void OnPlayerDied(OnPlayerDied e)
        {
            // Return all active bullets khi player chết
            PoolManager.Instance.ReturnAll("bullet");

            // Spawn explosion lớn tại vị trí player
            SpawnExplosion(transform.position);
        }

        // ─── Debug ───────────────────────────────────────────────

        [ContextMenu("Print Pool Stats")]
        private void PrintStats()
        {
            var (inactive, active) = PoolManager.Instance.GetStats("bullet");
            Debug.Log($"[bullet] inactive: {inactive}, active: {active}");
        }
    }
}
