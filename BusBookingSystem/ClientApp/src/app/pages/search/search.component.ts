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
  public searched = false;
  public today = new Date().toISOString().slice(0, 10);

  constructor(private api: ApiService, private router: Router) {}

  public search() {
    this.errorMessage = '';
    this.searched = false;
    if (!this.source || !this.destination || !this.date) {
      this.errorMessage = 'Please fill in all search fields.';
      return;
    }
    this.api.searchBuses(this.source, this.destination, this.date).subscribe({
      next: (result: any) => {
        this.buses = result;
        this.searched = true;
      },
      error: err => {
        this.errorMessage = err.error?.title || err.error || err.statusText || 'Search failed.';
        this.searched = true;
      }
    });
  }

  public book(busId: string) {
    this.router.navigate(['/booking', busId]);
  }
}
