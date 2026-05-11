import { Component, NgZone, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpParams } from '@angular/common/http';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { firstValueFrom, forkJoin } from 'rxjs';
import * as L from 'leaflet';
import { BasvuruService } from '../../../services/basvuru.service';
import { PiyasaEkraniComponent } from '../piyasa-ekrani/piyasa-ekrani.component';
import { Il, Ilce, SirketTipi, DokumanItem, BasvuruCreateRequest } from '../../../models/basvuru.model';
import { vergiNoValidator, tcknValidator, telefonValidator, emailValidator, vergiNoUniquenessValidator, IsTelefonValidator } from '../../../validators/custom.validators';

interface NominatimSearchResult {
  lat: string;
  lon: string;
  display_name: string;
}

interface NominatimReverseResult {
  display_name?: string;
}

@Component({
  selector: 'app-basvuru-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, PiyasaEkraniComponent],
  templateUrl: './basvuru-form.component.html'
})
export class BasvuruFormComponent implements OnInit, OnDestroy {
  form!: FormGroup;

  // Adım yönetimi
  mevcutAdim = 1;
  toplamAdim = 3;
  adimlar = [
    { no: 1, baslik: 'İşyeri Bilgileri',    ikon: 'domain' },
    { no: 2, baslik: 'Adres ve Konum',      ikon: 'location_on' },
    { no: 3, baslik: 'Evrak Yükleme',       ikon: 'upload_file' },
  ];

  // Lookup
  iller: Il[] = [];
  ilceler: Ilce[] = [];
  sirketTipleri: SirketTipi[] = [];

  // Harita
  harita: L.Map | null = null;
  marker: L.CircleMarker | null = null;
  private haritaEl: HTMLElement | null = null;
  seciliEnlem: number | null = null;
  seciliBoylam: number | null = null;
  konumSecildi = false;
  haritaAramaMetni = '';
  konumAraniyor = false;
  konumAramaHatasi = '';

  // Dokümanlar
  dokumanlar: DokumanItem[] = [
    { tip: 'VERGI_LEVHASI',         ad: 'Vergi Levhası',          aciklama: 'Güncel yılı kapsayan onaylı suret',  ikon: 'description',    zorunlu: true,  dosya: null, onizleme: null },
    { tip: 'IMZA_SIRKULERI',        ad: 'İmza Sirküleri',         aciklama: 'Yetkili kişilerin imza beyannamesi', ikon: 'signature',      zorunlu: true,  dosya: null, onizleme: null },
    { tip: 'TICARET_SICIL',         ad: 'Ticaret Sicil Gazetesi', aciklama: 'Kuruluş ve son unvan değişikliği',   ikon: 'account_balance', zorunlu: false, dosya: null, onizleme: null },
    { tip: 'KIMLIK',                ad: 'Kimlik Belgesi',         aciklama: 'Yetkili kişi T.C. kimlik belgesi',  ikon: 'badge',          zorunlu: false, dosya: null, onizleme: null },
    { tip: 'UYE_ISYERI_SOZLESMESI', ad: 'Üye İşyeri Sözleşmesi', aciklama: 'İmzalanmış sözleşme',               ikon: 'contract',       zorunlu: false, dosya: null, onizleme: null },
  ];

  gonderiliyor = false;
  basariMesaji = '';
  hataMesaji = '';

  constructor(
    private fb: FormBuilder,
    private basvuruService: BasvuruService,
    private ngZone: NgZone,
    private http: HttpClient
  ) {}

  ngOnInit() {
    this.formOlustur();
    this.lookupGetir();
  }

  ngOnDestroy() {
    this.harita?.remove();
    this.harita = null;
    this.marker = null;
    this.haritaEl = null;
  }

  private formOlustur() {
    this.form = this.fb.group({
      // Adım 1 — İşyeri Bilgileri
      sirketTipiId:    [1, Validators.required],
      firmaAdi:        ['', Validators.required],
      vergiNoTckn: ['', { validators: [Validators.required, vergiNoValidator()], asyncValidators: [vergiNoUniquenessValidator(this.basvuruService)], updateOn: 'blur' }],
      vergiDairesi:    [''],
      yetkiliTckn:     ['', tcknValidator()],
      yetkiliAdSoyad:  [''],
      cepTelefon:      ['', [Validators.required, telefonValidator()]],
      evTelefon:       ['', telefonValidator()],
      isTelefon:       ['', IsTelefonValidator()],
      email:           ['', [Validators.required, emailValidator()]],
      webAdres:        [''],
      isKategorisi:    [''],
      tahminiAylikCiro:[null, Validators.min(0)],
      // Adım 2 — Adres ve Konum
      adres:           ['', [Validators.required, Validators.minLength(10)]],
      ilId:            [null, Validators.required],
      ilceId:          [null, Validators.required],
      postaKodu:       [''],
    });
  }

