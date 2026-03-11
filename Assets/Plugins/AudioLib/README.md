# 🔊 AudioLib — Unity Audio System

Hệ thống audio reusable cho Unity, hỗ trợ:
- BGM với cross-fade
- SFX với AudioSource pooling
- 3D Spatial Audio
- Volume settings tự động lưu

## Yêu cầu
- Unity 2021.3+
- Addressables package

---

## Setup (5 bước)

### 1. Import vào project
Thêm vào `Packages/manifest.json` hoặc copy thư mục vào `Assets/Plugins/AudioLib`.

### 2. Tạo AudioRegistry asset
```
Right-click trong Project → Create → AudioLib → Audio Registry
```

### 3. Thêm AudioID cho game của bạn
Mở `AudioID.cs`, thêm entry vào enum:
```csharp
public enum BgmID {
    None = 0,
    MainMenu,   // ← thêm tên BGM game của bạn
    Gameplay,
}

public enum SfxID {
    None = 0,
    ButtonClick,   // ← thêm tên SFX game của bạn
}
```

### 4. Điền data vào AudioRegistry
Trong Inspector của AudioRegistry asset:
- **BGM Entries**: Thêm entry, điền `id` và `addressableKey` (key trong Addressable group)
- **SFX Entries**: Tương tự, bật `spatial` nếu cần 3D audio

### 5. Đặt AudioManager vào scene
```
Create Empty GameObject → đặt tên "AudioManager"
→ Add Component → AudioManager
→ Kéo AudioRegistry asset vào field "Registry"
```

---

## Sử dụng

### BGM
```csharp
// Play BGM, tự cross-fade nếu đang có track khác
AudioManager.Instance.PlayBgm(BgmID.Gameplay);

// Stop với fade out 2 giây
AudioManager.Instance.StopBgm(fadeDuration: 2f);

// Pause / Resume
AudioManager.Instance.PauseBgm();
AudioManager.Instance.ResumeBgm();
```

### SFX
```csharp
// Play SFX 2D đơn giản
AudioManager.Instance.PlaySfx(SfxID.ButtonClick);

// Play và giữ handle để control
var handle = AudioManager.Instance.PlaySfx(SfxID.CoinCollect);
handle.Pause();
handle.Resume();
handle.Stop();

// Play 3D tại vị trí
AudioManager.Instance.PlaySfxAt(SfxID.Explosion, transform.position);

// Play 3D gắn theo object (di chuyển cùng)
AudioManager.Instance.PlaySfxAttached(SfxID.EngineLoop, vehicleTransform);
```

### Volume
```csharp
// Set volume (0 → 1)
AudioManager.Instance.SetMasterVolume(0.8f);
AudioManager.Instance.SetBgmVolume(0.5f);
AudioManager.Instance.SetSfxVolume(1f);

// Toggle mute
AudioManager.Instance.ToggleMute();
AudioManager.Instance.ToggleBgmMute();
```

---

## Cấu trúc file

```
AudioSystem/
├── Core/
│   ├── AudioManager.cs          ← Entry point duy nhất
│   ├── BGMPlayer.cs             ← Cross-fade BGM
│   ├── SFXPlayer.cs             ← Pool + play SFX
│   └── AudioMixerController.cs  ← Volume + PlayerPrefs
├── Data/
│   ├── AudioID.cs               ← Enum định nghĩa audio
│   └── AudioRegistry.cs         ← Map ID → Addressable key
└── Runtime/
    └── AudioHandle.cs           ← Control handle
```

---

## Tái sử dụng cho game mới

1. Sửa `AudioID.cs` — thêm BGM/SFX mới
2. Tạo `AudioRegistry` asset mới — điền addressable key
3. Đặt `AudioManager` vào scene
4. Gọi `AudioManager.Instance.PlaySfx(...)` ở bất kỳ đâu

**Không cần sửa bất kỳ file nào khác.**
