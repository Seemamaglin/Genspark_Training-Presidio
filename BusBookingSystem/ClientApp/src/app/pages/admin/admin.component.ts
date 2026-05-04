import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html'
})
export class AdminComponent implements OnInit {
  public routes: any[] = [];
  public requests: any[] = [];
  public users: any[] = [];
  public allBuses: any[] = [];
  public operators: any[] = [];
  public revenue: any = null;
  public activeTab = 'dashboard';

  public newRoute = { source: '', destination: '' };
  public assignRoute: { [key: string]: any } = {};
  public cancelReason: { [key: string]: string } = {};
  public error = '';
  public success = '';
  public today = new Date().toISOString().slice(0, 10);

  public newBus: any = {
    registrationNumber: '',
    busName: '',
    busType: 'AC Seater',
    departureTime: '09:00',
    arrivalTime: '',
    travelDate: new Date().toISOString().slice(0, 10),
    totalSeats: 40,
    price: 500,
    routeId: '',
    operatorId: null,
    seatLayout: ''
  };

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.loadAll();
  }

  public loadAll(): void {
    this.loadRoutes();
    this.loadRequests();
    this.loadUsers();
    this.loadAllBuses();
    this.loadRevenue();
    this.loadOperators();
  }

  public loadRoutes(): void {
    this.api.getRoutes().subscribe({
      next: (result: any) => this.routes = result,
      error: () => {}
    });
  }

  public loadRequests(): void {
    this.api.getOperatorRequests().subscribe({
      next: (result: any) => this.requests = result,
      error: () => {}
    });
  }

  public loadUsers(): void {
    this.api.getAllUsers().subscribe({
      next: (result: any) => this.users = result,
      error: () => {}
    });
  }

  public loadAllBuses(): void {
    this.api.getAllBuses().subscribe({
      next: (result: any) => this.allBuses = result,
      error: () => {}
    });
  }

  public loadRevenue(): void {
    this.api.getAdminRevenue().subscribe({
      next: (result: any) => this.revenue = result,
      error: () => {}
    });
  }

  public loadOperators(): void {
    this.api.getOperatorsList().subscribe({
      next: (result: any) => this.operators = result,
      error: () => {}
    });
  }

  public createRoute(): void {
    this.error = '';
    this.success = '';
    if (!this.newRoute.source || !this.newRoute.destination) {
      this.error = 'Source and destination are required.';
      return;
    }
    this.api.createRoute(this.newRoute).subscribe({
      next: () => {
        this.newRoute = { source: '', destination: '' };
        this.success = 'Route created successfully.';
        this.loadRoutes();
      },
      error: err => this.error = err.error || err.statusText || 'Failed to create route.'
    });
  }

  public deleteRoute(route: any): void {
    if (!confirm(`Delete route ${route.source} → ${route.destination}?`)) return;
    this.api.deleteRoute(route.id).subscribe({
      next: () => { this.success = 'Route deleted.'; this.loadRoutes(); },
      error: err => this.error = err.error || err.statusText
    });
  }

  public approveRequest(request: any): void {
    this.error = '';
    this.success = '';
    this.api.approveOperator(request.id).subscribe({
      next: () => {
        this.success = `${request.name} approved as bus operator (${request.source} → ${request.destination}).`;
        this.loadAll();
      },
      error: err => this.error = typeof err.error === 'string' ? err.error : err.error?.message || err.statusText
    });
  }

  public rejectRequest(request: any): void {
    this.error = '';
    const reason = prompt('Reason for rejection (optional):') || 'Not approved at this time';
    this.api.rejectOperator(request.id, reason).subscribe({
      next: () => { this.success = `${request.name}'s request rejected.`; this.loadRequests(); },
      error: err => this.error = err.error || err.statusText
    });
  }

  public adminCreateBus(): void {
    this.error = '';
    this.success = '';
    if (!this.newBus.registrationNumber || !this.newBus.routeId || !this.newBus.travelDate || !this.newBus.price) {
      this.error = 'Registration number, route, travel date and price are required.';
      return;
    }
    const payload = { ...this.newBus, operatorId: this.newBus.operatorId || null };
    this.api.adminCreateBus(payload).subscribe({
      next: () => {
        this.success = 'Bus created successfully!';
        this.newBus = {
          registrationNumber: '', busName: '', busType: 'AC Seater',
          departureTime: '09:00', arrivalTime: '', travelDate: this.today,
          totalSeats: 40, price: 500, routeId: '', operatorId: null, seatLayout: ''
        };
        this.loadAllBuses();
      },
      error: err => this.error = err.error?.message || (typeof err.error === 'string' ? err.error : '') || err.statusText || 'Failed to create bus.'
    });
  }

  public cancelBus(bus: any): void {
    this.error = '';
    const reason = this.cancelReason[bus.id] || 'Cancelled by admin';
    this.api.cancelBus(bus.id, reason).subscribe({
      next: () => {
        this.success = `Bus ${bus.registrationNumber} cancelled. All bookings refunded.`;
        this.cancelReason[bus.id] = '';
        this.loadAllBuses();
      },
      error: err => this.error = err.error || err.statusText
    });
  }

  public disableOperator(op: any): void {
    this.error = '';
    this.api.disableOperator(op.id).subscribe({
      next: () => { this.success = `Operator ${op.name} disabled.`; this.loadOperators(); },
      error: err => this.error = err.error || err.statusText
    });
  }

  public enableOperator(op: any): void {
    this.error = '';
    this.api.enableOperator(op.id).subscribe({
      next: () => { this.success = `Operator ${op.name} enabled.`; this.loadOperators(); },
      error: err => this.error = err.error || err.statusText
    });
  }
}
