import { Routes } from '@angular/router';
import { BasvuruFormComponent } from './components/basvuru/basvuru-form/basvuru-form.component';
import { BasvuruListesiComponent } from './components/basvuru/basvuru-listesi/basvuru-listesi.component';
import { LoginComponent } from './components/auth/login/login.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '',        redirectTo: 'giris', pathMatch: 'full' },
  { path: 'giris',   component: LoginComponent },
  // canActivate: [authGuard] → giriş yapılmamışsa /giris'e yönlendir
  { path: 'basvuru', component: BasvuruFormComponent,  canActivate: [authGuard] },
  { path: 'gecmis',  component: BasvuruListesiComponent, canActivate: [authGuard] },
];