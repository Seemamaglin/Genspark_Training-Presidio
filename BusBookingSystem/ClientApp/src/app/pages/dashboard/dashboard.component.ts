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
  public upgradeRequest = { source: '', destination: '' };

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

  public cancelBooking(booking: any): void {
    if (!confirm(`Cancel booking ${booking.bookingReference}? This action cannot be undone.`)) return;
    this.message = '';
    this.api.cancelBooking(booking.id).subscribe({
      next: () => {
        this.message = 'Booking cancelled and refund processed.';
        this.loadDashboard();
      },
      error: err => this.message = err.error?.message || err.error || err.statusText
    });
  }

  public requestOperatorUpgrade(): void {
    this.message = '';
    if (!this.upgradeRequest.source || !this.upgradeRequest.destination) {
      this.message = 'Please enter both source and destination.';
      return;
    }
    this.api.requestOperatorUpgrade(this.upgradeRequest).subscribe({
      next: () => {
        this.message = `Request submitted for ${this.upgradeRequest.source} → ${this.upgradeRequest.destination}. Await admin approval.`;
        this.upgradeRequest = { source: '', destination: '' };
      },
      error: err => this.message = typeof err.error === 'string' ? err.error : err.error?.message || err.statusText
    });
  }
}
