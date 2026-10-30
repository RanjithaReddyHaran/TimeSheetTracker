import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

export interface UserSession {
  name: string;
  roles: string[];
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  session: UserSession | null = null;

  constructor(private readonly http: HttpClient) {}

  login(email: string, password: string): Observable<{ email: string; fullName: string }> {
    return this.http.post<{ email: string; fullName: string }>('/api/auth/login',
      { email, password, rememberMe: false }).pipe(
        tap(user => this.session = { name: user.fullName, roles: [] })
      );
  }

  loadSession(): Observable<UserSession> {
    return this.http.get<UserSession>('/api/auth/me').pipe(
      tap(user => this.session = user)
    );
  }

  logout(): Observable<void> {
    return this.http.post<void>('/api/auth/logout', {}).pipe(
      tap(() => this.session = null)
    );
  }
}

