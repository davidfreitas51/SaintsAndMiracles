import { TestBed } from '@angular/core/testing';
import { CreateEntityDialogComponent } from './create-entity-dialog.component';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { of, throwError } from 'rxjs';
import { EntityDialogData } from '../../../interfaces/entity-dialog-data';

describe('CreateEntityDialogComponent', () => {
  let component: CreateEntityDialogComponent;
  let dialogRef: jasmine.SpyObj<MatDialogRef<CreateEntityDialogComponent>>;
  let data: EntityDialogData;

  beforeEach(() => {
    dialogRef = jasmine.createSpyObj('MatDialogRef', ['close']);
    data = {
      entityName: 'Tag',
      getAllFn: jasmine.createSpy('getAllFn'),
      updateFn: jasmine.createSpy('updateFn'),
      deleteFn: jasmine.createSpy('deleteFn'),
      createFn: jasmine.createSpy('createFn').and.returnValue(of(void 0)),
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
    (data.createFn as jasmine.Spy).and.returnValue(
      throwError(() => new Error('fail')),
    );

    component.form.controls.name.setValue('Tag');

    component.create();

    expect(dialogRef.close).not.toHaveBeenCalled();
    expect(component.loading).toBeFalse();
  });
});
