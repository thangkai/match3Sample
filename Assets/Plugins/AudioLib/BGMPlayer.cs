using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AudioLib
{
    /// <summary>
    /// Xử lý BGM với cross-fade mượt giữa các track.
    /// Dùng 2 AudioSource để fade in/out đồng thời.
    /// </summary>
    public class BGMPlayer : MonoBehaviour
    {
        [Header("Cross-fade Settings")]
        [SerializeField] private float defaultFadeDuration = 1f;

        private AudioSource _sourceA;
        private AudioSource _sourceB;
        private AudioSource _activeSoruce;

        private AsyncOperationHandle<AudioClip> _currentHandle;
        private AsyncOperationHandle<AudioClip> _previousHandle;

        private Coroutine _fadeCoroutine;
        private float _masterVolume = 1f;
        private BgmID _currentBgmId = BgmID.None;

        internal void Initialize()
        {
            _sourceA = CreateSource("BGM_A");
            _sourceB = CreateSource("BGM_B");
            _activeSoruce = _sourceA;
        }

        private AudioSource CreateSource(string sourceName)
        {
            var go = new GameObject(sourceName);
            go.transform.SetParent(transform);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.volume = 0f;
            return source;
        }

        /// <summary>Play BGM mới, tự động cross-fade nếu đang có track</summary>
        public void Play(BgmEntry entry, float fadeDuration = -1f)
        {
            if (entry.id == _currentBgmId) return;

            float duration = fadeDuration < 0 ? defaultFadeDuration : fadeDuration;
            _currentBgmId = entry.id;

            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);

            _fadeCoroutine = StartCoroutine(CrossFadeRoutine(entry, duration));
        }

        public void Stop(float fadeDuration = -1f)
        {
            float duration = fadeDuration < 0 ? defaultFadeDuration : fadeDuration;
            _currentBgmId = BgmID.None;

            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);

            _fadeCoroutine = StartCoroutine(FadeOutRoutine(_activeSoruce, duration));
        }

        public void SetVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
            // Chỉ update source đang active
            if (_activeSoruce != null && _activeSoruce.isPlaying)
                _activeSoruce.volume = _masterVolume;
        }

        public void Pause() => _activeSoruce?.Pause();
        public void Resume() => _activeSoruce?.UnPause();

        private IEnumerator CrossFadeRoutine(BgmEntry entry, float duration)
        {
            // Source sẽ nhận track mới là source "không active"
            var nextSource = _activeSoruce == _sourceA ? _sourceB : _sourceA;
            var prevSource = _activeSoruce;

            // Load clip từ Addressable
            if (_previousHandle.IsValid())
                Addressables.Release(_previousHandle);

            _previousHandle = _currentHandle;

            var opHandle = Addressables.LoadAssetAsync<AudioClip>(entry.addressableKey);
            yield return opHandle;

            if (opHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[BGMPlayer] Failed to load BGM: {entry.addressableKey}");
                yield break;
            }

            _currentHandle = opHandle;

            // Setup next source
            nextSource.clip = opHandle.Result;
            nextSource.volume = 0f;
            nextSource.loop = entry.loop;
            nextSource.Play();

            // Cross-fade
            float elapsed = 0f;
            float targetVolume = entry.volume * _masterVolume;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                nextSource.volume = Mathf.Lerp(0f, targetVolume, t);
                prevSource.volume = Mathf.Lerp(_masterVolume, 0f, t);

                yield return null;
            }

            nextSource.volume = targetVolume;
            prevSource.volume = 0f;
            prevSource.Stop();
            prevSource.clip = null;

            _activeSoruce = nextSource;
            _fadeCoroutine = null;
        }

        private IEnumerator FadeOutRoutine(AudioSource source, float duration)
        {
            float startVolume = source.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            source.Stop();
            source.volume = 0f;
            _fadeCoroutine = null;
        }

        private void OnDestroy()
        {
            if (_currentHandle.IsValid()) Addressables.Release(_currentHandle);
            if (_previousHandle.IsValid()) Addressables.Release(_previousHandle);
        }
    }
}
