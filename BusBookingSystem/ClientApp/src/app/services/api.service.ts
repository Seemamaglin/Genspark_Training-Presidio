import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient, private auth: AuthService) {}

  private get headers(): HttpHeaders {
    let headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    if (this.auth.token) {
      headers = headers.set('Authorization', `Bearer ${this.auth.token}`);
    }
    return headers;
  }

  public register(data: any) {
    return this.http.post(`${this.baseUrl}/Account/register`, data, { headers: this.headers });
  }

  public login(data: any) {
    return this.http.post(`${this.baseUrl}/Account/login`, data, { headers: this.headers });
  }

  public searchBuses(source: string, destination: string, date: string) {
    return this.http.get(`${this.baseUrl}/Buses/search?source=${encodeURIComponent(source)}&destination=${encodeURIComponent(destination)}&date=${encodeURIComponent(date)}`);
  }

  public getBus(id: string) {
    return this.http.get(`${this.baseUrl}/Buses/${id}`);
  }

  public selectSeats(payload: any) {
    return this.http.post(`${this.baseUrl}/Bookings/select-seats`, payload, { headers: this.headers });
  }

  public confirmBooking(payload: any) {
    return this.http.post(`${this.baseUrl}/Bookings/confirm`, payload, { headers: this.headers });
  }

  public getMyBookings() {
    return this.http.get(`${this.baseUrl}/Bookings/my-bookings`, { headers: this.headers });
  }

  public getBookingDashboard() {
    return this.http.get(`${this.baseUrl}/Bookings/dashboard`, { headers: this.headers });
  }

  public requestOperatorUpgrade(data: { source: string; destination: string }) {
    return this.http.post(`${this.baseUrl}/Account/request-operator-upgrade`, data, { headers: this.headers });
  }

  public getOperatorProfile() {
    return this.http.get(`${this.baseUrl}/BusOperators/me`, { headers: this.headers });
  }

  public getOperatorBuses() {
    return this.http.get(`${this.baseUrl}/BusOperators/buses`, { headers: this.headers });
  }

  public getOperatorBookings() {
    return this.http.get(`${this.baseUrl}/BusOperators/bookings`, { headers: this.headers });
  }

  public getOperatorRevenue() {
    return this.http.get(`${this.baseUrl}/BusOperators/revenue`, { headers: this.headers });
  }

  public getOperatorStops() {
    return this.http.get(`${this.baseUrl}/BusOperators/stops`, { headers: this.headers });
  }

  public addOperatorStop(data: { stopName: string; type: number; sortOrder?: number }) {
    return this.http.post(`${this.baseUrl}/BusOperators/stops`, data, { headers: this.headers });
  }

  public deleteOperatorStop(stopId: number) {
    return this.http.delete(`${this.baseUrl}/BusOperators/stops/${stopId}`, { headers: this.headers });
  }

  public getBusStops(busId: string) {
    return this.http.get(`${this.baseUrl}/Buses/${busId}/stops`);
  }

  public createBus(data: any) {
    return this.http.post(`${this.baseUrl}/Buses`, data, { headers: this.headers });
  }

  public disableBus(id: any) {
    return this.http.put(`${this.baseUrl}/BusOperators/buses/${id}/disable`, {}, { headers: this.headers });
  }

  public enableBus(id: any) {
    return this.http.put(`${this.baseUrl}/BusOperators/buses/${id}/enable`, {}, { headers: this.headers });
  }

  public cancelBooking(bookingId: any) {
    return this.http.post(`${this.baseUrl}/Bookings/cancel/${bookingId}`, {}, { headers: this.headers });
  }

  public getRoutes() {
    return this.http.get(`${this.baseUrl}/Admin/routes`, { headers: this.headers });
  }

  public createRoute(route: any) {
    return this.http.post(`${this.baseUrl}/Admin/routes`, route, { headers: this.headers });
  }

  public getOperatorRequests() {
    return this.http.get(`${this.baseUrl}/Admin/operator-requests`, { headers: this.headers });
  }

  public approveOperator(userId: string) {
    return this.http.post(`${this.baseUrl}/Admin/operators/${userId}/approve`, {}, { headers: this.headers });
  }

  public rejectOperator(userId: string, reason: string) {
    return this.http.post(`${this.baseUrl}/Admin/operators/${userId}/reject`, JSON.stringify(reason), { headers: this.headers });
  }

  public getAllUsers() {
    return this.http.get(`${this.baseUrl}/Admin/users`, { headers: this.headers });
  }

  public getAdminRevenue() {
    return this.http.get(`${this.baseUrl}/Admin/revenue`, { headers: this.headers });
  }

  public getAllBuses() {
    return this.http.get(`${this.baseUrl}/Admin/buses`, { headers: this.headers });
  }

  public cancelBus(busId: any, reason: string) {
    return this.http.post(`${this.baseUrl}/Admin/buses/${busId}/cancel`, { cancellationReason: reason }, { headers: this.headers });
  }

  public disableOperator(operatorId: any) {
    return this.http.post(`${this.baseUrl}/Admin/operators/${operatorId}/disable`, {}, { headers: this.headers });
  }

  public enableOperator(operatorId: any) {
    return this.http.post(`${this.baseUrl}/Admin/operators/${operatorId}/enable`, {}, { headers: this.headers });
  }

  public getOperatorsList() {
    return this.http.get(`${this.baseUrl}/Admin/operators`, { headers: this.headers });
  }

  public adminCreateBus(data: any) {
    return this.http.post(`${this.baseUrl}/Admin/buses`, data, { headers: this.headers });
  }

  public deleteRoute(routeId: any) {
    return this.http.delete(`${this.baseUrl}/Admin/routes/${routeId}`, { headers: this.headers });
  }
}
