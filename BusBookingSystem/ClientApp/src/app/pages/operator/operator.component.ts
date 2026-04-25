import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-operator',
  templateUrl: './operator.component.html'
})
export class OperatorComponent implements OnInit {
  public profile: any = null;
  public buses: any[] = [];
  public bookings: any[] = [];
  public revenue = 0;
  public newBus: any = {
    registrationNumber: '',
    timing: '09:00',
    travelDate: new Date().toISOString().slice(0, 10),
    seatLayout: 'A1,A2,A3,B1,B2,B3',
    price: 0
  };
  public error = '';

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.loadOperatorData();
  }

  public loadOperatorData(): void {
    this.error = '';
    this.api.getOperatorProfile().subscribe({
      next: (result: any) => this.profile = result,
      error: err => this.error = err.error || err.statusText
    });
    this.api.getOperatorBuses().subscribe({
      next: (result: any) => this.buses = result,
      error: err => this.error = err.error || err.statusText
    });
    this.api.getOperatorBookings().subscribe({
      next: (result: any) => this.bookings = result,
      error: err => this.error = err.error || err.statusText
    });
    this.api.getOperatorRevenue().subscribe({
      next: (result: any) => this.revenue = result.revenue,
      error: err => this.error = err.error || err.statusText
    });
  }

  public createBus(): void {
    this.error = '';
    this.api.createBus(this.newBus).subscribe({
      next: () => this.loadOperatorData(),
      error: err => this.error = err.error || err.statusText
    });
  }

  public disableBus(bus: any): void {
    this.api.disableBus(bus.id).subscribe({
      next: () => this.loadOperatorData(),
      error: err => this.error = err.error || err.statusText
    });
  }

  public enableBus(bus: any): void {
    this.api.enableBus(bus.id).subscribe({
      next: () => this.loadOperatorData(),
      error: err => this.error = err.error || err.statusText
    });
  }
}
