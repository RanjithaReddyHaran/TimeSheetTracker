import { Component } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-login',
  template: `
    <div class="login-wrap">
      <div class="login-card">
        <div class="login-brand"><span class="brand-mark">T</span><b>tempo<span class="brand-light">works</span></b></div>
        <h1>Welcome back</h1><p>Sign in to manage your time and projects.</p>
        <div *ngIf="error" class="inline-error">{{ error }}</div>
        <form [formGroup]="form" (ngSubmit)="submit()">
          <div class="form-field"><label>Email address</label><input type="email" formControlName="email" autocomplete="username"></div>
          <div class="form-field"><label>Password</label><input type="password" formControlName="password" autocomplete="current-password"></div>
          <button class="primary-button login-button" type="submit" [disabled]="form.invalid || loading">{{ loading ? 'Signing in…' : 'Sign in' }}</button>
        </form>
        <small class="login-note">Use one of the development accounts listed in README.md.</small>
      </div>
    </div>
  `
})
export class LoginComponent {
  loading = false;
  error = '';
  form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required]
  });

  constructor(private readonly fb: FormBuilder, private readonly auth: AuthService, private readonly router: Router) {}

  submit(): void {
    if (this.form.invalid || this.loading) return;
    this.loading = true;
    this.auth.login(this.form.value.email || '', this.form.value.password || '').subscribe({
      next: () => this.router.navigate(['/']),
      error: () => { this.error = 'Email or password is incorrect.'; this.loading = false; }
    });
  }
}

