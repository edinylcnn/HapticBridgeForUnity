# logi-plugin

[English](README.md) · [Türkçe](README.tr.md)

Logi Options+ için companion plugin — bir pipe server açar, gelen event'leri host SDK haptic waveform'larına çevirip MX Master 4'e gönderir.

> Resmi olmayan, topluluk plugin'i. Logitech ile bağlantılı değildir.

## Gereksinimler

1. **Logi Options+** kurulu olmalı (plugin service + `PluginApi.dll` sağlar)
   - Windows: `C:\Program Files\Logi\LogiPluginService\PluginApi.dll`
   - macOS: `/Applications/Utilities/LogiPluginService.app/Contents/MonoBundle/PluginApi.dll`
2. **.NET 8 SDK**
3. **LogiPluginTool** global .NET aracı (paketleme için):

   ```bash
   dotnet tool install --global LogiPluginTool
   ```

## Dizin yapısı

```
logi-plugin/
├── src/
│   ├── Plugin.cs                               ← Loupedeck.Plugin entry
│   ├── Application.cs                          ← ClientApplication companion
│   ├── PipeServer.cs                           ← Named Pipe / Unix socket sunucusu
│   ├── HapticMapper.cs                         ← event → waveform eşleme
│   ├── PluginLog.cs                            ← SDK log wrapper
│   ├── HapticBridgeForUnity.Plugin.csproj
│   └── package/
│       ├── metadata/
│       │   ├── LoupedeckPackage.yaml           ← manifest (plugin4, HasHapticsMapping)
│       │   └── Icon256x256.png
│       └── events/
│           ├── DefaultEventSource.yaml         ← 15 waveform event'i
│           └── extra/
│               └── eventMapping.yaml           ← haptic UI kaydı (haptics block)
└── build/                                      ← .lplug4 çıktıları
```

## Geliştirme

```bash
cd logi-plugin/src
dotnet build -c Release
```

csproj otomatik olarak plugin service'in `Plugins/` dizinine bir `.link` dosyası yazar ve reload tetikler — host plugin'i anında hot-reload eder.

## Paketleme (.lplug4)

```bash
cd logi-plugin/src
dotnet build -c Release
logiplugintool pack ../bin/Release ../build/HapticBridgeForUnity_0.1.1.lplug4
```

Çıktı: `logi-plugin/build/HapticBridgeForUnity_<version>.lplug4` — çift tıklanabilir kurulum dosyası.

## Release

`PluginApi.dll` Logi Options+ ile geldiği için CI'da alınamaz. Release'ler repo root'tan lokal olarak kesilir:

```bash
./scripts/release-plugin.sh 0.2.0
```

Script şunları yapar:
1. Manifest version'unu senkronize eder, commit'i push'lar
2. `dotnet build -c Release` çalıştırır
3. `.lplug4`'ü paketler
4. `plugin-v<version>` tag'ını oluşturup push'lar
5. `.lplug4`'ü asset olarak ekleyip GitHub Release açar

Gereksinimler: `dotnet`, `logiplugintool`, `gh` (GitHub CLI), temiz çalışma ağacı.

## Nasıl çalışır?

1. `Load()` → 15 haptic waveform'u `PluginEvents.AddEvent` ile kaydeder, pipe server'ı başlatır
2. Unity client pipe'a bir event adı yazar (ör. `"success"`)
3. `HapticMapper` onu SDK waveform'una çevirir (`success → completed`)
4. `PluginEvents.RaiseEvent("completed")` → MX Master 4 haptic pulse tetiklenir

## Haptic UI kaydı

İki dosya uyuşmazsa Logi Options+ plugin'i **Haptic Feedback** ekranında göstermez:

1. `LoupedeckPackage.yaml` → `pluginCapabilities` listesinde **`HasHapticsMapping`** (plural "s") olmalı. `HasHapticMapping` tek başına yeterli değil.
2. `events/extra/eventMapping.yaml` → plugin'in kaydettiği her waveform için `DEFAULT: <waveform>` eşlemesi içeren bir `haptics:` block'u.

İkisi olmadan event'ler pipe'a düşer ama cihaz titremez — kullanıcı Logi Options+'tan bunları haptic çıkışına bağlayamaz.

## macOS notu

`.NET`'in `NamedPipeServerStream`'i macOS'ta `$TMPDIR/CoreFxPipe_<name>` yolunda bir Unix Domain Socket açar. `Dispose()` socket dosyasını silmez ve bu, yeni plugin instance'ının bind etmesini bloklar (`IO_AllPipeInstancesAreBusy`). `PipeServer` bu hatayı yakalayıp socket'ı explicit unlink edip retry yapıyor.

## Waveform'lar

15 SDK waveform'u: `sharp_collision`, `sharp_state_change`, `knock`, `damp_collision`, `mad`, `ringing`, `subtle_collision`, `completed`, `jingle`, `damp_state_change`, `firework`, `happy_alert`, `wave`, `angry_alert`, `square`.

Unity tarafı ya `HapticEvent` enum'unu (9 generic event) kullanır, ya da `TriggerRaw("firework")` ile doğrudan waveform adı gönderir.

SDK dokümantasyonu: https://logitech.github.io/actions-sdk-docs/
