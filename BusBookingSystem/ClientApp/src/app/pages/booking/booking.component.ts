import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../services/api.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-booking',
  templateUrl: './booking.component.html'
})
export class BookingComponent implements OnInit {
  public bus: any = null;
  public selectedSeats: string[] = [];
  public reservationToken = '';
  public message = '';
  public passengers: any[] = [{ name: '', email: '', phoneNumber: '', age: 18, seatNumber: '' }];
  public paymentMethod = 'card';

  constructor(
    private route: ActivatedRoute,
    private api: ApiService,
    private auth: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) {
      this.router.navigate(['/search']);
      return;
    }

    this.api.getBus(id).subscribe({
      next: (result: any) => {
        this.bus = result;
      },
      error: () => this.router.navigate(['/search'])
    });
  }

  public toggleSeat(seatCode: string) {
    if (!this.bus) return;
    if (this.selectedSeats.includes(seatCode)) {
      this.selectedSeats = this.selectedSeats.filter(s => s !== seatCode);
    } else {
      this.selectedSeats.push(seatCode);
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
    this.passengers = this.passengers.map((p, index) => ({ ...p, seatNumber: this.selectedSeats[index] || '' }));
  }

  public lockSeats() {
    this.message = '';
    const payload = {
      busId: this.bus.id,
      seatCodes: this.selectedSeats,
      reservationToken: this.reservationToken || undefined
    };
    this.api.selectSeats(payload).subscribe({
      next: (result: any) => {
        this.reservationToken = result.reservationToken;
        this.message = `Seats locked until ${new Date(result.lockedUntil).toLocaleTimeString()}`;
      },
      error: err => this.message = err.error || err.statusText
    });
  }

  public confirmBooking() {
    if (!this.auth.isAuthenticated()) {
      this.message = 'Please log in or sign up before confirming booking.';
      this.router.navigate(['/auth']);
      return;
    }

    const payload = {
      busId: this.bus.id,
      reservationToken: this.reservationToken,
      paymentMethod: this.paymentMethod,
      passengerDetails: this.passengers
    };

    this.api.confirmBooking(payload).subscribe({
      next: (result: any) => {
        this.message = `Booking confirmed. Reference: ${result.reference}`;
      },
      error: err => this.message = err.error || err.statusText
    });
  }
}
