import { CommonModule, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { SchedulingApi, AppointmentRowDto } from '@scheduling-ui/data-access';

import { MatDialog } from '@angular/material/dialog';
import { ConfirmDialog } from '../lib/dialogs/confirm-dialog';
import { RescheduleDialog } from '../lib/dialogs/reschedule-dialog';

@Component({
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DatePipe,
    MatCardModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatPaginatorModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
  ],
  selector: 'lib-scheduling-admin-appointments-page',
  templateUrl: './admin-appointments-page.html',
  styleUrl: './admin-appointments-page.scss',
})
export class AdminAppointmentsPage {
  private readonly api = inject(SchedulingApi);
  private readonly fb = inject(FormBuilder);
  private readonly snack = inject(MatSnackBar);

  readonly cols = [
    'createdAtUtc',
    'providerName',
    'slotStartUtc',
    'customerEmail',
    'status',
    'actions',
  ];

  readonly rows = signal<AppointmentRowDto[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(false);

  readonly page = signal(1);
  readonly pageSize = signal(20);

  readonly filters = this.fb.group({
    search: [''],
    status: [''],
  });

  private readonly dialog = inject(MatDialog);

  constructor() {
    this.load();
  }

  load() {
    this.loading.set(true);

    const search = this.filters.value.search?.trim() || undefined;
    const status = this.filters.value.status?.trim() || undefined;

    this.api
      .adminListAppointments({
        page: this.page(),
        pageSize: this.pageSize(),
        search,
        status,
      })
      .subscribe({
        next: (res) => {
          this.rows.set(res.items);
          this.totalCount.set(res.totalCount);
        },
        error: (err) => {
          this.rows.set([]);
          this.totalCount.set(0);
          const msg =
            err?.error?.detail ?? err?.message ?? 'Failed to load appointments';
          this.snack.open(msg, 'OK', { duration: 3000 });
        },
        complete: () => this.loading.set(false),
      });
  }

  apply() {
    this.page.set(1);
    this.load();
  }

  reset() {
    this.filters.reset({ search: '', status: '' });
    this.page.set(1);
    this.pageSize.set(20);
    this.load();
  }

  onPage(e: PageEvent) {
    this.page.set(e.pageIndex + 1);
    this.pageSize.set(e.pageSize);
    this.load();
  }

  cancel1(row: AppointmentRowDto) {
    this.api.cancelAppointment(row.appointmentId).subscribe({
      next: () => {
        this.snack.open('Appointment cancelled', 'OK', { duration: 2000 });
        this.load();
      },
      error: (err) => {
        const msg = err?.error?.detail ?? err?.message ?? 'Cancel failed';
        this.snack.open(msg, 'OK', { duration: 3000 });
        this.load();
      },
    });
  }
  reschedule(row: AppointmentRowDto) {
    const ref = this.dialog.open(RescheduleDialog, {
      data: {
        appointmentId: row.appointmentId,
        providerId: row.providerId,
        providerName: row.providerName,
        currentSlotStartUtc: row.slotStartUtc,
      },
    });

    ref.afterClosed().subscribe((result) => {
      if (!result?.newSlotId) return;

      this.api
        .rescheduleAppointment(row.appointmentId, result.newSlotId)
        .subscribe({
          next: () => {
            this.snack.open('Appointment rescheduled', 'OK', {
              duration: 2000,
            });
            this.load();
          },
          error: (err) => {
            const msg =
              err?.error?.detail ?? err?.message ?? 'Reschedule failed';
            this.snack.open(msg, 'OK', { duration: 3000 });
            this.load();
          },
        });
    });
  }

  cancel(row: AppointmentRowDto) {
    const ref = this.dialog.open(ConfirmDialog, {
      data: {
        title: 'Cancel appointment',
        message: `Cancel appointment for ${row.customerEmail} at ${row.slotStartUtc}?`,
        confirmText: 'Cancel',
        confirmColor: 'warn',
      },
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;

      this.api.cancelAppointment(row.appointmentId).subscribe({
        next: () => {
          this.snack.open('Appointment cancelled', 'OK', { duration: 2000 });
          this.load();
        },
        error: (err) => {
          const msg = err?.error?.detail ?? err?.message ?? 'Cancel failed';
          this.snack.open(msg, 'OK', { duration: 3000 });
          this.load();
        },
      });
    });
  }
}
