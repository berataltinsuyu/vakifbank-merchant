import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router      = inject(Router);

  if (authService.girisYapildiMi()) {
    return true; // Giriş yapılmış — sayfaya geç
  }

  // Giriş yapılmamış — login'e yönlendir
  router.navigate(['/giris']);
  return false;
};