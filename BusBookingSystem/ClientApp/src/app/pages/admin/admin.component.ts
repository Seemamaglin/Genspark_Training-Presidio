import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../services/api.service';

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html'
})
export class AdminComponent implements OnInit {
  public routes: any[] = [];
  public requests: any[] = [];
  public newRoute = { source: '', destination: '' };
  public assignRoute: { [key: string]: number } = {};
  public error = '';

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.loadRoutes();
    this.loadRequests();
  }

  public loadRoutes(): void {
    this.api.getRoutes().subscribe({
      next: (result: any) => this.routes = result,
      error: err => this.error = err.error || err.statusText
    });
  }

  public loadRequests(): void {
    this.api.getOperatorRequests().subscribe({
      next: (result: any) => this.requests = result,
      error: err => this.error = err.error || err.statusText
    });
  }

  public createRoute(): void {
    this.error = '';
    if (!this.newRoute.source || !this.newRoute.destination) {
      this.error = 'Source and destination are required.';
      return;
    }

    this.api.createRoute(this.newRoute).subscribe({
      next: () => {
        this.newRoute = { source: '', destination: '' };
        this.loadRoutes();
      },
      error: err => this.error = err.error || err.statusText
    });
  }

  public approveRequest(request: any): void {
    this.error = '';
    const routeId = this.assignRoute[request.id];
    if (!routeId) {
      this.error = 'Choose a route to assign.';
      return;
    }

    this.api.approveOperator(request.id, routeId).subscribe({
      next: () => this.loadRequests(),
      error: err => this.error = err.error || err.statusText
    });
  }

  public rejectRequest(request: any): void {
    this.error = '';
    this.api.rejectOperator(request.id, 'Not approved at this time').subscribe({
      next: () => this.loadRequests(),
      error: err => this.error = err.error || err.statusText
    });
  }
}