  private lookupGetir() {
    forkJoin({ iller: this.basvuruService.getIller(), sirketTipleri: this.basvuruService.getSirketTipleri() }).subscribe({
      next: ({ iller, sirketTipleri }) => { this.iller = iller; this.sirketTipleri = sirketTipleri; },
      error: () => { this.hataMesaji = 'Sayfa verileri yüklenemedi.'; }
    });
  }

  ilSecildi(event: Event) {
    const ilId = +(event.target as HTMLSelectElement).value;
    this.form.patchValue({ ilceId: null });
    this.ilceler = [];
    if (!ilId) return;
    this.basvuruService.getIlceler(ilId).subscribe({ next: d => { this.ilceler = d; } });
  }

  // ── Adım navigasyonu ──────────────────────────────────────────────────────
  sonrakiAdim() {
    // Adım 1 alanlarını kontrol et
    if (this.mevcutAdim === 1) {
      const adim1Alanlar = ['sirketTipiId','firmaAdi','vergiNoTckn','yetkiliTckn','cepTelefon','evTelefon','isTelefon','email'];
      adim1Alanlar.forEach(alan => this.form.get(alan)?.markAsTouched());
      const gecersiz = adim1Alanlar.some(alan => this.form.get(alan)?.invalid);
      if (gecersiz) { this.hataMesaji = 'Lütfen zorunlu alanları doldurun ve hatalı telefonları düzeltin.'; return; }
    }
    if (this.mevcutAdim === 2) {
      const adim2Alanlar = ['adres','ilId','ilceId'];
      adim2Alanlar.forEach(alan => this.form.get(alan)?.markAsTouched());
      const gecersiz = adim2Alanlar.some(alan => this.form.get(alan)?.invalid);
      if (gecersiz) { this.hataMesaji = 'Lütfen adres bilgilerini doldurun.'; return; }
      if (!this.konumSecildi) { this.hataMesaji = 'Lütfen haritadan konum seçin.'; return; }
      // Adım 2 geçince haritayı başlat (DOM hazır olsun diye)
    }
    this.hataMesaji = '';
    this.mevcutAdim = Math.min(this.mevcutAdim + 1, this.toplamAdim);
    if (this.mevcutAdim === 2) {
      setTimeout(() => this.haritaBaslat(), 300);
    }
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  oncekiAdim() {
    this.hataMesaji = '';
    this.mevcutAdim = Math.max(this.mevcutAdim - 1, 1);
    if (this.mevcutAdim === 2) {
      setTimeout(() => this.haritaBaslat(), 300);
    }
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  // ── OpenStreetMap / Leaflet ───────────────────────────────────────────────
  haritaBaslat() {
    this.ngZone.runOutsideAngular(() => this.haritaOlustur());
  }

  private haritaOlustur() {
    const mapEl = document.getElementById('leaflet-map');
    if (!mapEl) return;

    this.haritaAramaMetni ||= `${this.form.get('adres')?.value ?? ''}`.trim();

    if (this.harita && this.haritaEl === mapEl) {
      this.harita.invalidateSize();
      return;
    }

    if (this.harita) {
      this.harita.remove();
      this.harita = null;
      this.marker = null;
    }

    this.haritaEl = mapEl;
    const baslangicKonumu: L.LatLngExpression =
      this.seciliEnlem !== null && this.seciliBoylam !== null
        ? [this.seciliEnlem, this.seciliBoylam]
        : [41.0082, 28.9784];

    this.harita = L.map(mapEl, {
      center: baslangicKonumu,
      zoom: this.seciliEnlem !== null && this.seciliBoylam !== null ? 16 : 12,
      zoomControl: true
    });

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19,
      attribution: '&copy; OpenStreetMap contributors'
    }).addTo(this.harita);

    this.harita.on('click', (e: L.LeafletMouseEvent) => {
      this.ngZone.run(() => this.konumSec(e.latlng.lat, e.latlng.lng));
    });

    if (this.seciliEnlem !== null && this.seciliBoylam !== null) {
      this.konumuHaritadaGoster(this.seciliEnlem, this.seciliBoylam, false);
    }

    setTimeout(() => this.harita?.invalidateSize(), 0);
  }

