import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminAppointmentsPage } from './lib';

describe('AdminAppointmentsPage', () => {
  let component: AdminAppointmentsPage;
  let fixture: ComponentFixture<AdminAppointmentsPage>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminAppointmentsPage],
    }).compileComponents();

    fixture = TestBed.createComponent(AdminAppointmentsPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
