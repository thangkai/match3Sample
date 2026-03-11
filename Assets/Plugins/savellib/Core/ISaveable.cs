namespace SaveLib
{
    /// <summary>
    /// Implement interface này vào bất kỳ class nào muốn được save/load.
    /// 
    /// USAGE:
    ///   public class PlayerData : ISaveable { ... }
    ///   SaveManager.Instance.Register(playerData);
    /// </summary>
    public interface ISaveable
    {
        /// <summary>Key duy nhất để identify data này trong save file</summary>
        string SaveKey { get; }

        /// <summary>Trả về object sẽ được serialize thành JSON</summary>
        object CaptureState();

        /// <summary>Nhận lại data từ JSON đã deserialize</summary>
        void RestoreState(object state);
    }
}
