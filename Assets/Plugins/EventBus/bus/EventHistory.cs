using System;
using System.Collections.Generic;
using UnityEngine;

namespace EventLib
{
    [Serializable]
    public class EventRecord
    {
        public string eventName;
        public string data;
        public string timestamp;
    }

    /// <summary>
    /// Lưu lịch sử các event đã emit để debug.
    /// Chỉ active khi UNITY_EDITOR hoặc DEVELOPMENT_BUILD.
    /// </summary>
    public static class EventHistory
    {
        private const int MAX_RECORDS = 100;

        private static readonly Queue<EventRecord> _records = new();

        public static IEnumerable<EventRecord> Records => _records;
        public static int Count => _records.Count;

        public static void Record(string eventName, string data)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_records.Count >= MAX_RECORDS)
                _records.Dequeue();

            _records.Enqueue(new EventRecord
            {
                eventName = eventName,
                data      = data,
                timestamp = DateTime.Now.ToString("HH:mm:ss.fff"),
            });
#endif
        }

        public static void Clear() => _records.Clear();

        public static void PrintAll()
        {
            foreach (var r in _records)
                Debug.Log($"[{r.timestamp}] {r.eventName} → {r.data}");
        }
    }
}
