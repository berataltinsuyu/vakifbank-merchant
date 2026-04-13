import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router      = inject(Router);

  const token = authService.tokenAl();

  // Token varsa her isteğe Authorization header'ı ekle
  const yetkiliIstek = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(yetkiliIstek).pipe(
    catchError((hata: HttpErrorResponse) => {
      // 401 gelirse token geçersiz — çıkış yap
      if (hata.status === 401) {
        authService.cikis();
        router.navigate(['/giris']);
      }
      return throwError(() => hata);
    })
  );
};