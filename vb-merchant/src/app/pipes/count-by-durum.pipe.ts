import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'countByDurum', standalone: true })
export class CountByDurumPipe implements PipeTransform {
  transform(basvurular: any[], durum: string): number {
    return basvurular.filter(b => b.durum === durum).length;
  }
}
