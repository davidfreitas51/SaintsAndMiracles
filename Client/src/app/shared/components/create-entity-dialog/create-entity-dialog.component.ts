import {
  Component,
  ElementRef,
  inject,
  ViewChild,
  AfterViewInit,
} from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { EntityDialogData } from '../../../interfaces/entity-dialog-data';
import { LowerCasePipe } from '@angular/common';
import { minMaxLengthValidator } from '../../validators/min-max-length.validator';

@Component({
  selector: 'app-create-entity-dialog',
  standalone: true,
  templateUrl: './create-entity-dialog.component.html',
  styleUrls: ['./create-entity-dialog.component.scss'],
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    LowerCasePipe,
  ],
})
export class CreateEntityDialogComponent implements AfterViewInit {
  @ViewChild('inputEl') inputEl!: ElementRef<HTMLInputElement>;

  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<CreateEntityDialogComponent>);
  private data = inject<EntityDialogData>(MAT_DIALOG_DATA);

  loading = false;

  form = this.fb.nonNullable.group({
    name: ['', [Validators.required, minMaxLengthValidator(3, 100)]],
  });

  get entityName(): string {
    return this.data.entityName;
  }

  ngAfterViewInit() {
    setTimeout(() => this.inputEl.nativeElement.focus());
  }

  create(): void {
    if (this.form.invalid) return;

    const name = this.form.controls.name.value.trim();

    if (!name) return;

    this.loading = true;

    this.data.createFn(name).subscribe({
      next: () => this.dialogRef.close(true),
      error: (err) => {
        console.error(`Failed to create ${this.entityName}`, err);
        this.loading = false;
      },
    });
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}
