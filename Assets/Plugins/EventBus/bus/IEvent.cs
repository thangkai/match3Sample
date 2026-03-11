namespace EventLib
{
    /// <summary>
    /// Marker interface — tất cả event phải implement cái này.
    /// 
    /// USAGE:
    ///   public struct OnPlayerDied : IEvent { public int score; }
    ///   public struct OnCoinCollected : IEvent { public int amount; }
    /// </summary>
    public interface IEvent { }
}
