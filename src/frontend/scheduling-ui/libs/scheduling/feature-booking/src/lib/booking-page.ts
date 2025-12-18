import { CommonModule, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import {
  SchedulingApi,
  ProviderDto,
  SlotDto,
} from '@scheduling-ui/data-access';

@Component({
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DatePipe,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatDividerModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  selector: 'lib-scheduling-booking-page',
  templateUrl: './booking-page.html',
  styleUrl: './booking-page.scss',
})
export class BookingPage {
  private readonly api = inject(SchedulingApi);
  private readonly fb = inject(FormBuilder);
  private readonly snack = inject(MatSnackBar);

  readonly providers = signal<ProviderDto[]>([]);
  readonly slots = signal<SlotDto[]>([]);
  readonly selectedSlot = signal<SlotDto | null>(null);

  readonly loadingSlots = signal(false);
  readonly booking = signal(false);

  readonly searchForm = this.fb.group({
    providerId: ['', Validators.required],
    fromUtc: [new Date().toISOString(), Validators.required],
    toUtc: [
      new Date(Date.now() + 7 * 24 * 3600 * 1000).toISOString(),
      Validators.required,
    ],
  });

  readonly bookForm = this.fb.group({
    customerName: ['Gustavo Felix', Validators.required],
    customerEmail: [
      'gustavo@example.com',
      [Validators.required, Validators.email],
    ],
    customerPhone: ['+258858946440', Validators.required],
    reason: ['Consultation'],
  });

  constructor() {
    this.api.listProviders().subscribe({
      next: (x) => this.providers.set(x),
      error: () => this.providers.set([]),
    });
  }

  generateDefaultSlots() {
    const providerId = this.searchForm.value.providerId!;
    this.api
      .generateSlots(providerId, {
        days: 14,
        slotMinutes: 30,
        startHourLocal: 9,
        endHourLocal: 17,
      })
      .subscribe({
        next: (r) => {
          this.snack.open(`Slots generated: ${r.created}`, 'OK', {
            duration: 2000,
          });
          this.loadSlots();
        },
        error: () =>
          this.snack.open('Failed to generate slots', 'OK', { duration: 2500 }),
      });
  }

  loadSlots() {
    if (this.searchForm.invalid) return;
    this.loadingSlots.set(true);
    this.selectedSlot.set(null);

    const providerId = this.searchForm.value.providerId!;
    const fromUtc = this.searchForm.value.fromUtc!;
    const toUtc = this.searchForm.value.toUtc!;

    this.api.listAvailableSlots(providerId, fromUtc, toUtc).subscribe({
      next: (x) => this.slots.set(x),
      error: () => this.slots.set([]),
      complete: () => this.loadingSlots.set(false),
    });
  }

  selectSlot(s: SlotDto) {
    this.selectedSlot.set(s);
  }

  book() {
    if (
      this.searchForm.invalid ||
      this.bookForm.invalid ||
      !this.selectedSlot()
    )
      return;

    this.booking.set(true);

    const providerId = this.searchForm.value.providerId!;
    const slotId = this.selectedSlot()!.id;

    this.api
      .bookAppointment({
        providerId,
        slotId,
        customerName: this.bookForm.value.customerName!,
        customerEmail: this.bookForm.value.customerEmail!,
        customerPhone: this.bookForm.value.customerPhone!,
        reason: this.bookForm.value.reason ?? null,
      })
      .subscribe({
        next: (res) =>
          this.snack.open(`Booked: ${res.appointmentId}`, 'OK', {
            duration: 3500,
          }),
        error: (err) => {
          const msg = err?.error?.detail ?? err?.message ?? 'Booking failed';
          this.snack.open(msg, 'OK', { duration: 3500 });
        },
        complete: () => this.booking.set(false),
      });
  }

  trackById(_: number, s: SlotDto) {
    return s.id;
  }
}
