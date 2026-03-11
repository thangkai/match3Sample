using UnityEngine;
using UnityEngine.Audio;

namespace AudioLib
{
    /// <summary>
    /// Entry point duy nhất cho Audio System.
    /// Gắn vào GameObject trong scene hoặc dùng qua AudioManager.Instance.
    /// 
    /// USAGE:
    ///   AudioManager.Instance.PlayBgm(BgmID.Gameplay);
    ///   AudioManager.Instance.PlaySfx(SfxID.ButtonClick);
    ///   var handle = AudioManager.Instance.PlaySfx(SfxID.CoinCollect);
    ///   handle.Stop();
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Registry")]
        [SerializeField] private AudioRegistry registry;

        [Header("Optional: Unity Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;

        [Header("SFX Pool")]
        [SerializeField] private int sfxPoolSize = 10;

        private BGMPlayer _bgmPlayer;
        private SFXPlayer _sfxPlayer;
        private AudioMixerController _mixerController;

        // ─── Properties ──────────────────────────────────────────
        public float MasterVolume => _mixerController.MasterVolume;
        public float BgmVolume    => _mixerController.BgmVolume;
        public float SfxVolume    => _mixerController.SfxVolume;

        // ─── Lifecycle ───────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        private void Initialize()
        {
            // Tạo BGMPlayer
            var bgmGo = new GameObject("BGMPlayer");
            bgmGo.transform.SetParent(transform);
            _bgmPlayer = bgmGo.AddComponent<BGMPlayer>();
            _bgmPlayer.Initialize();

            // Tạo SFXPlayer
            var sfxGo = new GameObject("SFXPlayer");
            sfxGo.transform.SetParent(transform);
            _sfxPlayer = sfxGo.AddComponent<SFXPlayer>();
            _sfxPlayer.Initialize();

            // Mixer controller
            _mixerController = new AudioMixerController(_bgmPlayer, _sfxPlayer, audioMixer);

            registry.BuildCache();
        }

        // ─── BGM API ─────────────────────────────────────────────

        /// <summary>Play BGM, tự cross-fade nếu đang có track khác</summary>
        public void PlayBgm(BgmID id, float fadeDuration = -1f)
        {
            if (!registry.TryGetBgm(id, out var entry))
            {
                Debug.LogWarning($"[AudioManager] BgmID not found: {id}");
                return;
            }
            _bgmPlayer.Play(entry, fadeDuration);
        }

        /// <summary>Stop BGM hiện tại với fade out</summary>
        public void StopBgm(float fadeDuration = -1f) => _bgmPlayer.Stop(fadeDuration);

        public void PauseBgm() => _bgmPlayer.Pause();
        public void ResumeBgm() => _bgmPlayer.Resume();

        // ─── SFX API ─────────────────────────────────────────────

        /// <summary>Play SFX 2D. Trả về handle để control sau</summary>
        public AudioHandle PlaySfx(SfxID id)
        {
            if (!registry.TryGetSfx(id, out var entry))
            {
                Debug.LogWarning($"[AudioManager] SfxID not found: {id}");
                return null;
            }
            return _sfxPlayer.Play(entry);
        }

        /// <summary>Play SFX 3D tại world position</summary>
        public AudioHandle PlaySfxAt(SfxID id, Vector3 worldPosition)
        {
            if (!registry.TryGetSfx(id, out var entry))
            {
                Debug.LogWarning($"[AudioManager] SfxID not found: {id}");
                return null;
            }
            return _sfxPlayer.PlayAtPosition(entry, worldPosition);
        }

        /// <summary>Play SFX 3D gắn theo Transform</summary>
        public AudioHandle PlaySfxAttached(SfxID id, Transform target)
        {
            if (!registry.TryGetSfx(id, out var entry))
            {
                Debug.LogWarning($"[AudioManager] SfxID not found: {id}");
                return null;
            }
            return _sfxPlayer.PlayAttached(entry, target);
        }

        public void StopAllSfx() => _sfxPlayer.StopAll();

        // ─── Volume API ───────────────────────────────────────────

        public void SetMasterVolume(float value) => _mixerController.SetMasterVolume(value);
        public void SetBgmVolume(float value)    => _mixerController.SetBgmVolume(value);
        public void SetSfxVolume(float value)    => _mixerController.SetSfxVolume(value);

        public void ToggleMute()    => _mixerController.ToggleMute();
        public void ToggleBgmMute() => _mixerController.ToggleBgmMute();
        public void ToggleSfxMute() => _mixerController.ToggleSfxMute();
    }
}
