import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private tokenKey = 'bus-booking-token';
  private userKey = 'bus-booking-user';

  public get token(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  public get user(): any {
    const value = localStorage.getItem(this.userKey);
    return value ? JSON.parse(value) : null;
  }

  public isAuthenticated(): boolean {
    return !!this.token;
  }

  public setSession(token: string, user: any): void {
    localStorage.setItem(this.tokenKey, token);
    localStorage.setItem(this.userKey, JSON.stringify(user));
  }

  public clear(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
  }

  public hasRole(role: string): boolean {
    return this.user?.roles?.includes(role);
  }
}
