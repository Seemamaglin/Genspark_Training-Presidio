import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  public upcoming: any[] = [];
  public past: any[] = [];
  public cancelled: any[] = [];
  public message = '';

  constructor(private api: ApiService, public auth: AuthService) {}

  ngOnInit(): void {
    this.loadDashboard();
  }

  public loadDashboard(): void {
    this.api.getBookingDashboard().subscribe({
      next: (result: any) => {
        this.upcoming = result.upcoming || [];
        this.past = result.past || [];
        this.cancelled = result.cancelled || [];
      },
      error: () => {}
    });
  }

  public requestOperatorUpgrade(): void {
    this.message = '';
    this.api.requestOperatorUpgrade().subscribe({
      next: () => this.message = 'Upgrade request submitted. Wait for admin approval.',
      error: err => this.message = err.error || err.statusText
    });
  }
}
