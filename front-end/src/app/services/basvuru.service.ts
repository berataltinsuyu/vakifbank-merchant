import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { BasvuruCreateRequest, Il, Ilce, SirketTipi } from '../models/basvuru.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class BasvuruService {
  private api = environment.apiUrl;
  constructor(private http: HttpClient) {}

  getIller(): Observable<Il[]> {
    return this.http.get<Il[]>(`${this.api}/lookup/iller`).pipe(catchError(this.hataYonet));
  }
  getIlceler(ilId: number): Observable<Ilce[]> {
    return this.http.get<Ilce[]>(`${this.api}/lookup/ilceler/${ilId}`).pipe(catchError(this.hataYonet));
  }
  getSirketTipleri(): Observable<SirketTipi[]> {
    return this.http.get<SirketTipi[]>(`${this.api}/lookup/sirkettipleri`).pipe(catchError(this.hataYonet));
  }
  vergiNoKontrol(vergiNo: string): Observable<boolean> {
    return this.http.get<{ mevcut: boolean }>(`${this.api}/basvuru/vergino-kontrol?vergiNo=${vergiNo}`)
      .pipe(map(res => res.mevcut), catchError(() => [false]));
  }
  basvuruOlustur(data: BasvuruCreateRequest): Observable<any> {
    return this.http.post(`${this.api}/basvuru`, data).pipe(catchError(this.hataYonet));
  }
  getAll(): Observable<any[]> {
    return this.http.get<any[]>(`${this.api}/basvuru`).pipe(catchError(this.hataYonet));
  }
  dokumanYukle(basvuruId: number, tip: string, dosya: File): Observable<any> {
    const form = new FormData();
    form.append('dosyalar', dosya, dosya.name);
    return this.http.post(`${this.api}/dokuman/yukle?basvuruId=${basvuruId}&dokumanTipi=${tip}`, form)
      .pipe(catchError(this.hataYonet));
  }
  private hataYonet(hata: HttpErrorResponse) {
    let mesaj = 'Bir hata oluştu.';
    if (hata.error?.errors) mesaj = Object.values(hata.error.errors).flat().join(' ');
    else if (hata.error?.message) mesaj = hata.error.message;
    else if (hata.status === 0) mesaj = 'Sunucuya ulaşılamıyor.';
    return throwError(() => new Error(mesaj));
  }
  getListe(): Observable<any[]> {
  return this.http.get<any[]>(`${this.api}/basvuru/liste`)
    .pipe(catchError(this.hataYonet));
  }

  getDetay(id: number): Observable<any> {
    return this.http.get<any>(`${this.api}/basvuru/${id}/detay`)
      .pipe(catchError(this.hataYonet));
  }
}
