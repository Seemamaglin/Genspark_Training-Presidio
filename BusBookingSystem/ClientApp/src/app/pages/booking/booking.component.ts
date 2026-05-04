import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-booking',
  templateUrl: './booking.component.html'
})
export class BookingComponent implements OnInit, OnDestroy {
  public bus: any = null;
  public busStops: any = { boarding: [], dropping: [] };
  public selectedSeats: string[] = [];
  public reservationToken = '';
  public lockExpiry = '';
  public lockExpiryDate: Date | null = null;
  public message = '';
  public isSuccess = false;
  public isLoading = true;
  public passengers: any[] = [];
  public paymentMethod = 'card';
  public bookingConfirmed = false;
  public confirmedRef = '';

  // step: 1=seats, 2=stops, 3=passengers+payment
  public step = 1;
  public selectedPickup = '';
  public selectedDropping = '';

  private timerInterval: any;
  public secondsLeft = 0;

  constructor(
    private route: ActivatedRoute,
    private api: ApiService,
    public auth: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id') || '';
    if (!id) {
      this.router.navigate(['/search']);
      return;
    }

    this.api.getBus(id).subscribe({
      next: (result: any) => {
        this.bus = result;
        this.isLoading = false;
        this.api.getBusStops(id).subscribe({
          next: (stops: any) => this.busStops = stops,
          error: () => {}
        });
      },
      error: () => {
        this.isLoading = false;
        this.router.navigate(['/search']);
      }
    });
  }

  ngOnDestroy(): void {
    if (this.timerInterval) clearInterval(this.timerInterval);
  }

  public getSeatRows(): any[][] {
    if (!this.bus?.allSeats) return [];
    const seats = this.bus.allSeats as any[];
    const rowMap: { [key: string]: any[] } = {};
    for (const seat of seats) {
      const match = seat.seatCode.match(/^(\d+)([A-D])$/);
      if (match) {
        const row = match[1];
        if (!rowMap[row]) rowMap[row] = [null, null, null, null];
        const colIndex = 'ABCD'.indexOf(match[2]);
        rowMap[row][colIndex] = seat;
      }
    }
    return Object.keys(rowMap).sort((a, b) => +a - +b).map(r => rowMap[r]);
  }

  public getSeatState(seat: any): string {
    if (!seat) return 'empty';
    if (this.selectedSeats.includes(seat.seatCode)) return 'selected';
    if (!seat.isAvailable) return 'unavailable';
    if (seat.isLocked) return 'locked';
    return 'available';
  }

  public toggleSeat(seat: any) {
    if (!seat || !seat.isAvailable || seat.isLocked) return;
    const code = seat.seatCode;
    if (this.selectedSeats.includes(code)) {
      this.selectedSeats = this.selectedSeats.filter(s => s !== code);
    } else {
      this.selectedSeats.push(code);
    }
    this.syncPassengers();
  }

  private syncPassengers() {
    while (this.passengers.length < this.selectedSeats.length) {
      this.passengers.push({ name: '', email: '', phoneNumber: '', age: 18, seatNumber: '' });
    }
    while (this.passengers.length > this.selectedSeats.length) {
      this.passengers.pop();
    }
    this.passengers = this.passengers.map((p, i) => ({ ...p, seatNumber: this.selectedSeats[i] || '' }));
  }

  public lockSeats() {
    this.message = '';
    if (this.selectedSeats.length === 0) {
      this.message = 'Please select at least one seat.';
      return;
    }
    const payload = {
      busId: this.bus.id,
      seatCodes: this.selectedSeats,
      reservationToken: this.reservationToken || undefined
    };
    this.api.selectSeats(payload).subscribe({
      next: (result: any) => {
        this.reservationToken = result.reservationToken;
        this.lockExpiryDate = new Date(result.lockedUntil);
        this.lockExpiry = this.lockExpiryDate.toLocaleTimeString();
        this.startTimer();
        this.message = '';
        this.step = 2;
      },
      error: err => {
        this.message = err.error?.message || err.error || err.statusText || 'Failed to reserve seats.';
      }
    });
  }

  private startTimer() {
    if (this.timerInterval) clearInterval(this.timerInterval);
    this.timerInterval = setInterval(() => {
      if (!this.lockExpiryDate) return;
      this.secondsLeft = Math.max(0, Math.floor((this.lockExpiryDate.getTime() - Date.now()) / 1000));
      if (this.secondsLeft === 0) {
        clearInterval(this.timerInterval);
        this.reservationToken = '';
        this.message = 'Seat reservation expired. Please select seats again.';
      }
    }, 1000);
  }

  public get timerDisplay(): string {
    const m = Math.floor(this.secondsLeft / 60).toString().padStart(2, '0');
    const s = (this.secondsLeft % 60).toString().padStart(2, '0');
    return `${m}:${s}`;
  }

  public confirmStops(): void {
    const hasStops = this.busStops.boarding.length > 0 || this.busStops.dropping.length > 0;
    if (hasStops) {
      if (this.busStops.boarding.length > 0 && !this.selectedPickup) {
        this.message = 'Please select a boarding stop.';
        return;
      }
      if (this.busStops.dropping.length > 0 && !this.selectedDropping) {
        this.message = 'Please select a dropping stop.';
        return;
      }
    }
    this.message = '';
    this.step = 3;
  }

  public isPassengersValid(): boolean {
    return this.passengers.every(p => p.name && p.email && p.phoneNumber && p.age > 0);
  }

  public confirmBooking() {
    if (!this.auth.isAuthenticated()) {
      this.router.navigate(['/auth'], { queryParams: { returnUrl: this.router.url } });
      return;
    }

    if (!this.reservationToken) {
      this.message = 'Please reserve seats first.';
      return;
    }

    if (!this.isPassengersValid()) {
      this.message = 'Please fill in all passenger details.';
      return;
    }

    const payload = {
      busId: this.bus.id,
      reservationToken: this.reservationToken,
      paymentMethod: this.paymentMethod,
      passengerDetails: this.passengers,
      pickupStop: this.selectedPickup || null,
      droppingStop: this.selectedDropping || null
    };

    this.api.confirmBooking(payload).subscribe({
      next: (result: any) => {
        this.bookingConfirmed = true;
        this.confirmedRef = result.reference;
        this.isSuccess = true;
        this.message = '';
        if (this.timerInterval) clearInterval(this.timerInterval);
      },
      error: err => {
        const errMsg = err.error?.message || err.error || err.statusText || 'Booking failed.';
        this.message = errMsg;
        this.isSuccess = false;
      }
    });
  }

  public goToDashboard() {
    this.router.navigate(['/dashboard']);
  }
}
