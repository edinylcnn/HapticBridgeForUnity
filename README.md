# LogiHapticsForUnity

> MX Master 4 için Unity oyunlarına tek satırda haptic feedback desteği
> veren açık kaynak araç seti.

**Durum:** Çalışır — Faz 1-2 gerçek cihazda doğrulandı, Faz 3 release otomasyonu bekliyor.

---

## Kurulum (3 adım)

1. **[Logi Options+](https://www.logitech.com/software/logi-options-plus)** kurulu olsun (MX Master 4 ile zaten geliyor)
2. **GitHub Releases**'ten `LogiHapticsUnity_x.y.lplug4` indir → çift tıkla → Logi Options+ plugin'i kurar
3. **Unity Package Manager** → `+` → Add package from git URL:

   ```
   https://github.com/edinylcnn/LogiHapticsForUnity.git?path=/unity-package
   ```

4. Kodda:

   ```csharp
   LogiHapticsUnity.Trigger(HapticEvent.Click);
   ```

> **Haptic hissetmiyorsan** — Logi Options+ → MX Master 4 → **Haptic Feedback** sekmesinde **"LogiHaptics for Unity"** toggle'ının açık olduğunu doğrula (kurulumdan sonra default açık gelir ama kullanıcı daha önce kapatmış olabilir).

Firewall onayı yok. Domain yok. Sertifika yok. Port ayarı yok.

---

## Mimari

```
Unity Oyunu
    │
    │  Windows: Named Pipe  \\.\pipe\LogiHapticsUnity
    │  macOS/Linux: Unix Domain Socket  $TMPDIR/CoreFxPipe_LogiHapticsUnity
    ▼
LogiHapticsUnity Plugin  (Logi Options+ içinde — .lplug4)
    │
    │  PluginEvents.RaiseEvent(waveform)
    ▼
MX Master 4 haptic aktuatör
```

İki bileşenli monorepo:

| Klasör | İçerik | Dağıtım |
|---|---|---|
| [`logi-plugin/`](logi-plugin/) | Logi Options+ plugin (C#, Loupedeck.Plugin SDK) | `.lplug4` — GitHub Releases |
| [`unity-package/`](unity-package/) | Unity Package (UPM) — `com.logihapticsunity` | Git URL / OpenUPM |

### Neden Unix Domain Socket?

Unity'nin Mono runtime'ı macOS'ta `NamedPipeClientStream`'i `.NET`'in beklediği `$TMPDIR/CoreFxPipe_<name>` path'ine götürmüyor. `UnixDomainSocketEndPoint` ile doğrudan bağlanıp bu tutarsızlığı aşıyoruz. Windows'ta klasik Named Pipe kullanılıyor.

---

## Desteklenen Event'ler

Tür bağımsız, her oyunda kullanılabilecek genel amaçlı başlangıç seti:

| Event | Waveform | Tipik Kullanım |
|-------|----------|----------------|
| `Click` | subtle_collision | UI etkileşimi, buton, menü |
| `Confirm` | jingle | Onay, satın alma, kaydet |
| `Success` | completed | Başarılı işlem, görev tamamlama |
| `Failure` | mad | Hatalı aksiyon, yanlış giriş |
| `Warning` | damp_state_change | Uyarı, kritik eşik |
| `Notification` | happy_alert | Mesaj, bildirim, ipucu |
| `Achievement` | firework | Ödül, seviye atlama, nadir olay |
| `ImpactLight` | subtle_collision | Hafif temas, küçük etki |
| `ImpactMedium` | sharp_collision | Standart darbe, çarpışma |

Ham waveform ismi göndermek istersen:

```csharp
LogiHapticsUnity.TriggerRaw("firework");
```

15 SDK waveform'unun tam listesi: `sharp_collision`, `sharp_state_change`, `knock`, `damp_collision`, `mad`, `ringing`, `subtle_collision`, `completed`, `jingle`, `damp_state_change`, `firework`, `happy_alert`, `wave`, `angry_alert`, `square`.

---

## Kullanım

```csharp
using LogiHaptics;

// Tek satır — her yerden (lazy singleton)
LogiHapticsUnity.Trigger(HapticEvent.Click);

// Manuel DI için
IHapticService haptic = new LogiHapticsService();
haptic.Trigger(HapticEvent.Success);
```

Plugin kurulu değilse `Connect` 200ms timeout'a düşer, `LastError` set edilir, **oyun çökmez** — haptic sessizce skip edilir.

### Editor Test Paneli

Menü: **Window → LogiHaptics → Test Panel**

- Pipe connection durumu + Last error
- Her event için tek-tık tetik butonu
- Temp path gösterimi (debug için)

---

## Platform Matrisi

| Platform | Haptic | Not |
|---|:-:|---|
| Windows Standalone | ✅ | `NamedPipeClientStream` |
| macOS Standalone | ✅ | `UnixDomainSocketEndPoint` |
| Linux Standalone | ✅ | `UnixDomainSocketEndPoint` |
| Unity Editor (Win/Mac/Linux) | ✅ | Standalone ile aynı |
| iOS / Android / WebGL / Console | ➖ | Sessiz fallback, oyun çökmez |

Build'de `IL2CPP` ve `Mono` scripting backend'lerinin ikisinde de çalışır — runtime asmdef `noEngineReferences: true` ile saf .NET.

---

## Son Kullanıcı Dağıtımı

Oyununu yayınladığında son kullanıcı için kurulum adımı:

1. Logi Options+ kurulu (MX Master 4 kullanıcılarında zaten var)
2. `.lplug4` dosyanı indir + çift tıkla
3. Logi Options+ → MX Master 4 → Haptic Feedback → toggle

Bu adımları oyunun içinde bir "Haptic Ayarları" sayfasına yazabilirsin. `LogiHapticsUnity.IsAvailable` false ise kullanıcıya kurulum linki göster.

---

## Yol Haritası

- [x] **Faz 1 — Logi Plugin:** Loupedeck.Plugin SDK, Named Pipe server, event → waveform mapping, haptic UI kaydı (`HasHapticsMapping` + `eventMapping.yaml`), `.lplug4` build
- [x] **Faz 2 — Unity Package:** `HapticEvent` enum, Unix Domain Socket / Named Pipe client, static façade, Editor test paneli, Samples
- [ ] **Faz 3 — Paketleme & Dağıtım:** `plugin-v*` / `unity-v*` tag'lerinde GitHub Actions release, OpenUPM kaydı
- [ ] **Faz 4 — Dokümantasyon:** Kurulum gif'i, ekran görüntüleri, event/waveform ekleme rehberi

---

## Cihaz Uyumluluğu

Yalnızca **MX Master 4** desteklenir — Logi Actions SDK'nın haptic API'si şu an yalnızca bu cihazda çalışıyor.

---

## Lisans

MIT — detay için [LICENSE](LICENSE).
