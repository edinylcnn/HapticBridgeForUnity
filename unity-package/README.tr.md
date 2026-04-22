# unity-package — `com.edinylcnn.hapticbridge`

[English](README.md) · [Türkçe](README.tr.md)

Unity tarafındaki UPM paketi. Windows'ta Named Pipe, macOS/Linux'ta Unix Domain Socket üzerinden companion plugin'e ([`logi-plugin`](../logi-plugin)) event gönderir.

> Resmi olmayan, topluluk paketi. Logitech veya Unity Technologies ile bağlantılı değildir.

## Kurulum

Unity Package Manager → `+` → **Add package from git URL**:

```
https://github.com/edinylcnn/LogiHapticsForUnity.git?path=/unity-package
```

## Kullanım

```csharp
using HapticBridge;

// Tek satır
HapticsBridge.Trigger(HapticEvent.Click);

// Ham waveform (15 SDK waveform'undan biri)
HapticsBridge.TriggerRaw("firework");

// Manuel DI
IHapticService haptic = new HapticBridgeService();
haptic.Trigger(HapticEvent.Success);
```

Companion plugin kurulu mu diye kontrol etmek için `HapticsBridge.IsAvailable` — `false` ise kullanıcıya kurulum rehberini gösterebilirsin. Çağrılar her durumda güvenli: fallback no-op, hiçbir şey çökmez.

## Editor test paneli

**Window → HapticBridge → Test Panel**

- Pipe bağlantı durumu + son hata
- Temp path
- Her event için tetik butonu

Plugin çalışmıyorsa `Could not connect — ...` mesajı burada görünür.

## Dizin yapısı

```
unity-package/
├── Runtime/
│   ├── IHapticService.cs
│   ├── HapticEvent.cs                ← 9 generic event
│   ├── HapticBridgeService.cs        ← pipe (Windows) / Unix socket (mac/linux) client
│   ├── NullHapticsService.cs         ← fallback
│   ├── HapticsBridge.cs              ← static façade
│   └── HapticBridge.Runtime.asmdef
├── Editor/
│   ├── HapticsBridgeChecker.cs       ← test paneli
│   └── HapticBridge.Editor.asmdef
├── Samples~/
│   └── BasicUsage/                   ← 1-5 tuşlarıyla event demo
└── package.json
```

## Event'ler

| Event | Waveform |
|---|---|
| `Click` | subtle_collision |
| `Confirm` | jingle |
| `Success` | completed |
| `Failure` | mad |
| `Warning` | damp_state_change |
| `Notification` | happy_alert |
| `Achievement` | firework |
| `ImpactLight` | subtle_collision |
| `ImpactMedium` | sharp_collision |

`TriggerRaw`'a geçirebileceğin ham waveform'lar: `sharp_collision`, `sharp_state_change`, `knock`, `damp_collision`, `mad`, `ringing`, `subtle_collision`, `completed`, `jingle`, `damp_state_change`, `firework`, `happy_alert`, `wave`, `angry_alert`, `square`.

## Platform desteği

| Platform | Haptic |
|---|:-:|
| Windows Standalone + Editor | ✅ |
| macOS Standalone + Editor | ✅ |
| Linux Standalone + Editor | ✅ |
| iOS / Android / WebGL / Console | ➖ (no-op fallback) |

Runtime asmdef'i `noEngineReferences: true` — saf .NET, Mono ve IL2CPP'de derlenir.
