import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import {
  AppointmentRowDto,
  BookAppointmentCommand,
  BookAppointmentResult,
  PagedResult,
  ProviderDto,
  SlotDto,
} from './scheduling-api.models';

@Injectable({ providedIn: 'root' })
export class SchedulingApi {
  private readonly http = inject(HttpClient);

  listProviders() {
    return this.http.get<ProviderDto[]>('/api/providers');
  }

  generateSlots(
    providerId: string,
    args: {
      days: number;
      slotMinutes: number;
      startHourLocal: number;
      endHourLocal: number;
    }
  ) {
    const url = `/api/admin/providers/${providerId}/slots/generate`;
    const params = new HttpParams()
      .set('days', args.days)
      .set('slotMinutes', args.slotMinutes)
      .set('startHourLocal', args.startHourLocal)
      .set('endHourLocal', args.endHourLocal);

    return this.http.post<{ created: number }>(url, null, { params });
  }

  listAvailableSlots(providerId: string, fromUtcIso: string, toUtcIso: string) {
    const params = new HttpParams()
      .set('fromUtc', fromUtcIso)
      .set('toUtc', toUtcIso);
    return this.http.get<SlotDto[]>(`/api/providers/${providerId}/slots`, {
      params,
    });
  }

  bookAppointment(cmd: BookAppointmentCommand) {
    return this.http.post<BookAppointmentResult>('/api/appointments', cmd);
  }

  cancelAppointment(appointmentId: string) {
    return this.http.put<void>(`/api/appointments/${appointmentId}/cancel`, {});
  }

  rescheduleAppointment(appointmentId: string, newSlotId: string) {
    return this.http.put<void>(
      `/api/appointments/${appointmentId}/reschedule`,
      { newSlotId }
    );
  }

  adminListAppointments(args: {
    page: number;
    pageSize: number;
    search?: string;
    status?: string;
  }) {
    let params = new HttpParams()
      .set('page', args.page)
      .set('pageSize', args.pageSize);
    if (args.search) params = params.set('search', args.search);
    if (args.status) params = params.set('status', args.status);
    return this.http.get<PagedResult<AppointmentRowDto>>(
      '/api/admin/appointments',
      { params }
    );
  }
}
