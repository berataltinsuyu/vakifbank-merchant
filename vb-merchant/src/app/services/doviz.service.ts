import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, interval, switchMap, startWith, shareReplay, catchError, of } from 'rxjs';
import { DovizKurlari } from '../models/basvuru.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class DovizService {
  private api = `${environment.apiUrl}/doviz`;
  readonly kurlar$: Observable<DovizKurlari> = interval(30_000).pipe(
    startWith(0),
    switchMap(() => this.http.get<DovizKurlari>(this.api).pipe(catchError(() => of({} as DovizKurlari)))),
    shareReplay(1)
  );
  constructor(private http: HttpClient) {}
}
