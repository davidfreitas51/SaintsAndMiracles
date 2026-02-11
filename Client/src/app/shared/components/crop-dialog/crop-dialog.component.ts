import { Component, Inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import {
  MatDialogModule,
  MatDialogRef,
  MAT_DIALOG_DATA,
} from '@angular/material/dialog';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import {
  ImageCroppedEvent,
  ImageCropperComponent,
  LoadedImage,
} from 'ngx-image-cropper';

@Component({
  selector: 'app-crop-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, ImageCropperComponent],
  templateUrl: './crop-dialog.component.html',
  styleUrl: './crop-dialog.component.scss',
})
export class CropDialogComponent {
  imageChangedEvent: Event;
  croppedImage: SafeUrl = '';

  constructor(
    private sanitizer: DomSanitizer,
    private dialogRef: MatDialogRef<CropDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { imageChangedEvent: Event },
  ) {
    this.imageChangedEvent = data.imageChangedEvent;
  }

  onNoClick(): void {
    this.dialogRef.close();
  }

  async onCrop(): Promise<void> {
    if (!this.croppedImage) {
      this.dialogRef.close();
      return;
    }

    const blobUrl = (this.croppedImage as any)
      .changingThisBreaksApplicationSecurity as string;

    const base64 = await this.blobUrlToBase64(blobUrl);
    this.dialogRef.close(base64);
  }

  imageCropped(event: ImageCroppedEvent): void {
    this.croppedImage = this.sanitizer.bypassSecurityTrustUrl(
      event.objectUrl ?? '',
    );
  }

  async blobUrlToBase64(blobUrl: string): Promise<string> {
    const response = await fetch(blobUrl);
    const blob = await response.blob();
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onloadend = () => {
        resolve(reader.result as string);
      };
      reader.onerror = reject;
      reader.readAsDataURL(blob);
    });
  }
}
