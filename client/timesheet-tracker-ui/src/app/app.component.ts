import { Component } from '@angular/core';
import { AuthService } from './core/auth.service';

@Component({
  selector: 'app-root',
  template: `
    <div class="app-shell">
      <aside class="sidebar">
        <a class="brand" routerLink="/">
          <span class="brand-mark">T</span>
          <span>tempo<span class="brand-light">works</span><small>WORKFORCE PLATFORM</small></span>
        </a>
        <div class="nav-label">WORKSPACE</div>
        <a class="nav-item active" routerLink="/"><span>▦</span> Overview</a>
        <a class="nav-item" routerLink="/"><span>◷</span> My timesheets</a>
        <a class="nav-item" routerLink="/"><span>✓</span> Approvals <span class="nav-count">4</span></a>
        <div class="nav-label section-gap">MANAGE</div>
        <a class="nav-item" routerLink="/"><span>▤</span> Projects</a>
        <a class="nav-item" routerLink="/"><span>♙</span> Team</a>
        <a class="nav-item" routerLink="/"><span>▥</span> Reports</a>
        <div class="sidebar-bottom">
          <div class="help-card"><div class="help-icon">?</div><b>Need a hand?</b><p>Visit the help center for tips and guides.</p><a href="mailto:support@example.com">Get support ↗</a></div>
          <div class="profile"><div class="avatar">EE</div><div><b>Employee Example</b><small>Product team</small></div><span class="more">•••</span></div>
        </div>
      </aside>
      <main class="main-area">
        <header class="topbar">
          <div class="breadcrumbs">Workspace <span>/</span> <b>Overview</b></div>
          <div class="top-actions"><span class="today">Thursday, October 30</span><button class="icon-button" aria-label="Notifications">♧<i></i></button><div class="avatar small-avatar">EE</div></div>
        </header>
        <section class="page-content"><router-outlet></router-outlet></section>
      </main>
    </div>
  `
})
export class AppComponent {
  constructor(public readonly auth: AuthService) {}
}

