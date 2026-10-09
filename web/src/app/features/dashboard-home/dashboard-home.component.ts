import { Component, OnDestroy, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { DashboardStats } from '../../core/models';

type Sender = 'client' | 'ai' | 'system';

interface DemoMessage {
  sender: Sender;
  text: string;
  time: string;
}

interface DemoScenario {
  label: string;
  businessName: string;
  messages: DemoMessage[];
}

/** One animated WhatsApp-style conversation per business type, plus two cross-business-type feature
 * demos (follow-up, cancel/reschedule) - same technique as the landing page's hero demo, so there's
 * no real video to record for any of this. */
const DEMO_SCENARIOS: DemoScenario[] = [
  {
    label: 'Clinic',
    businessName: 'Sunrise Clinic',
    messages: [
      { sender: 'client', text: "Hi, I'd like to book a consultation with Dr. Rekha", time: '10:32 AM' },
      { sender: 'ai', text: 'Dr. Rekha has slots Tuesday and Thursday this week - which works for you?', time: '10:32 AM' },
      { sender: 'client', text: 'Thursday morning please', time: '10:33 AM' },
      { sender: 'ai', text: 'I have 9:30 AM and 11:00 AM open Thursday. Which would you like?', time: '10:33 AM' },
      { sender: 'client', text: '9:30 works', time: '10:33 AM' },
      { sender: 'system', text: 'Appointment confirmed - Dr. Rekha, Thursday 9:30 AM. Reminder sent 24 hours before.', time: '10:33 AM' }
    ]
  },
  {
    label: 'Salon',
    businessName: 'Glow Salon',
    messages: [
      { sender: 'client', text: 'Hey, can I get a haircut appointment this weekend?', time: '4:05 PM' },
      { sender: 'ai', text: 'We have Saturday 11:00 AM and Sunday 3:00 PM open for a haircut - which suits you?', time: '4:05 PM' },
      { sender: 'client', text: 'Saturday 11 is perfect', time: '4:06 PM' },
      { sender: 'system', text: 'Booked! Haircut, Saturday 11:00 AM at Glow Salon. See you then 💇', time: '4:06 PM' }
    ]
  },
  {
    label: 'Spa',
    businessName: 'Serene Spa',
    messages: [
      { sender: 'client', text: 'Do you have any slots for a relaxing massage tomorrow?', time: '6:14 PM' },
      { sender: 'ai', text: 'Tomorrow we have 2:00 PM and 4:30 PM open for a Swedish massage.', time: '6:14 PM' },
      { sender: 'client', text: '4:30 please', time: '6:15 PM' },
      { sender: 'system', text: 'Appointment confirmed - Swedish Massage, tomorrow 4:30 PM at Serene Spa.', time: '6:15 PM' }
    ]
  },
  {
    label: 'Gym',
    businessName: 'PowerFit Gym',
    messages: [
      { sender: 'client', text: 'Can I book a personal training session this week?', time: '7:40 AM' },
      { sender: 'ai', text: 'Coach Arjun has Wednesday 6:00 AM and Friday 7:00 AM open.', time: '7:40 AM' },
      { sender: 'client', text: 'Friday 7am works', time: '7:41 AM' },
      { sender: 'system', text: 'Session booked - Personal Training with Arjun, Friday 7:00 AM.', time: '7:41 AM' }
    ]
  },
  {
    label: 'Scanning Center',
    businessName: 'CityScan Diagnostics',
    messages: [
      { sender: 'client', text: 'I need to book an MRI scan', time: '11:02 AM' },
      { sender: 'ai', text: 'We have MRI slots Monday 10:00 AM and Wednesday 2:00 PM.', time: '11:02 AM' },
      { sender: 'client', text: 'Monday 10am', time: '11:03 AM' },
      { sender: 'system', text: 'MRI scan booked for Monday 10:00 AM. Please arrive 15 minutes early.', time: '11:03 AM' }
    ]
  },
  {
    label: 'Service Center',
    businessName: 'AutoCare Service Center',
    messages: [
      { sender: 'client', text: 'My car needs a service, any slots this week?', time: '9:12 AM' },
      { sender: 'ai', text: 'We have Tuesday 9:00 AM and Thursday 1:00 PM open for a full service.', time: '9:12 AM' },
      { sender: 'client', text: 'Thursday 1pm', time: '9:13 AM' },
      { sender: 'system', text: 'Service booked - Full Car Service, Thursday 1:00 PM at AutoCare.', time: '9:13 AM' }
    ]
  },
  {
    label: 'Follow-up',
    businessName: 'Sunrise Clinic',
    messages: [
      { sender: 'system', text: 'Staff marked the appointment Completed.', time: '12:15 PM' },
      { sender: 'ai', text: 'Hi Priya, thank you for visiting Sunrise Clinic today! Hope you feel better soon - reach out anytime 🙏', time: '12:15 PM' },
      {
        sender: 'ai',
        text: 'Quick follow-up from Dr. Rekha: continue Paracetamol 500mg twice daily, stay hydrated, and avoid strenuous exercise for a week.',
        time: '9:00 AM, next day'
      }
    ]
  },
  {
    label: 'Cancel & Reschedule',
    businessName: 'Glow Salon',
    messages: [
      { sender: 'client', text: 'Hi, I need to reschedule my appointment', time: '2:50 PM' },
      { sender: 'ai', text: 'No problem! You have a Haircut booked for Saturday 11:00 AM. What new day/time works?', time: '2:50 PM' },
      { sender: 'client', text: 'Can we do Sunday 2pm instead?', time: '2:51 PM' },
      { sender: 'system', text: 'Rescheduled - Haircut moved to Sunday 2:00 PM.', time: '2:51 PM' }
    ]
  }
];

const STEP_DELAY_MS = 1400;
const RESTART_PAUSE_MS = 2600;

@Component({
  selector: 'app-dashboard-home',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard-home.component.html',
  styleUrl: './dashboard-home.component.scss'
})
export class DashboardHomeComponent implements OnInit, OnDestroy {
  stats = signal<DashboardStats | null>(null);
  loading = signal(true);
  planLocked = signal(false);

  readonly scenarios = DEMO_SCENARIOS;
  activeScenarioIndex = signal(0);
  visibleCount = signal(0);
  isTyping = signal(false);

  readonly activeScenario = computed(() => this.scenarios[this.activeScenarioIndex()]);

  private timer: ReturnType<typeof setTimeout> | null = null;

  constructor(private readonly api: ApiService) {}

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

    this.playDemo();
  }

  ngOnDestroy(): void {
    if (this.timer) {
      clearTimeout(this.timer);
    }
  }

  selectScenario(index: number): void {
    if (index === this.activeScenarioIndex()) return;

    if (this.timer) {
      clearTimeout(this.timer);
    }
    this.activeScenarioIndex.set(index);
    this.visibleCount.set(0);
    this.isTyping.set(false);
    this.playDemo();
  }

  private playDemo(): void {
    const script = this.activeScenario().messages;

    if (this.visibleCount() >= script.length) {
      this.timer = setTimeout(() => {
        this.visibleCount.set(0);
        this.playDemo();
      }, RESTART_PAUSE_MS);
      return;
    }

    this.isTyping.set(true);
    this.timer = setTimeout(() => {
      this.isTyping.set(false);
      this.visibleCount.update((count) => count + 1);
      this.timer = setTimeout(() => this.playDemo(), STEP_DELAY_MS);
    }, 700);
  }
}
