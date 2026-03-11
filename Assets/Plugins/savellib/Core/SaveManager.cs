using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

namespace SaveLib
{
    /// <summary>
    /// Entry point chính của Save System.
    /// Quản lý multiple save slots và điều phối save/load cho các ISaveable.
    ///
    /// USAGE:
    ///   // Đăng ký data class
    ///   SaveManager.Instance.Register(playerData);
    ///
    ///   // Save/Load
    ///   SaveManager.Instance.Save();
    ///   SaveManager.Instance.Load();
    ///
    ///   // Slot management
    ///   SaveManager.Instance.SwitchSlot(1);
    ///   var slots = SaveManager.Instance.GetAllSlots();
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool useEncryption = true;
        [SerializeField] private int maxSlots = 3;
        [SerializeField] private string appVersion = "1.0.0";

        // Events
        public event Action OnSaveCompleted;
        public event Action OnLoadCompleted;
        public event Action<int> OnSlotSwitched;

        public int CurrentSlot { get; private set; } = 0;

        private FileStorage _storage;
        private readonly List<ISaveable> _saveables = new();
        private const string SLOTS_META_FILE = "slots_meta";

        // ─── Lifecycle ───────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _storage = new FileStorage(useEncryption);
        }

        // ─── Registration ─────────────────────────────────────────

        /// <summary>Đăng ký một ISaveable vào hệ thống</summary>
        public void Register(ISaveable saveable)
        {
            if (_saveables.Any(s => s.SaveKey == saveable.SaveKey))
            {
                Debug.LogWarning($"[SaveManager] Duplicate SaveKey: {saveable.SaveKey}");
                return;
            }
            _saveables.Add(saveable);
        }

        public void Unregister(ISaveable saveable) => _saveables.Remove(saveable);

        // ─── Save / Load ──────────────────────────────────────────

        /// <summary>Save tất cả ISaveable vào current slot</summary>
        public void Save()
        {
            var file = new SaveFile
            {
                slotIndex = CurrentSlot,
                version   = appVersion,
                savedAt   = DateTime.UtcNow.ToString("o"),
            };

            foreach (var saveable in _saveables)
            {
                try
                {
                    var state = saveable.CaptureState();
                    file.data[saveable.SaveKey] = JsonConvert.SerializeObject(state);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveManager] CaptureState failed for {saveable.SaveKey}: {e.Message}");
                }
            }

            _storage.Write(SlotFileName(CurrentSlot), file);
            UpdateSlotMeta(CurrentSlot, file);

            OnSaveCompleted?.Invoke();
            Debug.Log($"[SaveManager] Saved slot {CurrentSlot} ({_saveables.Count} saveables)");
        }

        /// <summary>Load từ current slot vào tất cả ISaveable</summary>
        public bool Load()
        {
            var file = _storage.Read<SaveFile>(SlotFileName(CurrentSlot));
            if (file == null)
            {
                Debug.Log($"[SaveManager] No save found for slot {CurrentSlot}");
                return false;
            }

            foreach (var saveable in _saveables)
            {
                if (!file.data.TryGetValue(saveable.SaveKey, out var json)) continue;

                try
                {
                    var state = JsonConvert.DeserializeObject(json);
                    saveable.RestoreState(state);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveManager] RestoreState failed for {saveable.SaveKey}: {e.Message}");
                }
            }

            OnLoadCompleted?.Invoke();
            Debug.Log($"[SaveManager] Loaded slot {CurrentSlot} (saved at {file.savedAt})");
            return true;
        }

        // ─── Slot Management ──────────────────────────────────────

        /// <summary>Chuyển sang slot khác (không tự động save/load)</summary>
        public void SwitchSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxSlots)
            {
                Debug.LogError($"[SaveManager] Invalid slot: {slotIndex}");
                return;
            }
            CurrentSlot = slotIndex;
            OnSlotSwitched?.Invoke(CurrentSlot);
        }

        /// <summary>Lấy metadata của tất cả slots để hiển thị UI</summary>
        public List<SaveSlotInfo> GetAllSlots()
        {
            var meta = _storage.Read<List<SaveSlotInfo>>(SLOTS_META_FILE)
                       ?? new List<SaveSlotInfo>();

            // Đảm bảo đủ maxSlots entries
            for (int i = 0; i < maxSlots; i++)
            {
                if (!meta.Any(s => s.slotIndex == i))
                    meta.Add(new SaveSlotInfo { slotIndex = i, isEmpty = true, displayName = $"Slot {i + 1}" });
            }

            return meta.OrderBy(s => s.slotIndex).ToList();
        }

        /// <summary>Xoá save data của một slot</summary>
        public void DeleteSlot(int slotIndex)
        {
            _storage.Delete(SlotFileName(slotIndex));

            var meta = _storage.Read<List<SaveSlotInfo>>(SLOTS_META_FILE) ?? new();
            meta.RemoveAll(s => s.slotIndex == slotIndex);
            _storage.Write(SLOTS_META_FILE, meta);

            Debug.Log($"[SaveManager] Deleted slot {slotIndex}");
        }

        public bool SlotExists(int slotIndex) => _storage.Exists(SlotFileName(slotIndex));

        // ─── Private ─────────────────────────────────────────────

        private string SlotFileName(int slot) => $"save_slot_{slot}";

        private void UpdateSlotMeta(int slotIndex, SaveFile file)
        {
            var meta = _storage.Read<List<SaveSlotInfo>>(SLOTS_META_FILE) ?? new();
            meta.RemoveAll(s => s.slotIndex == slotIndex);

            // Game tự override method này để thêm preview info
            var info = BuildSlotInfo(slotIndex, file);
            meta.Add(info);
            _storage.Write(SLOTS_META_FILE, meta);
        }

        /// <summary>
        /// Override để thêm preview data (level, tên nhân vật...) vào slot info.
        /// Mặc định chỉ lưu datetime.
        /// </summary>
        protected virtual SaveSlotInfo BuildSlotInfo(int slotIndex, SaveFile file)
        {
            return new SaveSlotInfo
            {
                slotIndex   = slotIndex,
                displayName = $"Slot {slotIndex + 1}",
                lastSavedAt = file.savedAt,
                isEmpty     = false,
            };
        }
    }
}
