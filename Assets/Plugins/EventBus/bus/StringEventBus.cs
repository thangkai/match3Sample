using System;
using System.Collections.Generic;
using UnityEngine;

namespace EventLib
{
    /// <summary>
    /// Event Bus dùng string key — linh hoạt hơn, ít type-safe hơn.
    /// Phù hợp cho event đơn giản không cần truyền data phức tạp.
    ///
    /// USAGE:
    ///   // Subscribe
    ///   StringEventBus.On("on_level_complete", OnLevelComplete);
    ///   StringEventBus.On("on_coin_collect", OnCoinCollect, this);
    ///
    ///   // Emit (có hoặc không có data)
    ///   StringEventBus.Emit("on_level_complete");
    ///   StringEventBus.Emit("on_coin_collect", 100);
    ///
    ///   // Unsubscribe
    ///   StringEventBus.Off("on_level_complete", OnLevelComplete);
    /// </summary>
    public static class StringEventBus
    {
        private static readonly Dictionary<string, List<StringBinding>> _bindings = new();
        private static bool _isEmitting;
        private static readonly List<(string key, StringBinding binding)> _toRemove = new();

        // ─── Subscribe ────────────────────────────────────────────

        public static void On(string key, Action<object> handler, UnityEngine.Object owner = null,
            EventPriority priority = EventPriority.Normal)
        {
            EnsureKey(key);
            _bindings[key].Add(new StringBinding(handler, owner, priority));
            _bindings[key].Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        /// <summary>Subscribe không có data</summary>
        public static void On(string key, Action handler, UnityEngine.Object owner = null,
            EventPriority priority = EventPriority.Normal)
            => On(key, _ => handler(), owner, priority);

        public static void Once(string key, Action<object> handler)
        {
            Action<object> wrapper = null;
            wrapper = (data) =>
            {
                handler(data);
                Off(key, wrapper);
            };
            On(key, wrapper);
        }

        // ─── Unsubscribe ─────────────────────────────────────────

        public static void Off(string key, Action<object> handler)
        {
            if (!_bindings.ContainsKey(key)) return;

            if (_isEmitting)
            {
                var found = _bindings[key].Find(b => b.Handler == handler);
                if (found != null) _toRemove.Add((key, found));
                return;
            }
            _bindings[key].RemoveAll(b => b.Handler == handler);
        }

        public static void ClearKey(string key)
        {
            if (_bindings.ContainsKey(key)) _bindings[key].Clear();
        }

        public static void ClearAll() => _bindings.Clear();

        // ─── Emit ────────────────────────────────────────────────

        public static void Emit(string key, object data = null)
        {
            if (!_bindings.TryGetValue(key, out var list)) return;

            _isEmitting = true;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                var binding = list[i];
                if (!binding.IsAlive) { list.RemoveAt(i); continue; }

                try { binding.Handler?.Invoke(data); }
                catch (Exception e)
                {
                    Debug.LogError($"[StringEventBus:{key}] Handler threw: {e.Message}");
                }
            }

            _isEmitting = false;

            if (_toRemove.Count > 0)
            {
                foreach (var (k, b) in _toRemove)
                    if (_bindings.ContainsKey(k)) _bindings[k].Remove(b);
                _toRemove.Clear();
            }

            EventHistory.Record(key, data?.ToString() ?? "");
        }

        // ─── Utility ─────────────────────────────────────────────

        public static int GetSubscriberCount(string key)
            => _bindings.TryGetValue(key, out var list) ? list.Count : 0;

        private static void EnsureKey(string key)
        {
            if (!_bindings.ContainsKey(key))
                _bindings[key] = new List<StringBinding>();
        }
    }

    internal class StringBinding
    {
        public Action<object> Handler { get; }
        public EventPriority Priority { get; }
        public bool IsAlive           => _owner == null || _owner != null && _owner;
        private readonly UnityEngine.Object _owner;

        public StringBinding(Action<object> handler, UnityEngine.Object owner, EventPriority priority)
        {
            Handler  = handler;
            Priority = priority;
            _owner   = owner;
        }
    }
}
