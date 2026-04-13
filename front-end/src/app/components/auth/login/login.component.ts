import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  form: FormGroup;
  sifreGoster = false;
  yukleniyor  = false;
  hata        = '';

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private authService: AuthService
  ) {
    // Zaten giriş yapılmışsa direkt yönlendir
    if (this.authService.girisYapildiMi()) {
      this.router.navigate(['/basvuru']);
    }

    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      sifre: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

  girisYap() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.yukleniyor = true;
    this.hata       = '';

    const { email, sifre } = this.form.value;

    this.authService.giris(email, sifre).subscribe({
      next: () => {
        this.yukleniyor = false;
        this.router.navigate(['/basvuru']);
      },
      error: (err) => {
        this.yukleniyor = false;
        this.hata = err.status === 401
          ? 'Email veya şifre hatalı.'
          : 'Giriş yapılamadı. Tekrar deneyin.';
      }
    });
  }

  hataVar(alan: string): boolean {
    const c = this.form.get(alan);
    return !!(c?.invalid && c?.touched);
  }
}