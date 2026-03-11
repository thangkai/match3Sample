using UnityEngine;
using SaveLib.Example;

namespace SaveLib
{
    /// <summary>
    /// Bootstrap tất cả data class và đăng ký vào SaveManager.
    /// Đây là class game-specific duy nhất cần tạo cho mỗi game.
    ///
    /// USAGE từ bất kỳ đâu:
    ///   GameDataManager.Progress.CompleteLevel(1, 3, 5000);
    ///   GameDataManager.Currency.Add(CurrencyData.Type.Coin, 100);
    ///   GameDataManager.Inventory.AddItem("shield_wood");
    ///   SaveManager.Instance.Save();
    /// </summary>
    public class GameDataManager : MonoBehaviour
    {
        public static GameDataManager Instance { get; private set; }

        // ─── Data Access ─────────────────────────────────────────
        public static PlayerProgressData Progress { get; private set; }
        public static SettingsData Settings       { get; private set; }
        public static CurrencyData Currency       { get; private set; }
        public static InventoryData Inventory     { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeData();
        }

        private void InitializeData()
        {
            Progress  = new PlayerProgressData();
            Settings  = new SettingsData();
            Currency  = new CurrencyData();
            Inventory = new InventoryData();

            // Đăng ký vào SaveManager
            var save = SaveManager.Instance;
            save.Register(Progress);
            save.Register(Settings);
            save.Register(Currency);
            save.Register(Inventory);

            // Load save hiện tại (nếu có)
            bool loaded = save.Load();
            if (!loaded)
                Debug.Log("[GameDataManager] No save found, using defaults");
        }

        // ─── Convenience shortcuts ────────────────────────────────

        public void SaveGame() => SaveManager.Instance.Save();

        public void LoadGame() => SaveManager.Instance.Load();

        private void OnApplicationPause(bool pauseStatus)
        {
            // Auto-save khi app bị pause (chuyển tab, nhận call...)
            if (pauseStatus) SaveGame();
        }

        private void OnApplicationQuit() => SaveGame();
    }
}
