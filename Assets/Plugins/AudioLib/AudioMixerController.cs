using UnityEngine;
using UnityEngine.Audio;

namespace AudioLib
{
    /// <summary>
    /// Quản lý volume cho từng channel (Master, BGM, SFX).
    /// Tự động lưu/load setting bằng PlayerPrefs.
    /// </summary>
    public class AudioMixerController
    {
        private const string KEY_MASTER = "audio_master";
        private const string KEY_BGM    = "audio_bgm";
        private const string KEY_SFX    = "audio_sfx";

        private readonly AudioMixer _mixer; // Optional - có thể null nếu không dùng Mixer
        private readonly BGMPlayer _bgmPlayer;
        private readonly SFXPlayer _sfxPlayer;

        private float _masterVolume;
        private float _bgmVolume;
        private float _sfxVolume;

        public float MasterVolume => _masterVolume;
        public float BgmVolume    => _bgmVolume;
        public float SfxVolume    => _sfxVolume;

        public AudioMixerController(BGMPlayer bgmPlayer, SFXPlayer sfxPlayer, AudioMixer mixer = null)
        {
            _bgmPlayer = bgmPlayer;
            _sfxPlayer = sfxPlayer;
            _mixer = mixer;

            LoadSettings();
            ApplyAll();
        }

        // ─── Public API ─────────────────────────────────────────

        public void SetMasterVolume(float value)
        {
            _masterVolume = Mathf.Clamp01(value);
            ApplyMaster();
            PlayerPrefs.SetFloat(KEY_MASTER, _masterVolume);
        }

        public void SetBgmVolume(float value)
        {
            _bgmVolume = Mathf.Clamp01(value);
            ApplyBgm();
            PlayerPrefs.SetFloat(KEY_BGM, _bgmVolume);
        }

        public void SetSfxVolume(float value)
        {
            _sfxVolume = Mathf.Clamp01(value);
            ApplySfx();
            PlayerPrefs.SetFloat(KEY_SFX, _sfxVolume);
        }

        public void ToggleMute()
        {
            bool muted = _masterVolume <= 0f;
            SetMasterVolume(muted ? 1f : 0f);
        }

        public void ToggleBgmMute() => SetBgmVolume(_bgmVolume > 0f ? 0f : 1f);
        public void ToggleSfxMute() => SetSfxVolume(_sfxVolume > 0f ? 0f : 1f);

        // ─── Internal ────────────────────────────────────────────

        private void LoadSettings()
        {
            _masterVolume = PlayerPrefs.GetFloat(KEY_MASTER, 1f);
            _bgmVolume    = PlayerPrefs.GetFloat(KEY_BGM, 1f);
            _sfxVolume    = PlayerPrefs.GetFloat(KEY_SFX, 1f);
        }

        private void ApplyAll()
        {
            ApplyMaster();
            ApplyBgm();
            ApplySfx();
        }

        private void ApplyMaster()
        {
            if (_mixer != null)
                _mixer.SetFloat("MasterVolume", VolumeToDb(_masterVolume));
        }

        private void ApplyBgm()
        {
            float effective = _bgmVolume * _masterVolume;
            _bgmPlayer?.SetVolume(effective);

            if (_mixer != null)
                _mixer.SetFloat("BgmVolume", VolumeToDb(_bgmVolume));
        }

        private void ApplySfx()
        {
            float effective = _sfxVolume * _masterVolume;
            _sfxPlayer?.SetVolume(effective);

            if (_mixer != null)
                _mixer.SetFloat("SfxVolume", VolumeToDb(_sfxVolume));
        }

        /// Convert 0-1 sang dB cho AudioMixer (-80dB → 0dB)
        private float VolumeToDb(float volume)
            => volume > 0.0001f ? Mathf.Log10(volume) * 20f : -80f;
    }
}
