using System;
using UnityEngine;

namespace EventLib
{
    public enum EventPriority
    {
        Low    = 0,
        Normal = 1,
        High   = 2,
    }

    /// <summary>
    /// Giữ một subscriber và metadata của nó.
    /// Hỗ trợ auto-unsubscribe khi MonoBehaviour bị destroy.
    /// </summary>
    public class EventBinding<T> where T : IEvent
    {
        public Action<T> Handler   { get; }
        public EventPriority Priority { get; }
        public bool IsAlive           => _owner == null || _owner != null && _owner;

        private readonly UnityEngine.Object _owner; // null = không cần track lifetime

        /// <summary>Subscribe không track lifetime (tự unsubscribe thủ công)</summary>
        public EventBinding(Action<T> handler, EventPriority priority = EventPriority.Normal)
        {
            Handler  = handler;
            Priority = priority;
            _owner   = null;
        }

        /// <summary>Subscribe với owner — tự unsubscribe khi owner bị destroy</summary>
        public EventBinding(Action<T> handler, UnityEngine.Object owner, EventPriority priority = EventPriority.Normal)
        {
            Handler  = handler;
            Priority = priority;
            _owner   = owner;
        }
    }
}
