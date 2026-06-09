import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe, NgIf } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { firstValueFrom, forkJoin } from 'rxjs';
import { BasvuruService } from '../../../services/basvuru.service';
import { CountByDurumPipe } from '../../../pipes/count-by-durum.pipe';
import { MinPipe } from '../../../pipes/min.pipe';

@Component({
  selector: 'app-basvuru-listesi',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, DatePipe, CountByDurumPipe, MinPipe],
  templateUrl: './basvuru-listesi.component.html'
})
export class BasvuruListesiComponent implements OnInit {
  basvurular: any[] = [];
  filtrelenmis: any[] = [];
  yukleniyor = true;
  detayYukleniyor = false;
  hata = '';
  excelYukleniyor = false;
  excelMesaji = '';
  excelMesajTipi: 'uyari' | 'hata' = 'uyari';
  private excelMesajZamanlayici?: ReturnType<typeof setTimeout>;

  aramaVergiNo = '';
  aramaFirma = '';
  aramaEmail = '';
  seciliDurum = '';
  baslangicTarihi = '';
  bitisTarihi = '';
  siralamaAlani = 'olusturmaTarihi';
  siralamaYonu: 'asc' | 'desc' = 'desc';

  sayfaBasi = 10;
  mevcutSayfa = 1;

  seciliBasvuru: any = null;
  detayAcik = false;

  durumlar = ['Bekliyor', 'Incelemede', 'Onaylandi', 'Reddedildi', 'EksikBelge'];
  durumEtiket: Record<string, string> = {
    'Bekliyor':   'Bekliyor',
    'Incelemede': 'İncelemede',
    'Onaylandi':  'Onaylandı',
    'Reddedildi': 'Reddedildi',
    'EksikBelge': 'Eksik Belge',
  };

  constructor(private basvuruService: BasvuruService) {}

  ngOnInit() {
    this.basvuruService.getListe().subscribe({
      next: data => {
        this.basvurular = data;
        this.filtrele();
        this.yukleniyor = false;
      },
      error: () => {
        this.hata = 'Başvurular yüklenemedi.';
        this.yukleniyor = false;
      }
    });
}
  filtrele() {
    this.mevcutSayfa = 1;
    let sonuc = this.basvurular.filter(b => {
      const v = !this.aramaVergiNo || b.vergiNoTckn?.toLowerCase().includes(this.aramaVergiNo.toLowerCase());
      const f = !this.aramaFirma || b.firmaAdi?.toLowerCase().includes(this.aramaFirma.toLowerCase()) || b.adSoyad?.toLowerCase().includes(this.aramaFirma.toLowerCase());
      const e = !this.aramaEmail || b.email?.toLowerCase().includes(this.aramaEmail.toLowerCase());
      const d = !this.seciliDurum || b.durum === this.seciliDurum;
      const bas = !this.baslangicTarihi || new Date(b.olusturmaTarihi) >= new Date(this.baslangicTarihi);
      const bit = !this.bitisTarihi || new Date(b.olusturmaTarihi) <= new Date(this.bitisTarihi);
      return v && f && e && d && bas && bit;
    });

    sonuc = sonuc.sort((a, b) => {
      const av = a[this.siralamaAlani] ?? '';
      const bv = b[this.siralamaAlani] ?? '';
      const cmp = av < bv ? -1 : av > bv ? 1 : 0;
      return this.siralamaYonu === 'asc' ? cmp : -cmp;
    });

    this.filtrelenmis = sonuc;
  }

  sirala(alan: string) {
    if (this.siralamaAlani === alan) {
      this.siralamaYonu = this.siralamaYonu === 'asc' ? 'desc' : 'asc';
    } else {
      this.siralamaAlani = alan;
      this.siralamaYonu = 'asc';
    }
    this.filtrele();
  }

  temizle() {
    this.aramaVergiNo = ''; this.aramaFirma = ''; this.aramaEmail = '';
    this.seciliDurum = ''; this.baslangicTarihi = ''; this.bitisTarihi = '';
    this.filtrele();
  }

