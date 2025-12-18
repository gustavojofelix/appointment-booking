export interface ProviderDto {
  id: string;
  name: string;
}

export interface SlotDto {
  id: string;
  startUtc: string;
  endUtc: string;
}

export interface BookAppointmentCommand {
  providerId: string;
  slotId: string;
  customerName: string;
  customerEmail: string;
  customerPhone: string;
  reason?: string | null;
}

export interface BookAppointmentResult {
  appointmentId: string;
}

export interface PagedResult<T> {
  page: number;
  pageSize: number;
  totalCount: number;
  items: T[];
}

export interface AppointmentRowDto {
  appointmentId: string;
  providerId: string;
  providerName: string;
  slotId: string;
  slotStartUtc: string;
  slotEndUtc: string;
  status: string;
  customerName: string;
  customerEmail: string;
  createdAtUtc: string;
}
