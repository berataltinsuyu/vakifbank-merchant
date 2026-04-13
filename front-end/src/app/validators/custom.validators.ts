import { AbstractControl, ValidationErrors, ValidatorFn, AsyncValidatorFn } from '@angular/forms';
import { of, timer } from 'rxjs';
import { map, catchError, switchMap, first } from 'rxjs/operators';

interface VergiNoKontrolServisi {
  vergiNoKontrol(vergiNo: string): import('rxjs').Observable<boolean>;
}

export function vergiNoValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const val = control.value?.toString().trim();
    if (!val) return null;
    if (!/^\d{10}$/.test(val)) return { vergiNoFormat: 'Vergi numarası tam 10 haneli rakamdan oluşmalıdır.' };
    return null;
  };
}

export function tcknValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const val = control.value?.toString().trim();
    if (!val) return null;
    if (!/^\d{11}$/.test(val)) return { tcknFormat: 'TC Kimlik No tam 11 haneli rakamdan oluşmalıdır.' };
    return null;
  };
}

export function telefonValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const val = control.value?.toString().trim().replace(/\s/g, '');
    if (!val) return null;
    if (!/^(05|5)\d{9}$/.test(val)) return { telefonFormat: 'Geçerli telefon girin. Örn: 5321234567' };
    return null;
  };
}

export function IsTelefonValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const val = control.value?.toString().trim().replace(/\s/g, '');
    if (!val) return null;
    if (!/^(02|2)\d{9}$/.test(val)) return { telefonFormat: 'Geçerli iş telefonu girin. Örn: 2321234567' };
    return null;
  };
}

export function emailValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const val = control.value?.trim();
    if (!val) return null;
    if (!/^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$/.test(val)) return { emailFormat: 'Geçerli e-posta girin.' };
    return null;
  };
}

export function vergiNoUniquenessValidator(service: VergiNoKontrolServisi): AsyncValidatorFn {
  return (control: AbstractControl) => {
    const val = control.value?.toString().trim();
    if (!val || !/^\d{10}$/.test(val)) return of(null);
    return timer(600).pipe(
      switchMap(() => service.vergiNoKontrol(val)),
      map((mevcut: boolean) => mevcut ? { vergiNoMevcut: 'Bu vergi numarasıyla daha önce başvuru yapılmış.' } : null),
      catchError(() => of(null)),
      first()
    );
  };
}
