<p align="center">
  <img src="docs/images/icon.png" width="128" alt="HapticBridge for Unity" />
</p>

<h1 align="center">HapticBridge for Unity</h1>

<p align="center">
  <a href="README.md">English</a> · <a href="README.tr.md">Türkçe</a>
</p>

<p align="center"><sub>Resmi olmayan, topluluk tarafından geliştirilen bir plugin. Logitech veya Unity Technologies ile bağlantılı/onaylı değildir.</sub></p>

> **Logitech MX Master 4** için Unity oyunlarına tek satırda haptic feedback — port yok, sertifika yok, domain ayarı yok.

![MX Master 4 haptic feedback](docs/images/hero.webp)

```csharp
using HapticBridge;

HapticsBridge.Trigger(HapticEvent.Click);
```

---

## Kurulum (4 adım)

1. **[Logi Options+](https://www.logitech.com/software/logi-options-plus)** kurulu olsun (MX Master 4 ile zaten geliyor).
2. [Releases](https://github.com/edinylcnn/LogiHapticsForUnity/releases) sayfasından **`HapticBridgeForUnity_x.y.lplug4`**'ü indir → çift tıkla → Logi Options+ companion plugin'i kurar.
3. Unity'de **Package Manager**'ı aç → `+` → **Add package from git URL**:

   ```
   https://github.com/edinylcnn/LogiHapticsForUnity.git?path=/unity-package
   ```

4. İstediğin yerden çağır:

   ```csharp
   HapticsBridge.Trigger(HapticEvent.Click);
   ```

Hiçbir şey hissetmiyorsan Logi Options+ → MX Master 4 → **Haptic Feedback** sekmesine gir ve **"HapticBridge for Unity"** toggle'ının açık olduğunu doğrula (kurulumdan sonra default açık gelir).

---

## Event'ler

Oyundan bağımsız dokuz genel amaçlı event, 15 haptic waveform'una eşlenmiş. İstersen genişlet.

| Event | Waveform | Tipik kullanım |
|-------|----------|----------------|
| `Click` | subtle_collision | UI etkileşimi, buton, menü |
| `Confirm` | jingle | Onay, satın alma, kaydet |
| `Success` | completed | Başarılı işlem, doğru cevap |
| `Failure` | mad | Hatalı aksiyon, yanlış giriş |
| `Warning` | damp_state_change | Uyarı, kritik eşik |
| `Notification` | happy_alert | Mesaj, ipucu, bildirim |
| `Achievement` | firework | Ödül, seviye atlama, nadir olay |
| `ImpactLight` | subtle_collision | Hafif temas, küçük etki |
| `ImpactMedium` | sharp_collision | Standart darbe, çarpışma |

Belirli bir waveform istiyorsan 15 waveform id'sinden birini doğrudan gönder:

```csharp
HapticsBridge.TriggerRaw("firework");
```

Tam liste: `sharp_collision`, `sharp_state_change`, `knock`, `damp_collision`, `mad`, `ringing`, `subtle_collision`, `completed`, `jingle`, `damp_state_change`, `firework`, `happy_alert`, `wave`, `angry_alert`, `square`.

---

## Nasıl çalışır

```
Unity oyunu
    │
    │  Windows:      Named Pipe \\.\pipe\HapticBridgeForUnity
    │  macOS/Linux:  Unix Domain Socket  $TMPDIR/CoreFxPipe_HapticBridgeForUnity
    ▼
HapticBridge companion plugin  (Logi Options+ içinde çalışır — .lplug4)
    │
    │  PluginEvents.RaiseEvent(waveform)
    ▼
MX Master 4 haptic aktuatör
```

Repo iki bileşenli bir monorepo:

| Klasör | İçerik | Dağıtım |
|---|---|---|
| [`logi-plugin/`](logi-plugin/) | Logi Options+ companion plugin (C#) | `.lplug4` — GitHub Releases |
| [`unity-package/`](unity-package/) | Unity Package — `com.edinylcnn.hapticbridge` | Unity Package Manager (git URL) |

### Neden macOS/Linux'ta Unix Domain Socket?

Unity'nin Mono runtime'ı `NamedPipeClientStream`'i `.NET`'in plugin tarafında kullandığı `$TMPDIR/CoreFxPipe_<name>` path'ine götürmüyor. İstemci bu soyutlamayı atlayıp socket'a doğrudan bağlanıyor — böylece iki taraf aynı path'te buluşuyor. Windows'ta klasik Named Pipe.

---

## Platform desteği

| Platform | Haptic | Not |
|---|:-:|---|
| Windows Standalone + Editor | ✅ | `NamedPipeClientStream` |
| macOS Standalone + Editor | ✅ | `UnixDomainSocketEndPoint` |
| Linux Standalone + Editor | ✅ | `UnixDomainSocketEndPoint` |
| iOS / Android / WebGL / Console | ➖ | Sessiz fallback, oyun çalışmaya devam eder |

Unity runtime asmdef'i `noEngineReferences: true` — saf .NET, Mono ve IL2CPP'nin ikisinde de derlenir.

Companion plugin kurulu değilse `Connect` 200 ms timeout'a düşer, `HapticsBridge.IsAvailable` `false` döner, çağrılar sessizce skip edilir — oyun hiçbir zaman çökmez.

---

## Editor test paneli

**Window → HapticBridge → Test Panel** bir pencere açar: pipe bağlantı durumu, son hata, temp path ve her event için tetik butonu — sahneye bir şey koymaya gerek yok.

<p align="center">
  <img src="docs/images/test-panel.png" width="380" alt="HapticBridge test paneli" />
</p>

---

## Kendi oyununda dağıtırken

Son kullanıcıya da companion plugin lazım. İyi bir örüntü:

```csharp
if (!HapticsBridge.IsAvailable)
{
    // .lplug4 release'ine götüren tek seferlik bir kurulum ipucu göster.
}
```

Sonra `HapticsBridge.Trigger(...)` çağrılarını serbest bırak — plugin olsa da olmasa da güvenli.

---

## Desteklenen cihaz

**Sadece MX Master 4.** Host SDK'nın haptic API'si şu an başka bir cihaz expose etmiyor.

---

## Lisans

MIT — detay için [LICENSE](LICENSE).

HapticBridge for Unity, resmi olmayan, topluluk tarafından geliştirilen bir projedir. "Logi", "Logitech", "MX Master", "Logi Options+" Logitech'in; "Unity" Unity Technologies'in tescilli markalarıdır. Bu proje adı geçen firmalardan hiçbiriyle bağlantılı veya onları onaylı değildir.
