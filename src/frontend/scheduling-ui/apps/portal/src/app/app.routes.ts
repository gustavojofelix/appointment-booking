import { Route } from '@angular/router';

export const appRoutes: Route[] = [
  {
    path: 'booking',
    loadComponent: () =>
      import('@scheduling-ui/feature-booking').then((m) => m.BookingPage),
  },
  {
    path: 'admin/appointments',
    loadComponent: () =>
      import('@scheduling-ui/feature-admin').then(
        (m) => m.AdminAppointmentsPage
      ),
  },
  { path: '', pathMatch: 'full', redirectTo: 'booking' },
  { path: '**', redirectTo: 'booking' },
];