  async excelIndir() {
    if (this.filtrelenmis.length === 0) {
      this.excelMesajGoster('İndirilecek başvuru bulunmuyor.', 'uyari');
      return;
    }

    this.excelYukleniyor = true;

    try {
      const detaylar = await firstValueFrom(forkJoin(
        this.filtrelenmis.map(b => this.basvuruService.getDetay(b.id))
      ));
      const XLSX = await import('xlsx');
      const satirlar = detaylar.map(b => ({
        'Başvuru ID': b.id ?? '',
        'Durum': this.durumEtiket[b.durum] || b.durum || '',
        'Başvuru Tarihi': this.tarihFormatla(b.olusturmaTarihi),
        'Şirket Tipi': b.sirketTipi || '',
        'Firma Ünvanı / Ad Soyad': b.firmaAdi || b.adSoyad || '',
        'Vergi No / TCKN': b.vergiNoTCKN || b.vergiNoTckn || '',
        'Vergi Dairesi': b.vergiDairesi || '',
        'Yetkili TCKN': b.yetkiliTckn || '',
        'Yetkili Ad Soyad': b.yetkiliAdSoyad || '',
        'Cep Telefonu': b.cepTelefon || '',
        'Ev Telefonu': b.evTelefon || '',
        'İş Telefonu': b.isTelefon || '',
        'E-posta': b.email || '',
        'Web Sitesi': b.webAdres || '',
        'Tam Adres': b.adres || '',
        'İl': b.ilAdi || '',
        'İlçe': b.ilceAdi || '',
        'Posta Kodu': b.postaKodu || '',
        'Enlem': b.enlem ?? '',
        'Boylam': b.boylam ?? '',
        'İş Kategorisi': b.isKategorisi || '',
        'Tahmini Aylık Ciro': b.tahminiAylikCiro ?? '',
      }));

      const calismaSayfasi = XLSX.utils.json_to_sheet(satirlar);
      calismaSayfasi['!cols'] = this.sutunGenislikleri(satirlar);

      const calismaKitabi = XLSX.utils.book_new();
      XLSX.utils.book_append_sheet(calismaKitabi, calismaSayfasi, 'Başvuru Geçmişi');
      XLSX.writeFile(calismaKitabi, 'basvuru-gecmisi-detayli.xlsx');
    } catch {
      this.excelMesajGoster('Excel dosyası hazırlanamadı. Lütfen tekrar deneyin.', 'hata');
    } finally {
      this.excelYukleniyor = false;
    }
  }

  get sayfaliListe(): any[] {
    const bas = (this.mevcutSayfa - 1) * this.sayfaBasi;
    return this.filtrelenmis.slice(bas, bas + this.sayfaBasi);
  }

  get toplamSayfa(): number {
    return Math.ceil(this.filtrelenmis.length / this.sayfaBasi);
  }

  get sayfaListesi(): number[] {
    return Array.from({ length: this.toplamSayfa }, (_, i) => i + 1);
  }

  sayfaDegistir(s: number) {
    if (s >= 1 && s <= this.toplamSayfa) { this.mevcutSayfa = s; window.scrollTo({ top: 0, behavior: 'smooth' }); }
  }


  detayAc(b: any) {
    this.seciliBasvuru = b;  
    this.detayAcik = true;
    this.detayYukleniyor = true;

    this.basvuruService.getDetay(b.id).subscribe({
      next: detay => {
        this.seciliBasvuru = detay;  
        this.detayYukleniyor = false;
      },
      error: () => {
        this.detayYukleniyor = false;
      }
    });
}

  detayKapat() { this.detayAcik = false; setTimeout(() => { this.seciliBasvuru = null; }, 300); }

  durumRengi(d: string): string {
    const r: Record<string, string> = {
      'Bekliyor': 'bg-yellow-100 text-yellow-800', 'Incelemede': 'bg-blue-100 text-blue-800',
      'Onaylandi': 'bg-green-100 text-green-800', 'Reddedildi': 'bg-red-100 text-red-800',
      'EksikBelge': 'bg-orange-100 text-orange-800',
    };
    return r[d] || 'bg-neutral-100 text-neutral-600';
  }

  durumIkon(d: string): string {
    const r: Record<string, string> = {
      'Bekliyor': 'schedule', 'Incelemede': 'manage_search',
      'Onaylandi': 'check_circle', 'Reddedildi': 'cancel', 'EksikBelge': 'warning',
    };
    return r[d] || 'help';
  }

  siralamaIkonu(alan: string): string {
    if (this.siralamaAlani !== alan) return 'unfold_more';
    return this.siralamaYonu === 'asc' ? 'arrow_upward' : 'arrow_downward';
  }

  get aktifFiltreSayisi(): number {
    return [this.aramaVergiNo, this.aramaFirma, this.aramaEmail,
            this.seciliDurum, this.baslangicTarihi, this.bitisTarihi].filter(f => !!f).length;
  }

  private tarihFormatla(tarih: string | null | undefined): string {
    if (!tarih) return '';

    const deger = new Date(tarih);
    if (Number.isNaN(deger.getTime())) return '';

    return new Intl.DateTimeFormat('tr-TR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    }).format(deger);
  }

  private sutunGenislikleri(satirlar: Record<string, unknown>[]): { wch: number }[] {
    const basliklar = Object.keys(satirlar[0]);

    return basliklar.map(baslik => ({
      wch: Math.min(
        50,
        Math.max(
          baslik.length + 2,
          ...satirlar.map(satir => String(satir[baslik] ?? '').length + 2)
        )
      )
    }));
  }

  private excelMesajGoster(mesaj: string, tip: 'uyari' | 'hata') {
    clearTimeout(this.excelMesajZamanlayici);
    this.excelMesaji = mesaj;
    this.excelMesajTipi = tip;
    this.excelMesajZamanlayici = setTimeout(() => {
      this.excelMesaji = '';
    }, 4000);
  }
}
