using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AudioLib
{
    /// <summary>
    /// Quản lý và phát SFX với AudioSource pooling.
    /// Hỗ trợ cả 2D và 3D spatial audio.
    /// </summary>
    public class SFXPlayer : MonoBehaviour
    {
        [SerializeField] private int initialPoolSize = 10;

        private Queue<AudioSource> _pool = new();
        private List<ActiveSfx> _activeSfxList = new();
        private Dictionary<string, AsyncOperationHandle<AudioClip>> _clipCache = new();
        private float _masterVolume = 1f;

        private struct ActiveSfx
        {
            public AudioSource Source;
            public AudioHandle Handle;
        }

        internal void Initialize()
        {
            for (int i = 0; i < initialPoolSize; i++)
                _pool.Enqueue(CreateSource());

            StartCoroutine(TrackCompletedSfx());
        }

        private AudioSource CreateSource()
        {
            var go = new GameObject("SFX_Source");
            go.transform.SetParent(transform);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            return source;
        }

        /// <summary>Play SFX 2D (UI, non-spatial)</summary>
        public AudioHandle Play(SfxEntry entry)
        {
            return PlayInternal(entry, null, Vector3.zero);
        }

        /// <summary>Play SFX 3D tại world position</summary>
        public AudioHandle PlayAtPosition(SfxEntry entry, Vector3 worldPosition)
        {
            return PlayInternal(entry, null, worldPosition);
        }

        /// <summary>Play SFX 3D gắn vào Transform (di chuyển theo object)</summary>
        public AudioHandle PlayAttached(SfxEntry entry, Transform target)
        {
            return PlayInternal(entry, target, Vector3.zero);
        }

        private AudioHandle PlayInternal(SfxEntry entry, Transform attachTarget, Vector3 worldPos)
        {
            var source = GetSource();
            ConfigureSource(source, entry, attachTarget, worldPos);

            AudioHandle handle = null;
            handle = new AudioHandle(source, entry.id, h => ReturnSource(source));

            // Load clip (từ cache hoặc Addressable)
            StartCoroutine(LoadAndPlay(entry, source));

            _activeSfxList.Add(new ActiveSfx { Source = source, Handle = handle });
            return handle;
        }

        private IEnumerator LoadAndPlay(SfxEntry entry, AudioSource source)
        {
            AudioClip clip;

            if (_clipCache.TryGetValue(entry.addressableKey, out var cached) && cached.IsValid())
            {
                clip = cached.Result;
            }
            else
            {
                var op = Addressables.LoadAssetAsync<AudioClip>(entry.addressableKey);
                _clipCache[entry.addressableKey] = op;
                yield return op;

                if (op.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"[SFXPlayer] Failed to load SFX: {entry.addressableKey}");
                    ReturnSource(source);
                    yield break;
                }

                clip = op.Result;
            }

            if (source == null) yield break; // Đã bị return về pool

            source.clip = clip;
            source.Play();
        }

        private void ConfigureSource(AudioSource source, SfxEntry entry, Transform attachTarget, Vector3 worldPos)
        {
            source.volume = entry.volume * _masterVolume;
            source.pitch = entry.pitch + Random.Range(-entry.pitchVariance, entry.pitchVariance);
            source.loop = false;

            if (entry.spatial)
            {
                source.spatialBlend = 1f; // Full 3D
                source.rolloffMode = AudioRolloffMode.Logarithmic;
                source.minDistance = entry.minDistance;
                source.maxDistance = entry.maxDistance;

                if (attachTarget != null)
                    source.transform.SetParent(attachTarget);
                else
                    source.transform.position = worldPos;
            }
            else
            {
                source.spatialBlend = 0f; // Full 2D
                source.transform.SetParent(transform);
            }
        }

        public void SetVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
            foreach (var active in _activeSfxList)
                if (active.Source != null)
                    active.Source.volume = _masterVolume;
        }

        public void StopAll()
        {
            foreach (var active in _activeSfxList)
                active.Handle?.Stop();
            _activeSfxList.Clear();
        }

        private AudioSource GetSource()
        {
            if (_pool.Count > 0) return _pool.Dequeue();

            // Pool rỗng → tạo thêm
            Debug.LogWarning("[SFXPlayer] Pool exhausted, creating new AudioSource");
            return CreateSource();
        }

        private void ReturnSource(AudioSource source)
        {
            source.Stop();
            source.clip = null;
            source.transform.SetParent(transform);
            source.transform.localPosition = Vector3.zero;
            source.spatialBlend = 0f;
            _pool.Enqueue(source);

            // Xóa khỏi active list
            _activeSfxList.RemoveAll(a => a.Source == source);
        }

        /// <summary>Theo dõi SFX đã kết thúc để return về pool</summary>
        private IEnumerator TrackCompletedSfx()
        {
            while (true)
            {
                for (int i = _activeSfxList.Count - 1; i >= 0; i--)
                {
                    var active = _activeSfxList[i];
                    if (active.Source != null && !active.Source.isPlaying && !active.Source.isActiveAndEnabled == false)
                    {
                        active.Handle.MarkCompleted();
                        ReturnSource(active.Source);
                    }
                }
                yield return new WaitForSeconds(0.1f);
            }
        }

        private void OnDestroy()
        {
            foreach (var kv in _clipCache)
                if (kv.Value.IsValid())
                    Addressables.Release(kv.Value);
        }
    }
}
