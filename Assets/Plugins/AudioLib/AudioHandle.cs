using System;
using UnityEngine;

namespace AudioLib
{
    /// <summary>
    /// Handle để control một audio instance đang chạy.
    /// Giống như một "remote control" cho audio cụ thể.
    /// </summary>
    public class AudioHandle
    {
        public bool IsValid => _source != null && _source.isPlaying;
        public bool IsPaused { get; private set; }
        public SfxID SfxId { get; }

        private AudioSource _source;
        private Action<AudioHandle> _onComplete;

        internal AudioHandle(AudioSource source, SfxID sfxId, Action<AudioHandle> onComplete = null)
        {
            _source = source;
            SfxId = sfxId;
            _onComplete = onComplete;
        }

        public void Pause()
        {
            if (_source == null) return;
            _source.Pause();
            IsPaused = true;
        }

        public void Resume()
        {
            if (_source == null) return;
            _source.UnPause();
            IsPaused = false;
        }

        public void Stop()
        {
            if (_source == null) return;
            _source.Stop();
            _onComplete?.Invoke(this);
            _source = null;
        }

        public void SetVolume(float volume)
        {
            if (_source == null) return;
            _source.volume = Mathf.Clamp01(volume);
        }

        /// <summary>Gọi bởi SFXPlayer khi audio tự kết thúc</summary>
        internal void MarkCompleted()
        {
            _onComplete?.Invoke(this);
            _source = null;
        }
    }
}
