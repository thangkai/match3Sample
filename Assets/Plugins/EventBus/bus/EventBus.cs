using System;
using System.Collections.Generic;
using UnityEngine;

namespace EventLib
{
    /// <summary>
    /// Global Event Bus dùng generic type làm key.
    /// Thread-safe cho emit, không cần MonoBehaviour.
    ///
    /// USAGE:
    ///   // Define event
    ///   public struct OnPlayerDied : IEvent { public int score; }
    ///
    ///   // Subscribe
    ///   EventBus<OnPlayerDied>.On(HandleDeath);
    ///   EventBus<OnPlayerDied>.On(HandleDeath, this);           // auto-unsubscribe
    ///   EventBus<OnPlayerDied>.On(HandleDeath, EventPriority.High);
    ///
    ///   // Emit
    ///   EventBus<OnPlayerDied>.Emit(new OnPlayerDied { score = 100 });
    ///
    ///   // Unsubscribe
    ///   EventBus<OnPlayerDied>.Off(HandleDeath);
    ///   EventBus<OnPlayerDied>.Clear();
    /// </summary>
    public static class EventBus<T> where T : IEvent
    {
        private static readonly List<EventBinding<T>> _bindings = new();
        private static readonly List<EventBinding<T>> _toRemove = new(); // buffer tránh modify while iterating
        private static bool _isEmitting = false;

        // ─── Subscribe ────────────────────────────────────────────

        /// <summary>Subscribe, tự unsubscribe thủ công bằng Off()</summary>
        public static void On(Action<T> handler, EventPriority priority = EventPriority.Normal)
        {
            if (IsAlreadySubscribed(handler)) return;
            _bindings.Add(new EventBinding<T>(handler, priority));
            SortByPriority();
        }

        /// <summary>Subscribe với owner MonoBehaviour — tự unsubscribe khi owner destroy</summary>
        public static void On(Action<T> handler, UnityEngine.Object owner, EventPriority priority = EventPriority.Normal)
        {
            if (IsAlreadySubscribed(handler)) return;
            _bindings.Add(new EventBinding<T>(handler, owner, priority));
            SortByPriority();
        }

        /// <summary>Subscribe chỉ nhận 1 lần rồi tự unsubscribe</summary>
        public static void Once(Action<T> handler, UnityEngine.Object owner = null)
        {
            Action<T> wrapper = null;
            wrapper = (e) =>
            {
                handler(e);
                Off(wrapper);
            };

            if (owner != null) On(wrapper, owner);
            else On(wrapper);
        }

        // ─── Unsubscribe ─────────────────────────────────────────

        public static void Off(Action<T> handler)
        {
            if (_isEmitting)
            {
                // Defer removal để tránh modify list khi đang iterate
                _toRemove.AddRange(_bindings.FindAll(b => b.Handler == handler));
                return;
            }
            _bindings.RemoveAll(b => b.Handler == handler);
        }

        /// <summary>Xóa tất cả subscriber của event này</summary>
        public static void Clear() => _bindings.Clear();

        // ─── Emit ────────────────────────────────────────────────

        public static void Emit(T eventData)
        {
            _isEmitting = true;

            for (int i = _bindings.Count - 1; i >= 0; i--)
            {
                var binding = _bindings[i];

                // Auto-cleanup destroyed owner
                if (!binding.IsAlive)
                {
                    _bindings.RemoveAt(i);
                    continue;
                }

                try
                {
                    binding.Handler?.Invoke(eventData);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EventBus<{typeof(T).Name}>] Handler threw: {e.Message}\n{e.StackTrace}");
                }
            }

            _isEmitting = false;

            // Xử lý defer removal
            if (_toRemove.Count > 0)
            {
                foreach (var b in _toRemove)
                    _bindings.Remove(b);
                _toRemove.Clear();
            }

            // Log vào history
            EventHistory.Record(typeof(T).Name, eventData.ToString());
        }

        // ─── Utility ─────────────────────────────────────────────

        public static int SubscriberCount => _bindings.Count;

        private static bool IsAlreadySubscribed(Action<T> handler)
        {
            bool dup = _bindings.Exists(b => b.Handler == handler);
            if (dup) Debug.LogWarning($"[EventBus<{typeof(T).Name}>] Duplicate subscription detected");
            return dup;
        }

        private static void SortByPriority()
            => _bindings.Sort((a, b) => b.Priority.CompareTo(a.Priority));
    }
}
