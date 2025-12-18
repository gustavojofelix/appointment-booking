import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FeatureBooking } from './feature-booking';

describe('FeatureBooking', () => {
  let component: FeatureBooking;
  let fixture: ComponentFixture<FeatureBooking>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FeatureBooking],
    }).compileComponents();

    fixture = TestBed.createComponent(FeatureBooking);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
