import { CommonModule, DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatRadioModule } from '@angular/material/radio';

import { SchedulingApi, SlotDto } from '@scheduling-ui/data-access';
import { FormsModule } from '@angular/forms';

export interface RescheduleDialogData {
  appointmentId: string;
  providerId: string;
  providerName?: string;
  currentSlotStartUtc?: string;
}

export interface RescheduleDialogResult {
  newSlotId: string;
}

@Component({
  standalone: true,
  selector: 'lib-scheduling-reschedule-dialog',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DatePipe,
    MatDialogModule,
    MatButtonModule,
    MatDividerModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatRadioModule,
    FormsModule,
  ],
  templateUrl: './reschedule-dialog.html',
  styleUrl: './reschedule-dialog.scss',
})
export class RescheduleDialog {
  private readonly api = inject(SchedulingApi);
  private readonly fb = inject(FormBuilder);
  private readonly dialogRef = inject(
    MatDialogRef<RescheduleDialog, RescheduleDialogResult | null>
  );
  readonly data = inject<RescheduleDialogData>(MAT_DIALOG_DATA);

  readonly loading = signal(false);
  readonly slots = signal<SlotDto[]>([]);
  readonly selectedSlotId = signal<string | null>(null);

  readonly rangeForm = this.fb.group({
    fromUtc: [new Date().toISOString(), Validators.required],
    toUtc: [
      new Date(Date.now() + 14 * 24 * 3600 * 1000).toISOString(),
      Validators.required,
    ],
  });

  constructor() {
    this.loadSlots();
  }

  loadSlots() {
    if (this.rangeForm.invalid) return;

    const fromUtc = this.rangeForm.value.fromUtc!;
    const toUtc = this.rangeForm.value.toUtc!;
    const providerId = this.data.providerId;

    this.loading.set(true);
    this.selectedSlotId.set(null);

    this.api.listAvailableSlots(providerId, fromUtc, toUtc).subscribe({
      next: (x) => this.slots.set(x),
      error: () => this.slots.set([]),
      complete: () => this.loading.set(false),
    });
  }

  cancel() {
    this.dialogRef.close(null);
  }

  confirm() {
    const id = this.selectedSlotId();
    if (!id) return;
    this.dialogRef.close({ newSlotId: id });
  }

  trackById(_: number, s: SlotDto) {
    return s.id;
  }
}
