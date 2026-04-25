import { Component } from '@angular/core';
import { ApiService } from '../../services/api.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html'
})
export class SearchComponent {
  public source = '';
  public destination = '';
  public date = new Date().toISOString().slice(0, 10);
  public buses: any[] = [];
  public errorMessage = '';

  constructor(private api: ApiService, private router: Router) {}

  public search() {
    this.errorMessage = '';
    this.api.searchBuses(this.source, this.destination, this.date).subscribe({
      next: (result: any) => {
        this.buses = result;
      },
      error: err => this.errorMessage = err.error?.title || err.error || err.statusText
    });
  }

  public book(busId: number) {
    this.router.navigate(['/booking', busId]);
  }
}
