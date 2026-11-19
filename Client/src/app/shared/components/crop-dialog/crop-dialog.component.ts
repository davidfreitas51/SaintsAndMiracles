import { Component, Inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import {
  MatDialogModule,
  MatDialogRef,
  MAT_DIALOG_DATA,
} from '@angular/material/dialog';
import { ImageCroppedEvent, ImageCropperComponent } from 'ngx-image-cropper';

@Component({
  selector: 'app-crop-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, ImageCropperComponent],
  templateUrl: './crop-dialog.component.html',
  styleUrl: './crop-dialog.component.scss',
})
export class CropDialogComponent {
  imageChangedEvent: Event;
  croppedImage = '';

  constructor(
    private dialogRef: MatDialogRef<CropDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { imageChangedEvent: Event },
  ) {
    this.imageChangedEvent = data.imageChangedEvent;
  }

  onNoClick(): void {
    this.dialogRef.close();
  }

  imageCropped(event: ImageCroppedEvent): void {
    this.croppedImage = event.objectUrl ?? '';
  }

  async onCrop(): Promise<void> {
    if (!this.croppedImage) {
      this.dialogRef.close();
      return;
    }

    try {
      const base64 = await this.blobUrlToBase64(this.croppedImage);
      this.dialogRef.close(base64);
    } catch (error) {
      console.error('Error converting cropped image:', error);
      this.dialogRef.close();
    }
  }

  private async blobUrlToBase64(blobUrl: string): Promise<string> {
    const response = await fetch(blobUrl);

    if (!response.ok) {
      throw new Error('Failed to read cropped image');
    }

    const blob = await response.blob();

    return new Promise((resolve, reject) => {
      const reader = new FileReader();

      reader.onload = () => {
        if (typeof reader.result === 'string') {
          resolve(reader.result);
        } else {
          reject(new Error('Failed to convert image to Base64'));
        }
      };

      reader.onerror = () => reject(reader.error);

      reader.readAsDataURL(blob);
    });
  }
}
