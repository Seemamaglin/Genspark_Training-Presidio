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
  public stops: any = { boarding: [], dropping: [] };
  public error = '';
  public success = '';
  public activeTab = 'buses';

  public newBoardingStop = '';
  public newDroppingStop = '';

  public newBus: any = {
    registrationNumber: '',
    busName: '',
    busType: 'AC Seater',
    departureTime: '09:00',
    arrivalTime: '',
    travelDate: new Date().toISOString().slice(0, 10),
    totalSeats: 40,
    price: 500,
    boardingPoint: '',
    droppingPoint: '',
    seatLayout: ''
  };

  public today = new Date().toISOString().slice(0, 10);

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.loadOperatorData();
  }

  public loadOperatorData(): void {
    this.error = '';
    this.api.getOperatorProfile().subscribe({
      next: (result: any) => this.profile = result,
      error: err => this.error = 'Failed to load profile. ' + (err.error || err.statusText)
    });
    this.api.getOperatorBuses().subscribe({
      next: (result: any) => this.buses = result,
      error: () => {}
    });
    this.api.getOperatorBookings().subscribe({
      next: (result: any) => this.bookings = result,
      error: () => {}
    });
    this.api.getOperatorRevenue().subscribe({
      next: (result: any) => this.revenue = result.revenue ?? result.total ?? 0,
      error: () => {}
    });
    this.loadStops();
  }

  public loadStops(): void {
    this.api.getOperatorStops().subscribe({
      next: (result: any) => this.stops = { boarding: result.boarding || [], dropping: result.dropping || [] },
      error: () => {}
    });
  }

  public addStop(type: 'boarding' | 'dropping'): void {
    const name = type === 'boarding' ? this.newBoardingStop.trim() : this.newDroppingStop.trim();
    if (!name) return;
    this.error = '';
    this.api.addOperatorStop({ stopName: name, type: type === 'boarding' ? 0 : 1 }).subscribe({
      next: () => {
        if (type === 'boarding') this.newBoardingStop = '';
        else this.newDroppingStop = '';
        this.success = 'Stop added.';
        this.loadStops();
      },
      error: err => this.error = err.error || err.statusText
    });
  }

  public deleteStop(stopId: number): void {
    this.api.deleteOperatorStop(stopId).subscribe({
      next: () => { this.success = 'Stop removed.'; this.loadStops(); },
      error: err => this.error = err.error || err.statusText
    });
  }

  public createBus(): void {
    this.error = '';
    this.success = '';
    if (!this.newBus.registrationNumber || !this.newBus.departureTime || !this.newBus.travelDate || !this.newBus.price) {
      this.error = 'Registration number, departure time, travel date and price are required.';
      return;
    }
    if (!this.newBus.boardingPoint || !this.newBus.droppingPoint) {
      this.error = 'Boarding stop and dropping stop are required.';
      return;
    }
    this.api.createBus(this.newBus).subscribe({
      next: () => {
        this.success = 'Bus added successfully!';
        this.newBus = {
          registrationNumber: '', busName: '', busType: 'AC Seater',
          departureTime: '09:00', arrivalTime: '', travelDate: this.today,
          totalSeats: 40, price: 500, boardingPoint: '', droppingPoint: '', seatLayout: ''
        };
        this.loadOperatorData();
      },
      error: err => {
        this.error = err.error?.message || (typeof err.error === 'string' ? err.error : '') || err.statusText || 'Failed to create bus.';
      }
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
