import { Component, ElementRef, NgZone, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { BasvuruService } from '../../../services/basvuru.service';
import { PiyasaEkraniComponent } from '../piyasa-ekrani/piyasa-ekrani.component';
import { Il, Ilce, SirketTipi, DokumanItem, BasvuruCreateRequest } from '../../../models/basvuru.model';
import { vergiNoValidator, tcknValidator, telefonValidator, emailValidator, vergiNoUniquenessValidator, IsTelefonValidator } from '../../../validators/custom.validators';

declare const google: any;

@Component({
  selector: 'app-basvuru-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, PiyasaEkraniComponent],
  templateUrl: './basvuru-form.component.html'
})
export class BasvuruFormComponent implements OnInit {
  @ViewChild('haritaAramaInput') haritaAramaInput?: ElementRef<HTMLInputElement>;

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
  harita: any = null;
  marker: any = null;
  geocoder: any = null;
  autocomplete: any = null;
  private haritaEl: HTMLElement | null = null;
  private autocompleteInputEl: HTMLInputElement | null = null;
  private sonOtomatikAdres = '';
  seciliEnlem: number | null = null;
  seciliBoylam: number | null = null;
  konumSecildi = false;
  konumAramaMetni = '';
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

  constructor(private fb: FormBuilder, private basvuruService: BasvuruService, private ngZone: NgZone) {}

  ngOnInit() {
    this.formOlustur();
    this.lookupGetir();
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
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  // ── Google Maps ───────────────────────────────────────────────────────────
  haritaBaslat() {
    const kontrol = setInterval(() => {
      if (typeof google !== 'undefined') {
        clearInterval(kontrol);
        this.ngZone.run(() => this.haritaOlustur());
      }
    }, 300);
  }

  private haritaOlustur() {
    const mapEl = document.getElementById('google-map');
    if (!mapEl) return;

    if (!this.konumAramaMetni.trim()) {
      this.konumAramaMetni = this.form.get('adres')?.value ?? '';
    }

    if (!this.harita || this.haritaEl !== mapEl) {
      this.haritaEl = mapEl;
      this.geocoder = new google.maps.Geocoder();
      this.harita = new google.maps.Map(mapEl, {
        center: { lat: 41.0082, lng: 28.9784 }, zoom: 12,
        mapTypeControl: false, streetViewControl: false, fullscreenControl: false,
        styles: [
          { elementType: 'geometry', stylers: [{ color: '#f5f5f5' }] },
          { elementType: 'labels.text.fill', stylers: [{ color: '#616161' }] },
          { featureType: 'road', elementType: 'geometry', stylers: [{ color: '#ffffff' }] },
          { featureType: 'water', elementType: 'geometry', stylers: [{ color: '#c9c9c9' }] }
        ]
      });
      this.harita.addListener('click', (e: any) => {
        this.ngZone.run(() => this.konumSec(e.latLng.lat(), e.latLng.lng()));
      });

      if (this.seciliEnlem !== null && this.seciliBoylam !== null) {
        this.konumuHaritadaGoster(this.seciliEnlem, this.seciliBoylam, false);
      }
    }

    this.haritaAramaKutusuBaslat();
  }

  private konumSec(lat: number, lng: number) {
    this.konumuHaritadaGoster(lat, lng);
    this.koordinattanAdresGetir(lat, lng);
  }

  adresleKonumAra() {
    const sorgu = this.konumSorgusuOlustur();
    if (!sorgu) {
      this.konumAramaHatasi = 'Haritada göstermek için bir adres yazın.';
      return;
    }
    if (!this.geocoder) {
      this.konumAramaHatasi = 'Harita servisi henüz hazır değil.';
      return;
    }

    this.konumAramaHatasi = '';
    this.geocoder.geocode(
      { address: sorgu, componentRestrictions: { country: 'TR' } },
      (results: any, status: string) => {
        this.ngZone.run(() => {
          if (status !== 'OK' || !results?.length) {
            this.konumAramaHatasi = 'Adres bulunamadı. Daha açık bir adres deneyin.';
            return;
          }

          const sonuc = results[0];
          const konum = sonuc.geometry?.location;
          if (!konum) {
            this.konumAramaHatasi = 'Seçilen adres için konum alınamadı.';
            return;
          }

          this.konumAramaHatasi = '';
          this.adresAlanlariniGuncelle(sonuc.formatted_address, true);
          this.konumuHaritadaGoster(konum.lat(), konum.lng());
        });
      }
    );
  }

  private konumuHaritadaGoster(lat: number, lng: number, yakinlastir = true) {
    this.seciliEnlem = lat;
    this.seciliBoylam = lng;
    this.konumSecildi = true;

    if (this.marker) {
      this.marker.setMap(this.harita);
      this.marker.setPosition({ lat, lng });
    } else {
      this.marker = new google.maps.Marker({
        position: { lat, lng }, map: this.harita, title: 'İşyeri Konumu',
        icon: { path: google.maps.SymbolPath.CIRCLE, scale: 10, fillColor: '#705D00', fillOpacity: 1, strokeColor: '#ffffff', strokeWeight: 3 }
      });
    }

    this.harita.panTo({ lat, lng });
    if (yakinlastir) {
      this.harita.setZoom(Math.max(this.harita.getZoom() || 0, 16));
    }
  }

  private haritaAramaKutusuBaslat() {
    const inputEl = this.haritaAramaInput?.nativeElement;
    if (!inputEl || !google.maps.places) return;
    if (this.autocompleteInputEl === inputEl) return;

    this.autocompleteInputEl = inputEl;
    this.autocomplete = new google.maps.places.Autocomplete(inputEl, {
      componentRestrictions: { country: 'tr' },
      fields: ['formatted_address', 'geometry', 'name'],
      types: ['geocode']
    });

    this.autocomplete.addListener('place_changed', () => {
      this.ngZone.run(() => {
        const yer = this.autocomplete?.getPlace();
        const konum = yer?.geometry?.location;
        if (!konum) {
          this.konumAramaHatasi = 'Seçilen yer için konum alınamadı.';
          return;
        }

        this.konumAramaHatasi = '';
        this.adresAlanlariniGuncelle(yer.formatted_address || yer.name || this.konumAramaMetni, true);
        this.konumuHaritadaGoster(konum.lat(), konum.lng());
      });
    });
  }

  private koordinattanAdresGetir(lat: number, lng: number) {
    if (!this.geocoder) return;

    this.geocoder.geocode({ location: { lat, lng } }, (results: any, status: string) => {
      this.ngZone.run(() => {
        if (status !== 'OK' || !results?.length) return;
        this.konumAramaHatasi = '';
        this.adresAlanlariniGuncelle(results[0].formatted_address);
      });
    });
  }

  private adresAlanlariniGuncelle(adres: string, zorla = false) {
    const temizAdres = (adres || '').replace(/\s*,\s*Türkiye$/i, '').trim();
    if (!temizAdres) return;

    this.konumAramaMetni = temizAdres;
    const adresKontrol = this.form.get('adres');
    const mevcutAdres = `${adresKontrol?.value ?? ''}`.trim();
    const kullaniciManuelAdresGirdi =
      !!mevcutAdres &&
      adresKontrol?.dirty &&
      mevcutAdres !== this.sonOtomatikAdres;

    if (zorla || !kullaniciManuelAdresGirdi) {
      adresKontrol?.setValue(temizAdres);
      this.sonOtomatikAdres = temizAdres;
    }
  }

  private konumSorgusuOlustur(): string {
    const serbestMetin = this.konumAramaMetni.trim() || `${this.form.get('adres')?.value ?? ''}`.trim();
    const il = this.iller.find(x => x.id === Number(this.form.get('ilId')?.value))?.ilAdi;
    const ilce = this.ilceler.find(x => x.id === Number(this.form.get('ilceId')?.value))?.ilceAdi;

    return [serbestMetin, ilce, il, 'Türkiye'].filter(Boolean).join(', ');
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
