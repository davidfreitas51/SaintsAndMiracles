import { Injectable, NgZone } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class LoadingService {
  private loadingSubject = new BehaviorSubject<boolean>(false);
  loading$ = this.loadingSubject.asObservable();

  private requests = 0;

  constructor(private zone: NgZone) {}

  show(): void {
    this.requests++;
    if (this.requests === 1) {
      this.zone.runOutsideAngular(() => {
        setTimeout(() => this.zone.run(() => this.loadingSubject.next(true)));
      });
    }
  }

  hide(): void {
    this.requests--;
    if (this.requests === 0) {
      this.zone.runOutsideAngular(() => {
        setTimeout(() => this.zone.run(() => this.loadingSubject.next(false)));
      });
    }
  }
}
