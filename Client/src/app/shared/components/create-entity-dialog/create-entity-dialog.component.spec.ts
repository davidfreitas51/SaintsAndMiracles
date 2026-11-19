import { TestBed } from '@angular/core/testing';
import { describe, beforeEach, it, expect, vi } from 'vitest';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { of, throwError } from 'rxjs';

import { CreateEntityDialogComponent } from './create-entity-dialog.component';
import { EntityDialogData } from '../../../interfaces/entity-dialog-data';

describe('CreateEntityDialogComponent', () => {
  let component: CreateEntityDialogComponent;
  let dialogRef: { close: ReturnType<typeof vi.fn> };
  let data: EntityDialogData;

  beforeEach(() => {
    dialogRef = {
      close: vi.fn(),
    };

    data = {
      entityName: 'Tag',
      getAllFn: vi.fn(),
      updateFn: vi.fn(),
      deleteFn: vi.fn(),
      createFn: vi.fn().mockReturnValue(of(void 0)),
    };

    TestBed.configureTestingModule({
      imports: [CreateEntityDialogComponent],
      providers: [
        { provide: MatDialogRef, useValue: dialogRef },
        { provide: MAT_DIALOG_DATA, useValue: data },
      ],
    });

    component = TestBed.createComponent(
      CreateEntityDialogComponent,
    ).componentInstance;
  });

  it('does not submit when form is invalid', () => {
    component.form.controls.name.setValue('');

    component.create();

    expect(data.createFn).not.toHaveBeenCalled();
    expect(dialogRef.close).not.toHaveBeenCalled();
  });

  it('submits trimmed values and closes dialog', () => {
    component.form.controls.name.setValue('  New Tag  ');

    component.create();

    expect(data.createFn).toHaveBeenCalledWith('New Tag');
    expect(dialogRef.close).toHaveBeenCalledWith(true);
  });

  it('keeps dialog open on create failure', () => {
    vi.mocked(data.createFn).mockReturnValue(
      throwError(() => new Error('fail')),
    );

    component.form.controls.name.setValue('Tag');

    component.create();

    expect(dialogRef.close).not.toHaveBeenCalled();
    expect(component.loading).toBe(false);
  });
});
