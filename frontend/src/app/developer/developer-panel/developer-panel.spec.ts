import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DeveloperPanel } from './developer-panel';

describe('DeveloperPanel', () => {
  let component: DeveloperPanel;
  let fixture: ComponentFixture<DeveloperPanel>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DeveloperPanel],
    }).compileComponents();

    fixture = TestBed.createComponent(DeveloperPanel);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
