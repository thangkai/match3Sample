namespace EventLib.Example
{
    // ═══════════════════════════════════════════════════════════
    //  GAMEPLAY EVENTS
    // ═══════════════════════════════════════════════════════════

    public struct OnLevelStarted : IEvent
    {
        public int levelId;
    }

    public struct OnLevelCompleted : IEvent
    {
        public int levelId;
        public int stars;       // 1-3
        public int score;
        public float duration;  // giây
    }

    public struct OnLevelFailed : IEvent
    {
        public int levelId;
        public string reason;   // "timeout", "no_moves", etc.
    }

    // ═══════════════════════════════════════════════════════════
    //  PLAYER EVENTS
    // ═══════════════════════════════════════════════════════════

    public struct OnPlayerDied : IEvent
    {
        public int score;
    }

    public struct OnScoreChanged : IEvent
    {
        public int previous;
        public int current;
        public int delta;
    }

    // ═══════════════════════════════════════════════════════════
    //  ECONOMY EVENTS
    // ═══════════════════════════════════════════════════════════

    public struct OnCurrencyChanged : IEvent
    {
        public string type;     // "coin", "gem"
        public long previous;
        public long current;
        public long delta;
    }

    public struct OnItemAdded : IEvent
    {
        public string itemId;
        public int quantity;
    }

    public struct OnItemUsed : IEvent
    {
        public string itemId;
    }

    // ═══════════════════════════════════════════════════════════
    //  UI EVENTS
    // ═══════════════════════════════════════════════════════════

    public struct OnScreenOpened : IEvent
    {
        public string screenName;
    }

    public struct OnScreenClosed : IEvent
    {
        public string screenName;
    }

    // ═══════════════════════════════════════════════════════════
    //  SYSTEM EVENTS
    // ═══════════════════════════════════════════════════════════

    public struct OnGamePaused : IEvent { }
    public struct OnGameResumed : IEvent { }

    public struct OnSaveCompleted : IEvent
    {
        public int slotIndex;
    }

    public struct OnSettingsChanged : IEvent
    {
        public string changedKey; // "volume", "language", etc.
    }
}
