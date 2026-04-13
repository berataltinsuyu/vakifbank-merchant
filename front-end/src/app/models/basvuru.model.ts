export interface BasvuruCreateRequest {
  sirketTipiId: number;
  firmaAdi: string | null;
  adSoyad: string | null;
  vergiNoTckn: string;
  vergiDairesi: string | null;
  yetkiliTckn: string | null;
  yetkiliAdSoyad: string | null;
  cepTelefon: string;
  evTelefon: string | null;
  isTelefon: string | null;
  email: string;
  adres: string;
  ilId: number;
  ilceId: number;
  postaKodu: string | null;
  webAdres: string | null;
  isKategorisi: string | null;
  tahminiAylikCiro: number | null;
  enlem: number | null;
  boylam: number | null;
}
export interface Il { id: number; ilAdi: string; }
export interface Ilce { id: number; ilceAdi: string; }
export interface SirketTipi { id: number; tipAdi: string; tipKodu: string; }
export interface DokumanItem { tip: string; ad: string; aciklama: string; ikon: string; zorunlu: boolean; dosya: File | null; onizleme: string | null; }
export interface DovizKurlari { [key: string]: number; }