  private konumSec(lat: number, lng: number) {
    this.konumuHaritadaGoster(lat, lng);
    this.konumAramaHatasi = '';
    this.koordinattanAdresGetir(lat, lng);
  }

  async adresleKonumAra() {
    const sorgu = this.konumSorgusuOlustur();
    if (!sorgu) {
      this.konumAramaHatasi = 'Haritada aramak için bir adres yazın.';
      return;
    }

    this.konumAraniyor = true;
    this.konumAramaHatasi = '';

    try {
      const params = new HttpParams()
        .set('format', 'jsonv2')
        .set('limit', '1')
        .set('addressdetails', '1')
        .set('countrycodes', 'tr')
        .set('accept-language', 'tr')
        .set('q', sorgu);

      const sonuclar = await firstValueFrom(
        this.http.get<NominatimSearchResult[]>('https://nominatim.openstreetmap.org/search', { params })
      );
      const sonuc = sonuclar[0];

      if (!sonuc) {
        this.konumAramaHatasi = 'Adres bulunamadı. Daha açık bir adres deneyin.';
        return;
      }

      const lat = Number(sonuc.lat);
      const lng = Number(sonuc.lon);
      if (!Number.isFinite(lat) || !Number.isFinite(lng)) {
        this.konumAramaHatasi = 'Seçilen adres için konum alınamadı.';
        return;
      }

      this.adresAlaniniGuncelle(sonuc.display_name);
      this.konumAramaHatasi = '';
      this.konumuHaritadaGoster(lat, lng);
    } catch {
      this.konumAramaHatasi = 'Adres aranırken bir sorun oluştu. Lütfen tekrar deneyin.';
    } finally {
      this.konumAraniyor = false;
    }
  }

  private async koordinattanAdresGetir(lat: number, lng: number) {
    try {
      const params = new HttpParams()
        .set('format', 'jsonv2')
        .set('lat', String(lat))
        .set('lon', String(lng))
        .set('zoom', '18')
        .set('addressdetails', '1')
        .set('accept-language', 'tr');

      const sonuc = await firstValueFrom(
        this.http.get<NominatimReverseResult>('https://nominatim.openstreetmap.org/reverse', { params })
      );

      if (sonuc?.display_name) {
        this.konumAramaHatasi = '';
        this.adresAlaniniGuncelle(sonuc.display_name);
      }
    } catch {
      this.konumAramaHatasi = 'Konum seçildi ancak adres bilgisi alınamadı.';
    }
  }

  private konumuHaritadaGoster(lat: number, lng: number, yakinlastir = true) {
    this.seciliEnlem = lat;
    this.seciliBoylam = lng;
    this.konumSecildi = true;

    if (!this.harita) return;

    if (this.marker) {
      this.marker.setLatLng([lat, lng]);
    } else {
      this.marker = L.circleMarker([lat, lng], {
        radius: 10,
        color: '#ffffff',
        weight: 3,
        fillColor: '#705D00',
        fillOpacity: 1
      }).addTo(this.harita);
    }

    this.harita.panTo([lat, lng]);
    if (yakinlastir) {
      this.harita.setZoom(Math.max(this.harita.getZoom(), 16));
    }
  }

  private adresAlaniniGuncelle(adres: string | undefined) {
    const temizAdres = (adres || '').replace(/\s*,\s*Türkiye$/i, '').trim();
    if (!temizAdres) return;

    this.haritaAramaMetni = temizAdres;
    const adresKontrol = this.form.get('adres');
    adresKontrol?.setValue(temizAdres);
    adresKontrol?.markAsDirty();
    adresKontrol?.updateValueAndValidity();
  }

  private konumSorgusuOlustur(): string {
    const adres = this.haritaAramaMetni.trim() || `${this.form.get('adres')?.value ?? ''}`.trim();
    const il = this.iller.find(x => x.id === Number(this.form.get('ilId')?.value))?.ilAdi;
    const ilce = this.ilceler.find(x => x.id === Number(this.form.get('ilceId')?.value))?.ilceAdi;

    return [adres, ilce, il, 'Türkiye'].filter(Boolean).join(', ');
  }

