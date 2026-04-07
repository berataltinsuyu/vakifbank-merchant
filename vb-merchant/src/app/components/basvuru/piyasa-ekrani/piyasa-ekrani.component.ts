import { Component, OnInit } from '@angular/core';
import { CommonModule, KeyValuePipe } from '@angular/common';
import { DovizService } from '../../../services/doviz.service';
import { DovizKurlari } from '../../../models/basvuru.model';

@Component({
  selector: 'app-piyasa-ekrani',
  standalone: true,
  imports: [CommonModule, KeyValuePipe],
  templateUrl: './piyasa-ekrani.component.html'
})
export class PiyasaEkraniComponent implements OnInit {
  kurlar: DovizKurlari = {};
  sonGuncelleme = '';
  yukleniyor = true;

  constructor(private dovizService: DovizService) {}

  ngOnInit() {
    this.dovizService.kurlar$.subscribe(data => {
      this.kurlar = data;
      this.sonGuncelleme = new Date().toLocaleString('tr-TR');
      this.yukleniyor = false;
    });
  }
}
