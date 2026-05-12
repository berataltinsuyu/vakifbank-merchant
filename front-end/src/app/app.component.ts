import { Component } from '@angular/core';
import { RouterOutlet, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { SidebarComponent } from './components/layout/sidebar/sidebar.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule, SidebarComponent],
  template: `
    <ng-container *ngIf="!isLoginPage()">
      <app-sidebar />
    </ng-container>
    <router-outlet />
  `
})
export class AppComponent {
  constructor(private router: Router) {}
  isLoginPage(): boolean {
    return this.router.url === '/giris' || this.router.url === '/';
  }
}
