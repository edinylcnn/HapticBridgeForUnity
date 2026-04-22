# unity-package — `com.edinylcnn.hapticbridge`

[English](README.md) · [Türkçe](README.tr.md)

Unity-side UPM package. Sends events to the companion plugin ([`logi-plugin`](../logi-plugin)) over a Named Pipe (Windows) or a Unix Domain Socket (macOS/Linux).

> Unofficial community package. Not affiliated with Logitech or Unity Technologies.

## Install

Unity Package Manager → `+` → **Add package from git URL**:

```
https://github.com/edinylcnn/LogiHapticsForUnity.git?path=/unity-package
```

## Usage

```csharp
using HapticBridge;

// One-liner
HapticsBridge.Trigger(HapticEvent.Click);

// Raw waveform (one of the 15 SDK waveforms)
HapticsBridge.TriggerRaw("firework");

// Manual DI
IHapticService haptic = new HapticBridgeService();
haptic.Trigger(HapticEvent.Success);
```

Use `HapticsBridge.IsAvailable` to check whether the companion plugin is installed — if `false`, you can point the user to the install guide. Calls are safe either way: the fallback is a no-op, so nothing crashes.

## Editor test panel

**Window → HapticBridge → Test Panel**

- Pipe connection status + last error
- Temp path
- Per-event trigger buttons

Surfaces a `Could not connect — ...` message when the plugin is not running.

## Layout

```
unity-package/
├── Runtime/
│   ├── IHapticService.cs
│   ├── HapticEvent.cs                ← 9 generic events
│   ├── HapticBridgeService.cs        ← pipe (Windows) / Unix socket (mac/linux) client
│   ├── NullHapticsService.cs         ← fallback
│   ├── HapticsBridge.cs              ← static façade
│   └── HapticBridge.Runtime.asmdef
├── Editor/
│   ├── HapticsBridgeChecker.cs       ← test panel
│   └── HapticBridge.Editor.asmdef
├── Samples~/
│   └── BasicUsage/                   ← keys 1-5 trigger events
└── package.json
```

## Events

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

Raw waveforms you can pass to `TriggerRaw`: `sharp_collision`, `sharp_state_change`, `knock`, `damp_collision`, `mad`, `ringing`, `subtle_collision`, `completed`, `jingle`, `damp_state_change`, `firework`, `happy_alert`, `wave`, `angry_alert`, `square`.

## Platform support

| Platform | Haptic |
|---|:-:|
| Windows Standalone + Editor | ✅ |
| macOS Standalone + Editor | ✅ |
| Linux Standalone + Editor | ✅ |
| iOS / Android / WebGL / Console | ➖ (no-op fallback) |

The runtime asmdef sets `noEngineReferences: true` — plain .NET, compiles under Mono and IL2CPP.
