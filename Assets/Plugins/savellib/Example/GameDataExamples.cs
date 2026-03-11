using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaveLib.Example
{
    // ═══════════════════════════════════════════════════════════
    //  PLAYER PROGRESS
    // ═══════════════════════════════════════════════════════════

    [Serializable]
    public class PlayerProgressState
    {
        public int currentLevel;
        public int highScore;
        public int totalStarsEarned;
        public List<int> completedLevels = new();
        public Dictionary<int, int> levelStars = new(); // levelId → stars (1-3)
    }

    public class PlayerProgressData : ISaveable
    {
        public string SaveKey => "player_progress";

        // Runtime data
        public int CurrentLevel    { get; private set; } = 1;
        public int HighScore       { get; private set; } = 0;
        public int TotalStars      { get; private set; } = 0;

        private HashSet<int> _completedLevels = new();
        private Dictionary<int, int> _levelStars = new();

        public void CompleteLevel(int levelId, int stars, int score)
        {
            _completedLevels.Add(levelId);
            _levelStars[levelId] = Mathf.Max(_levelStars.GetValueOrDefault(levelId), stars);
            HighScore   = Mathf.Max(HighScore, score);
            TotalStars  = 0;
            foreach (var s in _levelStars.Values) TotalStars += s;

            CurrentLevel = Mathf.Max(CurrentLevel, levelId + 1);
        }

        public bool IsLevelCompleted(int levelId) => _completedLevels.Contains(levelId);
        public int GetLevelStars(int levelId)     => _levelStars.GetValueOrDefault(levelId, 0);

        // ─── ISaveable ───────────────────────────────────────────
        public object CaptureState() => new PlayerProgressState
        {
            currentLevel     = CurrentLevel,
            highScore        = HighScore,
            totalStarsEarned = TotalStars,
            completedLevels  = new List<int>(_completedLevels),
            levelStars       = new Dictionary<int, int>(_levelStars),
        };

        public void RestoreState(object state)
        {
            if (state is not PlayerProgressState s) return;
            CurrentLevel      = s.currentLevel;
            HighScore         = s.highScore;
            TotalStars        = s.totalStarsEarned;
            _completedLevels  = new HashSet<int>(s.completedLevels);
            _levelStars       = s.levelStars ?? new();
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  SETTINGS
    // ═══════════════════════════════════════════════════════════

    [Serializable]
    public class SettingsState
    {
        public float masterVolume = 1f;
        public float bgmVolume    = 1f;
        public float sfxVolume    = 1f;
        public string language    = "en";
        public bool vibration     = true;
        public int qualityLevel   = 2;
    }

    public class SettingsData : ISaveable
    {
        public string SaveKey => "settings";

        public float MasterVolume { get; private set; } = 1f;
        public float BgmVolume    { get; private set; } = 1f;
        public float SfxVolume    { get; private set; } = 1f;
        public string Language    { get; private set; } = "en";
        public bool Vibration     { get; private set; } = true;
        public int QualityLevel   { get; private set; } = 2;

        public void SetVolume(float master, float bgm, float sfx)
        {
            MasterVolume = master;
            BgmVolume    = bgm;
            SfxVolume    = sfx;
        }

        public void SetLanguage(string lang) => Language    = lang;
        public void SetVibration(bool on)    => Vibration   = on;
        public void SetQuality(int level)    => QualityLevel = level;

        public object CaptureState() => new SettingsState
        {
            masterVolume = MasterVolume,
            bgmVolume    = BgmVolume,
            sfxVolume    = SfxVolume,
            language     = Language,
            vibration    = Vibration,
            qualityLevel = QualityLevel,
        };

        public void RestoreState(object state)
        {
            if (state is not SettingsState s) return;
            MasterVolume = s.masterVolume;
            BgmVolume    = s.bgmVolume;
            SfxVolume    = s.sfxVolume;
            Language     = s.language;
            Vibration    = s.vibration;
            QualityLevel = s.qualityLevel;
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  CURRENCY
    // ═══════════════════════════════════════════════════════════

    [Serializable]
    public class CurrencyState
    {
        public Dictionary<string, long> balances = new();
    }

    public class CurrencyData : ISaveable
    {
        public string SaveKey => "currency";

        private Dictionary<string, long> _balances = new();

        // Tên currency dùng như constant để tránh typo
        public static class Type
        {
            public const string Coin = "coin";
            public const string Gem  = "gem";
        }

        public long Get(string type)      => _balances.GetValueOrDefault(type, 0);
        public bool Has(string type, long amount) => Get(type) >= amount;

        public void Add(string type, long amount)
        {
            _balances[type] = Get(type) + amount;
            Debug.Log($"[Currency] +{amount} {type} → total: {Get(type)}");
        }

        public bool Spend(string type, long amount)
        {
            if (!Has(type, amount))
            {
                Debug.LogWarning($"[Currency] Not enough {type}: need {amount}, have {Get(type)}");
                return false;
            }
            _balances[type] -= amount;
            return true;
        }

        public object CaptureState() => new CurrencyState
        {
            balances = new Dictionary<string, long>(_balances)
        };

        public void RestoreState(object state)
        {
            if (state is not CurrencyState s) return;
            _balances = s.balances ?? new();
        }
    }

    // ═══════════════════════════════════════════════════════════
    //  INVENTORY
    // ═══════════════════════════════════════════════════════════

    [Serializable]
    public class ItemEntry
    {
        public string itemId;
        public int quantity;
        public Dictionary<string, string> metadata = new(); // extra data tuỳ game
    }

    [Serializable]
    public class InventoryState
    {
        public List<ItemEntry> items = new();
    }

    public class InventoryData : ISaveable
    {
        public string SaveKey => "inventory";

        private Dictionary<string, ItemEntry> _items = new();

        public IEnumerable<ItemEntry> AllItems => _items.Values;

        public void AddItem(string itemId, int quantity = 1, Dictionary<string, string> metadata = null)
        {
            if (_items.TryGetValue(itemId, out var entry))
                entry.quantity += quantity;
            else
                _items[itemId] = new ItemEntry { itemId = itemId, quantity = quantity, metadata = metadata ?? new() };
        }

        public bool RemoveItem(string itemId, int quantity = 1)
        {
            if (!_items.TryGetValue(itemId, out var entry) || entry.quantity < quantity)
                return false;

            entry.quantity -= quantity;
            if (entry.quantity <= 0) _items.Remove(itemId);
            return true;
        }

        public bool HasItem(string itemId, int quantity = 1)
            => _items.TryGetValue(itemId, out var e) && e.quantity >= quantity;

        public int GetQuantity(string itemId)
            => _items.TryGetValue(itemId, out var e) ? e.quantity : 0;

        public object CaptureState() => new InventoryState
        {
            items = new List<ItemEntry>(_items.Values)
        };

        public void RestoreState(object state)
        {
            if (state is not InventoryState s) return;
            _items = new Dictionary<string, ItemEntry>();
            foreach (var item in s.items)
                _items[item.itemId] = item;
        }
    }
}
