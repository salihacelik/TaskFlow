import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TesterPanel } from './tester-panel';

describe('TesterPanel', () => {
  let component: TesterPanel;
  let fixture: ComponentFixture<TesterPanel>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TesterPanel],
    }).compileComponents();

    fixture = TestBed.createComponent(TesterPanel);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
