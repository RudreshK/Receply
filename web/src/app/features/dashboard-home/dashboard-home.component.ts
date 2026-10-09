import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { ApiService } from '../../core/api.service';
import { DashboardStats } from '../../core/models';

export interface DemoVideo {
  businessType: string;
  title: string;
  description: string;
  /** YouTube video id to embed. Null = not recorded yet, shown as a "Coming soon" card. */
  youtubeId: string | null;
}

/** One slide per business type's core booking flow, plus two cross-business-type feature demos.
 * Swap in a real youtubeId whenever each video is recorded - everything else just works. */
const DEMO_VIDEOS: DemoVideo[] = [
  { businessType: 'Clinic', title: 'Clinic booking demo', description: 'A patient books a consultation over WhatsApp, start to finish.', youtubeId: null },
  { businessType: 'Saloon', title: 'Salon booking demo', description: 'A client books a haircut slot with your AI receptionist.', youtubeId: null },
  { businessType: 'Spa', title: 'Spa booking demo', description: 'Booking a spa treatment and getting a confirmation.', youtubeId: null },
  { businessType: 'Gym', title: 'Gym booking demo', description: 'A member books a personal training session.', youtubeId: null },
  { businessType: 'Scanning Center', title: 'Scanning center booking demo', description: 'Booking a diagnostic scan appointment.', youtubeId: null },
  { businessType: 'Service Center', title: 'Service center booking demo', description: 'Booking a vehicle/appliance service slot.', youtubeId: null },
  { businessType: 'All businesses', title: 'Follow-up messages demo', description: 'Automatic care follow-ups after a completed appointment.', youtubeId: null },
  { businessType: 'All businesses', title: 'Cancel & reschedule demo', description: 'A client reschedules or cancels an existing booking by chat.', youtubeId: null }
];

@Component({
  selector: 'app-dashboard-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard-home.component.html',
  styleUrl: './dashboard-home.component.scss'
})
export class DashboardHomeComponent implements OnInit {
  stats = signal<DashboardStats | null>(null);
  loading = signal(true);
  planLocked = signal(false);

  readonly demoVideos = DEMO_VIDEOS;
  selectedVideo = signal<DemoVideo | null>(null);

  readonly selectedVideoUrl = computed<SafeResourceUrl | null>(() => {
    const id = this.selectedVideo()?.youtubeId;
    return id ? this.sanitizer.bypassSecurityTrustResourceUrl(`https://www.youtube.com/embed/${id}?autoplay=1`) : null;
  });

  constructor(private readonly api: ApiService, private readonly sanitizer: DomSanitizer) {}

  ngOnInit(): void {
    this.api.getDashboardStats().subscribe({
      next: (stats) => {
        this.stats.set(stats);
        this.loading.set(false);
      },
      error: (err: unknown) => {
        this.planLocked.set(err instanceof HttpErrorResponse && err.status === 403);
        this.loading.set(false);
      }
    });
  }

  openVideo(video: DemoVideo): void {
    if (video.youtubeId) {
      this.selectedVideo.set(video);
    }
  }

  closeVideo(): void {
    this.selectedVideo.set(null);
  }
}
