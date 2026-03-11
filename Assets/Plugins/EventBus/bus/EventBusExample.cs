using UnityEngine;
using EventLib;
using EventLib.Example;

/// <summary>
/// Ví dụ cách dùng EventBus trong game thực tế.
/// Tích hợp với AudioLib và SaveLib đã làm trước.
/// </summary>
public class EventBusExample : MonoBehaviour
{
    // ═══════════════════════════════════════════════════════════
    //  CÁCH 1: Generic Type (type-safe, khuyên dùng)
    // ═══════════════════════════════════════════════════════════

    private void Awake()
    {
        // Subscribe với owner = this → tự unsubscribe khi GameObject destroy
        EventBus<OnLevelCompleted>.On(HandleLevelCompleted, this);
        EventBus<OnCurrencyChanged>.On(HandleCurrencyChanged, this, EventPriority.High);
        EventBus<OnGamePaused>.On(HandleGamePaused, this);

        // Subscribe 1 lần
        EventBus<OnLevelStarted>.Once(e => Debug.Log($"First level started: {e.levelId}"));
    }

    private void HandleLevelCompleted(OnLevelCompleted e)
    {
        Debug.Log($"Level {e.levelId} done! Stars: {e.stars}, Score: {e.score}");

        // Tích hợp AudioLib
        // AudioManager.Instance.PlaySfx(e.stars == 3 ? SfxID.ComboLarge : SfxID.LevelComplete);
        // AudioManager.Instance.PlayBgm(BgmID.Victory);

        // Tích hợp SaveLib
        // GameDataManager.Progress.CompleteLevel(e.levelId, e.stars, e.score);
        // SaveManager.Instance.Save();
    }

    private void HandleCurrencyChanged(OnCurrencyChanged e)
    {
        Debug.Log($"[Currency] {e.type}: {e.previous} → {e.current} (Δ{e.delta})");
    }

    private void HandleGamePaused(OnGamePaused e)
    {
        // AudioManager.Instance.PauseBgm();
        Debug.Log("Game paused");
    }

    // ═══════════════════════════════════════════════════════════
    //  CÁCH 2: String Key (linh hoạt, ít code hơn)
    // ═══════════════════════════════════════════════════════════

    private void Start()
    {
        // Subscribe với data
        StringEventBus.On("on_combo", OnCombo, this);

        // Subscribe không có data
        StringEventBus.On("on_game_over", OnGameOver, this);
    }

    private void OnCombo(object data)
    {
        int comboCount = (int)data;
        Debug.Log($"Combo x{comboCount}!");
        // AudioManager.Instance.PlaySfx(comboCount >= 5 ? SfxID.ComboLarge : SfxID.ComboSmall);
    }

    private void OnGameOver()
    {
        Debug.Log("Game Over!");
    }

    // ═══════════════════════════════════════════════════════════
    //  EMIT — từ bất kỳ script nào
    // ═══════════════════════════════════════════════════════════

    // Ví dụ: gọi từ GameplayManager khi hoàn thành level
    public void SimulateLevelComplete()
    {
        // Generic type
        EventBus<OnLevelCompleted>.Emit(new OnLevelCompleted
        {
            levelId  = 1,
            stars    = 3,
            score    = 5000,
            duration = 45.2f,
        });

        // Cập nhật currency → emit event
        EventBus<OnCurrencyChanged>.Emit(new OnCurrencyChanged
        {
            type     = "coin",
            previous = 100,
            current  = 200,
            delta    = 100,
        });

        // String key
        StringEventBus.Emit("on_combo", 7);
    }

    // ─── Debug ───────────────────────────────────────────────

    [ContextMenu("Print Event History")]
    private void PrintHistory() => EventHistory.PrintAll();

    [ContextMenu("Simulate Level Complete")]
    private void TestEmit() => SimulateLevelComplete();
}
