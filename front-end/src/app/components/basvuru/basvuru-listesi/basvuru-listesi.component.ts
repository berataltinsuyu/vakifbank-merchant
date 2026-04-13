import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe, NgIf } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { BasvuruService } from '../../../services/basvuru.service';
import { CountByDurumPipe } from '../../../pipes/count-by-durum.pipe';
import { MinPipe } from '../../../pipes/min.pipe';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-basvuru-listesi',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule, DatePipe, CountByDurumPipe, MinPipe],
  templateUrl: './basvuru-listesi.component.html'
})
export class BasvuruListesiComponent implements OnInit {
  readonly apiBaseUrl = environment.apiUrl.replace(/\/api$/, '');
  basvurular: any[] = [];
  filtrelenmis: any[] = [];
  yukleniyor = true;
  detayYukleniyor = false;
  hata = '';

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
}
