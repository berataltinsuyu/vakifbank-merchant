import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private api = `${environment.apiUrl}/auth`;

  constructor(private http: HttpClient, private router: Router) {}

  giris(email: string, sifre: string): Observable<any> {
    return this.http.post(`${this.api}/giris`, { email, sifre }).pipe(
      tap((res: any) => {
        // Token ve kullanıcı bilgisini localStorage'a kaydet
        localStorage.setItem('token', res.token);
        localStorage.setItem('kullanici', JSON.stringify({
          adSoyad: res.adSoyad,
          email:   res.email,
          rol:     res.rol,
          expiry:  res.expiry
        }));
      })
    );
  }

  cikis() {
    localStorage.removeItem('token');
    localStorage.removeItem('kullanici');
    this.router.navigate(['/giris']);
  }

  tokenAl(): string | null {
    return localStorage.getItem('token');
  }

  girisYapildiMi(): boolean {
    const token = this.tokenAl();
    if (!token) return false;

    // Token süresi dolmuş mu kontrol et
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const simdi = Math.floor(Date.now() / 1000);
      return payload.exp > simdi;
    } catch {
      return false;
    }
  }

  kullaniciBilgisi(): any {
    const k = localStorage.getItem('kullanici');
    return k ? JSON.parse(k) : null;
  }
}