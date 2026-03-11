namespace PoolLib
{
    /// <summary>
    /// Implement interface này vào class muốn được pool.
    /// OnSpawn() thay thế cho Awake/Start, OnDespawn() thay thế OnDisable/OnDestroy.
    ///
    /// USAGE:
    ///   public class Bullet : MonoBehaviour, IPoolable {
    ///       public void OnSpawn()   { /* reset state */ }
    ///       public void OnDespawn() { /* cleanup     */ }
    ///   }
    /// </summary>
    public interface IPoolable
    {
        /// <summary>Gọi khi object được lấy ra khỏi pool (thay thế Awake/Enable)</summary>
        void OnSpawn();

        /// <summary>Gọi khi object được trả về pool (thay thế OnDisable/Destroy)</summary>
        void OnDespawn();
    }
}
