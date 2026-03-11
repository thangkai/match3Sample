# 📡 EventLib — Unity Event Bus System

Global event bus không cần reference, hỗ trợ 2 kiểu dùng.

---

## 2 cách dùng

### Cách 1: Generic Type (khuyên dùng)
Type-safe, IDE autocomplete, dễ refactor.

```csharp
// 1. Define event (struct)
public struct OnLevelCompleted : IEvent {
    public int levelId;
    public int stars;
}

// 2. Subscribe
EventBus<OnLevelCompleted>.On(HandleLevelComplete, this); // auto-unsubscribe khi this destroy
EventBus<OnLevelCompleted>.On(HandleLevelComplete);       // unsubscribe thủ công
EventBus<OnLevelCompleted>.Once(e => Debug.Log(e.levelId)); // chỉ nhận 1 lần

// 3. Emit
EventBus<OnLevelCompleted>.Emit(new OnLevelCompleted { levelId = 1, stars = 3 });

// 4. Unsubscribe thủ công
EventBus<OnLevelCompleted>.Off(HandleLevelComplete);
EventBus<OnLevelCompleted>.Clear(); // xóa hết
```

### Cách 2: String Key (linh hoạt)
Ít code hơn, không cần define struct.

```csharp
// Subscribe
StringEventBus.On("on_combo", OnCombo, this);
StringEventBus.On("on_game_over", OnGameOver, this); // không có data

// Emit
StringEventBus.Emit("on_combo", 7);      // có data
StringEventBus.Emit("on_game_over");     // không có data

// Unsubscribe
StringEventBus.Off("on_combo", OnCombo);
StringEventBus.ClearKey("on_combo");
StringEventBus.ClearAll();
```

---

## Khi nào dùng cái nào?

| | Generic Type | String Key |
|---|---|---|
| Type-safe | ✅ | ❌ |
| IDE autocomplete | ✅ | ❌ |
| Ít boilerplate | ❌ | ✅ |
| Truyền data phức tạp | ✅ | ⚠️ cần cast |
| Dùng cho | Gameplay events | UI events đơn giản |

---

## Tính năng

### Auto-unsubscribe
```csharp
// Truyền this vào → tự Off() khi GameObject bị Destroy
EventBus<OnPlayerDied>.On(HandleDeath, this);
```

### Priority
```csharp
// High chạy trước Normal, Normal trước Low
EventBus<OnCurrencyChanged>.On(UpdateUI, this, EventPriority.High);
EventBus<OnCurrencyChanged>.On(SaveData, this, EventPriority.Low);
```

### Once
```csharp
// Tự unsubscribe sau lần đầu tiên nhận event
EventBus<OnLevelStarted>.Once(e => ShowTutorial());
```

### Event History (debug)
```csharp
EventHistory.PrintAll(); // in toàn bộ lịch sử ra Console
// Chỉ active trong Editor và Development Build
```

---

## Tích hợp với AudioLib và SaveLib

```csharp
private void Awake()
{
    EventBus<OnLevelCompleted>.On(e => {
        AudioManager.Instance.PlaySfx(SfxID.LevelComplete);
        AudioManager.Instance.PlayBgm(BgmID.Victory);
        GameDataManager.Progress.CompleteLevel(e.levelId, e.stars, e.score);
        SaveManager.Instance.Save();
    }, this);

    EventBus<OnGamePaused>.On(_ => AudioManager.Instance.PauseBgm(), this);
    EventBus<OnGameResumed>.On(_ => AudioManager.Instance.ResumeBgm(), this);
}
```

---

## Cấu trúc file

```
EventSystem/
├── Core/
│   ├── IEvent.cs           ← Marker interface
│   ├── EventBinding.cs     ← Subscriber wrapper
│   ├── EventBus.cs         ← Generic type bus
│   ├── StringEventBus.cs   ← String key bus
│   └── EventHistory.cs     ← Debug history
└── Example/
    ├── GameEvents.cs       ← Định nghĩa tất cả event
    └── EventBusExample.cs  ← Ví dụ dùng thực tế
```
