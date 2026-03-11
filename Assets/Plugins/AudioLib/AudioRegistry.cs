using System.Collections.Generic;
using UnityEngine;

namespace AudioLib
{
    [System.Serializable]
    public class BgmEntry
    {
        public BgmID id;
        public string addressableKey;   // Key trong Addressable group
        [Range(0f, 1f)] public float volume = 1f;
        public bool loop = true;
    }

    [System.Serializable]
    public class SfxEntry
    {
        public SfxID id;
        public string addressableKey;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(-3f, 3f)] public float pitch = 1f;
        public float pitchVariance = 0f;    // Random pitch ±variance
        public bool spatial = false;         // 3D audio
        [Range(1f, 500f)] public float minDistance = 1f;
        [Range(1f, 500f)] public float maxDistance = 50f;
    }

    /// <summary>
    /// Registry chứa toàn bộ config audio của game.
    /// Mỗi game tạo 1 instance ScriptableObject riêng và điền data.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioRegistry", menuName = "AudioLib/Audio Registry")]
    public class AudioRegistry : ScriptableObject
    {
        [Header("BGM Entries")]
        [SerializeField] private List<BgmEntry> bgmEntries = new();

        [Header("SFX Entries")]
        [SerializeField] private List<SfxEntry> sfxEntries = new();

        // Runtime lookup cache
        private Dictionary<BgmID, BgmEntry> _bgmMap;
        private Dictionary<SfxID, SfxEntry> _sfxMap;

        private void OnEnable() => BuildCache();

        public void BuildCache()
        {
            _bgmMap = new Dictionary<BgmID, BgmEntry>();
            foreach (var entry in bgmEntries)
            {
                if (!_bgmMap.TryAdd(entry.id, entry))
                    Debug.LogWarning($"[AudioRegistry] Duplicate BgmID: {entry.id}");
            }

            _sfxMap = new Dictionary<SfxID, SfxEntry>();
            foreach (var entry in sfxEntries)
            {
                if (!_sfxMap.TryAdd(entry.id, entry))
                    Debug.LogWarning($"[AudioRegistry] Duplicate SfxID: {entry.id}");
            }
        }

        public bool TryGetBgm(BgmID id, out BgmEntry entry)
        {
            if (_bgmMap == null) BuildCache();
            return _bgmMap.TryGetValue(id, out entry);
        }

        public bool TryGetSfx(SfxID id, out SfxEntry entry)
        {
            if (_sfxMap == null) BuildCache();
            return _sfxMap.TryGetValue(id, out entry);
        }
    }
}
