import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, MatToolbarModule, MatButtonModule],
  template: `
    <mat-toolbar color="primary">
      <span class="toolbar-title">数据中心供电切换申请系统</span>
      <span class="spacer"></span>
      <button mat-button routerLink="/requests" routerLinkActive="active-link">
        申请列表
      </button>
    </mat-toolbar>
    <div class="app-container">
      <router-outlet></router-outlet>
    </div>
  `,
  styles: [`
    .toolbar-title {
      font-size: 18px;
      font-weight: 500;
    }
    .spacer { flex: 1 1 auto; }
    .active-link {
      background: rgba(255,255,255,0.15);
    }
  `]
})
export class AppComponent {
  title = 'Power Switch System';
}
