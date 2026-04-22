<p align="center">
  <img src="docs/images/icon.png" width="128" alt="LogiHapticsForUnity" />
</p>

<h1 align="center">LogiHapticsForUnity</h1>

<p align="center">
  <a href="README.md">English</a> · <a href="README.tr.md">Türkçe</a>
</p>

> One-line haptic feedback for Unity games on the **Logitech MX Master 4** — no ports, no certificates, no domain setup.

![MX Master 4 haptic feedback](docs/images/hero.webp)

```csharp
using LogiHaptics;

LogiHapticsUnity.Trigger(HapticEvent.Click);
```

---

## Install (4 steps)

1. Make sure **[Logi Options+](https://www.logitech.com/software/logi-options-plus)** is installed (it ships with the MX Master 4).
2. Download **`LogiHapticsUnity_x.y.lplug4`** from [Releases](https://github.com/edinylcnn/LogiHapticsForUnity/releases) → double-click → Logi Options+ installs the plugin.
3. In Unity, open **Package Manager** → `+` → **Add package from git URL**:

   ```
   https://github.com/edinylcnn/LogiHapticsForUnity.git?path=/unity-package
   ```

4. Call it from anywhere:

   ```csharp
   LogiHapticsUnity.Trigger(HapticEvent.Click);
   ```

If you don't feel anything, check Logi Options+ → MX Master 4 → **Haptic Feedback** and confirm the **"LogiHaptics for Unity"** toggle is on (it is on by default after install).

---

## Events

Nine game-agnostic events mapped to Logi's haptic waveforms. Extend freely.

| Event | Waveform | Typical use |
|-------|----------|-------------|
| `Click` | subtle_collision | UI interaction, button, menu |
| `Confirm` | jingle | Confirm, purchase, save |
| `Success` | completed | Task completed, correct answer |
| `Failure` | mad | Wrong action, invalid input |
| `Warning` | damp_state_change | Warning, critical threshold |
| `Notification` | happy_alert | Message, tip, notification |
| `Achievement` | firework | Reward, level up, rare drop |
| `ImpactLight` | subtle_collision | Light touch, minor hit |
| `ImpactMedium` | sharp_collision | Standard impact, collision |

Need a specific waveform? Send one of the 15 SDK waveform ids directly:

```csharp
LogiHapticsUnity.TriggerRaw("firework");
```

Full waveform list: `sharp_collision`, `sharp_state_change`, `knock`, `damp_collision`, `mad`, `ringing`, `subtle_collision`, `completed`, `jingle`, `damp_state_change`, `firework`, `happy_alert`, `wave`, `angry_alert`, `square`.

---

## How it works

```
Unity game
    │
    │  Windows:      Named Pipe \\.\pipe\LogiHapticsUnity
    │  macOS/Linux:  Unix Domain Socket  $TMPDIR/CoreFxPipe_LogiHapticsUnity
    ▼
LogiHapticsUnity plugin  (runs inside Logi Options+ — .lplug4)
    │
    │  Loupedeck.Plugin / PluginEvents.RaiseEvent(waveform)
    ▼
MX Master 4 haptic actuator
```

The repo is a monorepo:

| Folder | Contents | Ships as |
|---|---|---|
| [`logi-plugin/`](logi-plugin/) | Logi Options+ plugin (C#, Loupedeck.Plugin SDK) | `.lplug4` — GitHub Releases |
| [`unity-package/`](unity-package/) | Unity Package — `com.logihapticsunity` | Unity Package Manager (git URL) |

### Why Unix Domain Socket on macOS/Linux?

Unity's Mono runtime does not target `$TMPDIR/CoreFxPipe_<name>` the way .NET's `NamedPipeClientStream` does on the plugin side. The client bypasses that abstraction and connects to the socket directly, which keeps the two sides in sync. Windows still uses a classic Named Pipe.

---

## Platform support

| Platform | Haptic | Notes |
|---|:-:|---|
| Windows Standalone + Editor | ✅ | `NamedPipeClientStream` |
| macOS Standalone + Editor | ✅ | `UnixDomainSocketEndPoint` |
| Linux Standalone + Editor | ✅ | `UnixDomainSocketEndPoint` |
| iOS / Android / WebGL / Console | ➖ | No-op fallback, game keeps running |

The Unity runtime assembly has `noEngineReferences: true` — it is plain .NET and compiles under both Mono and IL2CPP.

If the plugin is not installed, `Connect` times out in 200 ms, `LogiHapticsUnity.IsAvailable` returns `false`, and calls silently do nothing — your game never crashes.

---

## Editor test panel

**Window → LogiHaptics → Test Panel** opens a window that shows the pipe connection status, last error, temp path, and a trigger button per event — no scene setup needed.

<p align="center">
  <img src="docs/images/test-panel.png" width="380" alt="LogiHaptics test panel" />
</p>

---

## Shipping this in your game

End users need the plugin too. A good pattern:

```csharp
if (!LogiHapticsUnity.IsAvailable)
{
    // Show a one-time install hint with a link to the .lplug4 release.
}
```

Then call `LogiHapticsUnity.Trigger(...)` freely — it is safe whether the plugin is there or not.

---

## Device support

**MX Master 4 only.** Logi's haptic API currently does not expose any other device.

---

## License

MIT — see [LICENSE](LICENSE).
