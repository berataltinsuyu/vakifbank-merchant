# 🏦 VakıfBank Üye İşyeri Başvuru Sistemi

Üye işyeri başvurularını dijital ortamda toplamak, yönetmek ve takip etmek için geliştirilmiş kurumsal web uygulaması.

---

## Teknoloji Yığını

### Backend
| Teknoloji | Kullanım |
|-----------|----------|
| .NET 9 / ASP.NET Core Web API | REST API |
| Entity Framework Core 9 (DB First) | ORM |
| PostgreSQL / Supabase | Veritabanı |
| FluentValidation | Model doğrulama |
| Swagger | API dokümantasyonu |

### Frontend
| Teknoloji | Kullanım |
|-----------|----------|
| Angular 17+ | SPA Framework |
| TypeScript | Dil |
| Tailwind CSS 3 | Stil |
| RxJS | Reaktif programlama |
| OpenStreetMap + Leaflet | Konum seçimi |

### Harici Servisler
| Servis | Kullanım |
|--------|----------|
| open.er-api.com | Anlık döviz kurları |
| Nominatim | Adres arama ve ters geocoding |
| Supabase Storage | Özel doküman depolama ve signed URL |

---

## Mimari

```
┌─────────────────────────────────────────────┐
│              Angular Frontend               │
│  Login  │  Başvuru Formu  │  Geçmiş Listesi │
└──────────────────┬──────────────────────────┘
                   │ HTTP / JSON
┌──────────────────▼──────────────────────────┐
│          ASP.NET Core Web API               │
│  Controllers → Services → Repositories      │
│  Entity Framework Core (DB First)           │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│       PostgreSQL / Supabase Database        │
└─────────────────────────────────────────────┘
```

**Yaklaşım:** DB First · Repository Pattern · DTO Katmanı

---

## Özellikler

### Başvuru Formu — 3 Adımlı
- **Adım 1:** Firma kimlik ve iletişim bilgileri
- **Adım 2:** İl/ilçe seçimi, OpenStreetMap + Leaflet üzerinden konum işaretleme
- **Adım 3:** PDF/JPEG/PNG formatında çoklu doküman yükleme (Supabase Storage)

### Validasyonlar
- Vergi no: tam 10 rakam + backend uniqueness kontrolü (async, 600ms debounce)
- TCKN: tam 11 rakam
- Türk cep telefonu formatı
- E-posta format kontrolü
- Zorunlu alan kontrolü

### Başvuru Geçmişi
- VKN, firma adı, e-posta, durum ve tarih aralığına göre filtreleme
- Tablo başlıklarına tıklayarak sıralama
- Sayfalama (10/25/50 kayıt)
- Duruma göre istatistik kartları
- Sağdan kayan detay paneli (dokümanlar ve tarihçe dahil)

### Piyasa Ekranı
- USD/TRY, EUR/TRY, GBP/TRY kurları
- 30 saniyede bir otomatik güncelleme

---

## API Endpoint'leri

### Başvuru

| Method | URL | Açıklama |
|--------|-----|----------|
| `GET` | `/api/basvuru/liste` | Özet liste |
| `GET` | `/api/basvuru/{id}/detay` | Tam detay |
| `POST` | `/api/basvuru` | Yeni başvuru |
| `PATCH` | `/api/basvuru/{id}/durum` | Durum güncelle |
| `GET` | `/api/basvuru/vergino-kontrol` | Uniqueness kontrolü |

### Diğer

| Method | URL | Açıklama |
|--------|-----|----------|
| `POST` | `/api/dokuman/yukle` | Dosya yükle |
| `GET` | `/api/lookup/iller` | İl listesi |
| `GET` | `/api/lookup/ilceler/{ilId}` | İlçe listesi |
| `GET` | `/api/lookup/sirkettipleri` | Şirket tipleri |
| `GET` | `/api/doviz` | Döviz kurları |

### Başvuru Durumları

| Değer | Anlamı |
|-------|--------|
| `Bekliyor` | Yeni başvuru |
| `Incelemede` | İnceleniyor |
| `Onaylandi` | Onaylandı |
| `Reddedildi` | Reddedildi |
| `EksikBelge` | Eksik belge |

---

## Veritabanı Şeması

```
Iller ──────────────────┐
Ilceler ────────────────┤
SirketTipleri ──────────┼──► Basvurular
                             │
                        ┌────┴───────────────────┐
                        ▼                        ▼
                BasvuruDokumanlari          BasvuruTarihce
```

---

## Proje Yapısı

### Backend
```
VbMerchant/
├── Controllers/
├── Data/Entities/
├── DTOs/
├── Repositories/
├── Services/
├── Validators/
└── Program.cs
```

### Frontend
```
src/app/
├── components/
│   ├── auth/login/
│   ├── basvuru/
│   │   ├── basvuru-form/
│   │   ├── basvuru-listesi/
│   │   └── piyasa-ekrani/
│   └── layout/
├── models/
├── pipes/
├── services/
└── validators/
```

---

## Geliştirme Notları

- **Yerel inceleme konfigürasyonu:** Proje Supabase PostgreSQL ve private Supabase Storage kullanır. Teknik inceleme için yerel PostgreSQL kurulumu veya migration çalıştırma gerekmez; gerçek `back-end/appsettings.Local.json` dosyası özel olarak paylaşılır.
- **Örnek config:** `back-end/appsettings.Local.example.json` dosyası placeholder değerler içerir. Gerekirse bu dosya `back-end/appsettings.Local.json` olarak kopyalanıp gerçek değerler özel kanaldan girilir.
- **Gizli bilgiler:** `appsettings.Local.json` git'e eklenmemelidir. Supabase connection string, service role key ve JWT secret hiçbir zaman commit edilmemelidir.
- **FluentValidation + Async:** `MustAsync` ASP.NET otomatik validasyon pipeline'ıyla uyumsuz. Vergi no uniqueness kontrolü controller'da `AnyAsync` ile yapılıyor.
- **DB First:** Tablo yapısı değişince `dotnet ef dbcontext scaffold` tekrar çalıştırılmalı.
- **Döviz Kurları:** DB'ye kaydedilmez, her istekte anlık çekilir.
- **Dokümanlar:** Dosyalar private Supabase Storage bucket'a backend üzerinden yüklenir; görüntüleme için backend signed URL üretir.

---

*ASP.NET Core 9 · Angular 17 · PostgreSQL · Entity Framework Core 9*
