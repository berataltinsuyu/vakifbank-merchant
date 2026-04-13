import { Component } from '@angular/core';
import { RouterOutlet, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from './components/layout/navbar/navbar.component';
import { SidebarComponent } from './components/layout/sidebar/sidebar.component';
import { FooterComponent } from './components/layout/footer/footer.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule, NavbarComponent, SidebarComponent, FooterComponent],
  template: `
    <ng-container *ngIf="!isLoginPage()">
      <app-navbar />
      <app-sidebar />
    </ng-container>
    <router-outlet />
    <ng-container *ngIf="!isLoginPage()">
      <app-footer />
    </ng-container>
  `
})
export class AppComponent {
  constructor(private router: Router) {}
  isLoginPage(): boolean {
    return this.router.url === '/giris' || this.router.url === '/';
  }
}
