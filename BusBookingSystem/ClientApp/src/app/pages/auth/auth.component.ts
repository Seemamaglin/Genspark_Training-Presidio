import { Component } from '@angular/core';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-auth',
  templateUrl: './auth.component.html'
})
export class AuthComponent {
  public isRegister = false;
  public model: any = {
    name: '',
    email: '',
    password: '',
    phoneNumber: '',
    age: 18,
    proof: ''
  };
  public errorMessage = '';
  public loading = false;

  constructor(private api: ApiService, private auth: AuthService, private router: Router) {}

  public toggleMode() {
    this.isRegister = !this.isRegister;
    this.errorMessage = '';
  }

  private extractError(err: any): string {
    if (!err.error) return err.statusText || 'An error occurred';
    if (typeof err.error === 'string') return err.error;
    if (Array.isArray(err.error)) return err.error.join(' ');
    if (err.error.message) return err.error.message;
    if (err.error.errors) {
      const msgs = Object.values(err.error.errors as Record<string, string[]>).flat();
      return msgs.join(' ');
    }
    return err.statusText || 'An error occurred';
  }

  public submit() {
    this.errorMessage = '';
    this.loading = true;
    if (this.isRegister) {
      this.api.register(this.model).subscribe({
        next: (result: any) => {
          this.auth.setSession(result.token, { name: result.userName, roles: result.roles });
          this.router.navigate(['/dashboard']);
        },
        error: err => { this.loading = false; this.errorMessage = this.extractError(err); }
      });
    } else {
      this.api.login(this.model).subscribe({
        next: (result: any) => {
          this.auth.setSession(result.token, { name: result.userName, roles: result.roles });
          this.router.navigate(['/dashboard']);
        },
        error: err => { this.loading = false; this.errorMessage = this.extractError(err); }
      });
    }
  }
}