  // ── Doküman ───────────────────────────────────────────────────────────────
  dosyaSec(dok: DokumanItem, event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    const dosya = input.files[0];
    if (dosya.size > 5 * 1024 * 1024) { alert(`${dok.ad}: Dosya 5 MB'ı aşamaz.`); input.value = ''; return; }
    if (!['application/pdf','image/jpeg','image/png'].includes(dosya.type)) { alert(`${dok.ad}: Yalnızca PDF, JPEG veya PNG.`); input.value = ''; return; }
    dok.dosya = dosya;
    if (dosya.type.startsWith('image/')) {
      const reader = new FileReader();
      reader.onload = e => { dok.onizleme = e.target?.result as string; };
      reader.readAsDataURL(dosya);
    } else { dok.onizleme = null; }
  }

  dosyaKaldir(dok: DokumanItem) { dok.dosya = null; dok.onizleme = null; }

  // ── Gönder ────────────────────────────────────────────────────────────────
  async gonder() {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      this.hataMesaji = 'Lütfen formdaki hatalı alanları düzeltin.';
      return;
    }

    const eksik = this.dokumanlar.filter(d => d.zorunlu && !d.dosya);
    if (eksik.length) { this.hataMesaji = `Zorunlu belgeler eksik: ${eksik.map(d => d.ad).join(', ')}`; return; }
    this.gonderiliyor = true; this.hataMesaji = ''; this.basariMesaji = '';
    const payload: BasvuruCreateRequest = {
      ...this.form.value,
      cepTelefon: this.telefonuTemizle(this.form.get('cepTelefon')?.value),
      evTelefon: this.telefonuTemizle(this.form.get('evTelefon')?.value),
      isTelefon: this.telefonuTemizle(this.form.get('isTelefon')?.value),
      enlem: this.seciliEnlem,
      boylam: this.seciliBoylam
    };
    this.basvuruService.basvuruOlustur(payload).subscribe({
      next: async (res) => {
        for (const dok of this.dokumanlar.filter(d => d.dosya)) {
          try { await this.basvuruService.dokumanYukle(res.id, dok.tip, dok.dosya!).toPromise(); } catch {}
        }
        this.basariMesaji = `Başvurunuz alındı! Başvuru No: ${res.id}`;
        this.gonderiliyor = false;
        window.scrollTo({ top: 0, behavior: 'smooth' });
      },
      error: err => { this.hataMesaji = err.message || 'Gönderilemedi.'; this.gonderiliyor = false; }
    });
  }

  // ── Yardımcılar ───────────────────────────────────────────────────────────
  hataVar(alan: string): boolean { const c = this.form.get(alan); return !!(c?.invalid && (c?.dirty || c?.touched)); }

  hataGetir(alan: string): string {
    const e = this.form.get(alan)?.errors;
    if (!e) return '';
    if (e['required'])       return 'Bu alan zorunludur.';
    if (e['vergiNoFormat'])  return e['vergiNoFormat'];
    if (e['vergiNoMevcut'])  return e['vergiNoMevcut'];
    if (e['tcknFormat'])     return e['tcknFormat'];
    if (e['telefonFormat'])  return e['telefonFormat'];
    if (e['IsTelefonFormat']) return e['IsTelefonFormat'];
    if (e['emailFormat'])    return e['emailFormat'];
    if (e['minlength'])      return `En az ${e['minlength'].requiredLength} karakter.`;
    return 'Geçersiz değer.';
  }

  asyncBekliyor(alan: string): boolean { return this.form.get(alan)?.status === 'PENDING'; }

  private telefonuTemizle(deger: string | null | undefined): string | null {
    const temizDeger = deger?.replace(/\s/g, '').trim() ?? '';
    return temizDeger || null;
  }

  inputCss(alan: string): string {
    const base = 'w-full px-4 py-3 bg-[#F3F3F3] rounded-xl text-sm focus:outline-none focus:ring-2 focus:bg-white transition-all';
    const ctrl = this.form.get(alan);
    if (ctrl?.invalid && (ctrl?.dirty || ctrl?.touched)) return `${base} ring-2 ring-red-200 bg-red-50`;
    if (ctrl?.valid && ctrl?.dirty) return `${base} ring-2 ring-green-200`;
    return `${base} focus:ring-[#FFD700]/50`;
  }

  get adimIlerleme(): number { return ((this.mevcutAdim - 1) / (this.toplamAdim - 1)) * 100; }
}
