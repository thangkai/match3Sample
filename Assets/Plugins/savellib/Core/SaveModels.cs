using System;
using System.Collections.Generic;

namespace SaveLib
{
    /// <summary>
    /// Metadata của một save slot (hiển thị ở màn hình chọn slot)
    /// </summary>
    [Serializable]
    public class SaveSlotInfo
    {
        public int slotIndex;
        public string displayName;      // VD: "Slot 1", hoặc tên player tự đặt
        public string lastSavedAt;      // ISO 8601 datetime string
        public int playerLevel;         // Snapshot để hiển thị preview
        public bool isEmpty;
    }

    /// <summary>
    /// Toàn bộ nội dung của một save file
    /// </summary>
    [Serializable]
    public class SaveFile
    {
        public int slotIndex;
        public string version;          // App version khi save — dùng cho migration
        public string savedAt;
        public Dictionary<string, string> data = new(); // key → JSON string
    }
}
