import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { BrandingService } from '../../core/branding.service';
import { BusinessBranding } from '../../core/models';

const MAX_IMAGE_BYTES = 2 * 1024 * 1024;

@Component({
  selector: 'app-business-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './business-profile.component.html',
  styleUrl: './business-profile.component.scss'
})
export class BusinessProfileComponent implements OnInit {
  loading = signal(true);
  saving = signal(false);
  saved = signal(false);

  name = '';
  businessEmail = '';
  website = '';
  ownerName = '';
  designation = '';
  logoDataUrl: string | null = null;
  ownerPhotoDataUrl: string | null = null;

  errors: Record<string, string> = {};

  readonly isOwner = computed(() => this.auth.currentUser()?.role === 'Owner');

  constructor(
    private readonly api: ApiService,
    private readonly branding: BrandingService,
    private readonly auth: AuthService
  ) {}

  ngOnInit(): void {
    this.api.getBranding().subscribe({
      next: (branding) => {
        this.fillForm(branding);
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  private fillForm(branding: BusinessBranding): void {
    this.name = branding.name;
    this.businessEmail = branding.businessEmail || '';
    this.website = branding.website || '';
    this.ownerName = branding.ownerName || '';
    this.designation = branding.designation || '';
    this.logoDataUrl = branding.logoDataUrl;
    this.ownerPhotoDataUrl = branding.ownerPhotoDataUrl;
  }

  async onLogoSelected(event: Event): Promise<void> {
    await this.handleImageSelected(event, 512, (dataUrl) => (this.logoDataUrl = dataUrl), 'logo');
  }

  async onOwnerPhotoSelected(event: Event): Promise<void> {
    await this.handleImageSelected(event, 400, (dataUrl) => (this.ownerPhotoDataUrl = dataUrl), 'ownerPhoto');
  }

  removeLogo(): void {
    this.logoDataUrl = null;
  }

  removeOwnerPhoto(): void {
    this.ownerPhotoDataUrl = null;
  }

  private async handleImageSelected(
    event: Event,
    maxSide: number,
    apply: (dataUrl: string) => void,
    errorKey: string
  ): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;

    this.errors[errorKey] = '';
    try {
      apply(await this.readAndResizeImage(file, maxSide));
    } catch (message) {
      this.errors[errorKey] = typeof message === 'string' ? message : 'Could not read that image.';
    }
  }

  private readAndResizeImage(file: File, maxSide: number): Promise<string> {
    return new Promise((resolve, reject) => {
      if (!file.type.startsWith('image/')) {
        reject('Please choose an image file.');
        return;
      }
      if (file.size > MAX_IMAGE_BYTES) {
        reject('Image must be 2 MB or smaller.');
        return;
      }

      const reader = new FileReader();
      reader.onerror = () => reject("Couldn't read that file.");
      reader.onload = () => {
        const result = reader.result as string;
        if (file.type === 'image/svg+xml') {
          resolve(result);
          return;
        }
        const img = new Image();
        img.onerror = () => reject("That image couldn't be opened.");
        img.onload = () => {
          const scale = Math.min(1, maxSide / Math.max(img.width, img.height));
          const canvas = document.createElement('canvas');
          canvas.width = Math.round(img.width * scale);
          canvas.height = Math.round(img.height * scale);
          const ctx = canvas.getContext('2d');
          if (!ctx) {
            reject("That image couldn't be processed.");
            return;
          }
          ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
          resolve(canvas.toDataURL(file.type === 'image/png' ? 'image/png' : 'image/jpeg', 0.88));
        };
        img.src = result;
      };
      reader.readAsDataURL(file);
    });
  }

  private validate(): boolean {
    this.errors = {};
    const name = this.name.trim();
    const email = this.businessEmail.trim();
    const owner = this.ownerName.trim();

    if (!name) this.errors['name'] = 'Business name is required.';
    if (!email) this.errors['businessEmail'] = 'Business email is required.';
    else if (!/^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/.test(email)) this.errors['businessEmail'] = 'Enter a valid email address.';
    if (!owner) this.errors['ownerName'] = 'Owner name is required.';

    if (this.website.trim()) {
      try {
        const url = new URL(this.normalizeUrl(this.website));
        if (!url.hostname.includes('.')) throw new Error('invalid');
      } catch {
        this.errors['website'] = 'Enter a valid website, e.g. www.example.com';
      }
    }

    return Object.keys(this.errors).length === 0;
  }

  private normalizeUrl(url: string): string {
    const trimmed = url.trim();
    if (!trimmed) return '';
    return /^https?:\/\//i.test(trimmed) ? trimmed : `https://${trimmed}`;
  }

  save(): void {
    if (!this.isOwner()) return;
    this.saved.set(false);
    if (!this.validate()) return;

    this.saving.set(true);
    this.api
      .updateBranding({
        name: this.name.trim(),
        businessEmail: this.businessEmail.trim().toLowerCase() || null,
        website: this.website.trim() ? this.normalizeUrl(this.website) : null,
        ownerName: this.ownerName.trim() || null,
        designation: this.designation.trim() || null,
        logoDataUrl: this.logoDataUrl,
        ownerPhotoDataUrl: this.ownerPhotoDataUrl
      })
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.saved.set(true);
          this.branding.refresh();
        },
        error: () => this.saving.set(false)
      });
  }
}
