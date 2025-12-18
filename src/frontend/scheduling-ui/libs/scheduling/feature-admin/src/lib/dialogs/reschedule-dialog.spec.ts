import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RescheduleDialog } from './reschedule-dialog';

describe('RescheduleDialog', () => {
  let component: RescheduleDialog;
  let fixture: ComponentFixture<RescheduleDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RescheduleDialog],
    }).compileComponents();

    fixture = TestBed.createComponent(RescheduleDialog);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
