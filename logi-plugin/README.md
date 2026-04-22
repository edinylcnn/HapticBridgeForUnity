# logi-plugin

[English](README.md) · [Türkçe](README.tr.md)

Companion plugin for Logi Options+ — opens a pipe server, forwards events to the host SDK's haptic waveforms on the MX Master 4.

> Unofficial community plugin. Not affiliated with Logitech.

## Requirements

1. **Logi Options+** installed (provides the plugin service + `PluginApi.dll`)
   - Windows: `C:\Program Files\Logi\LogiPluginService\PluginApi.dll`
   - macOS: `/Applications/Utilities/LogiPluginService.app/Contents/MonoBundle/PluginApi.dll`
2. **.NET 8 SDK**
3. **LogiPluginTool** global .NET tool (for packaging):

   ```bash
   dotnet tool install --global LogiPluginTool
   ```

## Layout

```
logi-plugin/
├── src/
│   ├── Plugin.cs                               ← Loupedeck.Plugin entry
│   ├── Application.cs                          ← ClientApplication companion
│   ├── PipeServer.cs                           ← Named Pipe / Unix socket server
│   ├── HapticMapper.cs                         ← event → waveform mapping
│   ├── PluginLog.cs                            ← SDK log wrapper
│   ├── HapticBridgeForUnity.Plugin.csproj
│   └── package/
│       ├── metadata/
│       │   ├── LoupedeckPackage.yaml           ← manifest (plugin4, HasHapticsMapping)
│       │   └── Icon256x256.png
│       └── events/
│           ├── DefaultEventSource.yaml         ← 15 waveform events
│           └── extra/
│               └── eventMapping.yaml           ← haptic UI registration (haptics block)
└── build/                                      ← .lplug4 output
```

## Development

```bash
cd logi-plugin/src
dotnet build -c Release
```

The csproj automatically writes a `.link` file into the plugin service's `Plugins/` directory and triggers a reload, so the host hot-reloads the plugin.

## Packaging (.lplug4)

```bash
cd logi-plugin/src
dotnet build -c Release
logiplugintool pack ../bin/Release ../build/HapticBridgeForUnity_0.1.1.lplug4
```

Output: `logi-plugin/build/HapticBridgeForUnity_<version>.lplug4` — double-clickable installer.

## Releasing

`PluginApi.dll` ships with Logi Options+, so it cannot be fetched in CI. Releases are cut locally from the repo root:

```bash
./scripts/release-plugin.sh 0.2.0
```

The script:
1. Syncs the manifest version and pushes the commit
2. Runs `dotnet build -c Release`
3. Packs the `.lplug4`
4. Creates and pushes the `plugin-v<version>` tag
5. Creates a GitHub Release with the `.lplug4` attached as an asset

Requirements: `dotnet`, `logiplugintool`, `gh` (GitHub CLI), a clean working tree.

## How it works

1. `Load()` registers the 15 haptic waveforms via `PluginEvents.AddEvent` and starts the pipe server.
2. The Unity client writes an event name (e.g. `"success"`) to the pipe.
3. `HapticMapper` resolves it to an SDK waveform (e.g. `success → completed`).
4. `PluginEvents.RaiseEvent("completed")` fires the haptic pulse on the MX Master 4.

## Haptic UI registration

Two files must line up or Logi Options+ will not show the plugin on its **Haptic Feedback** screen:

1. `LoupedeckPackage.yaml` → `pluginCapabilities` must include **`HasHapticsMapping`** (note the plural "s"). `HasHapticMapping` alone is not enough.
2. `events/extra/eventMapping.yaml` → a `haptics:` block with `DEFAULT: <waveform>` for every waveform the plugin registers.

Without both, events still flow through the pipe but the mouse never vibrates, because the user has no way to enable them in Logi Options+.

## macOS note

`.NET`'s `NamedPipeServerStream` opens a Unix Domain Socket at `$TMPDIR/CoreFxPipe_<name>` on macOS. `Dispose()` does not remove the socket file, which blocks the next plugin instance from binding (`IO_AllPipeInstancesAreBusy`). `PipeServer` catches that error, unlinks the socket explicitly, and retries.

## Waveforms

15 SDK waveforms: `sharp_collision`, `sharp_state_change`, `knock`, `damp_collision`, `mad`, `ringing`, `subtle_collision`, `completed`, `jingle`, `damp_state_change`, `firework`, `happy_alert`, `wave`, `angry_alert`, `square`.

The Unity side can use the `HapticEvent` enum (9 generic events) or `TriggerRaw("firework")` to send a waveform name directly.

SDK docs: https://logitech.github.io/actions-sdk-docs/
