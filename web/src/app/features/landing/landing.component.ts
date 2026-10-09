import { Component, OnDestroy, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { PlanOption } from '../../core/models';

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

interface Feature {
  icon: string;
  title: string;
  description: string;
}

interface Step {
  number: string;
  title: string;
  description: string;
}

/** One scripted, auto-typing WhatsApp conversation per business type, plus two cross-business-type
 * feature demos (follow-up, cancel/reschedule) - visitors switch between them with the tabs above
 * the phone mockup, so every business type sees itself represented without a single real video. */
const DEMO_SCENARIOS: DemoScenario[] = [
  {
    label: 'Clinic',
    businessName: 'Mehta Family Clinic',
    messages: [
      { sender: 'client', text: 'Hi, I need to book an appointment with Dr. Mehta', time: '10:32 AM' },
      { sender: 'ai', text: 'Dr. Mehta has slots available Tuesday and Thursday this week - which works best?', time: '10:32 AM' },
      { sender: 'client', text: 'Thursday please, morning if possible', time: '10:33 AM' },
      { sender: 'ai', text: 'I have 10:00 AM and 11:30 AM open Thursday. Which would you prefer?', time: '10:33 AM' },
      { sender: 'client', text: '10am works!', time: '10:33 AM' },
      { sender: 'system', text: 'Appointment confirmed - Dr. Mehta, Thursday 10:00 AM. Reminder sent 2 hours before.', time: '10:33 AM' }
    ]
  },
  {
    label: 'Appointment Reminder',
    businessName: 'Mehta Family Clinic',
    messages: [
      { sender: 'ai', text: 'Hi Priya! This is a reminder for your consultation with Dr. Mehta tomorrow at 10:00 AM.', time: '10:00 AM, day before' },
      { sender: 'ai', text: 'Reply here anytime if you need to reschedule or cancel.', time: '10:00 AM, day before' },
      { sender: 'client', text: 'Got it, thank you! See you then 😊', time: '10:04 AM, day before' },
      { sender: 'system', text: 'Reminder delivered 24 hours before the appointment - no action needed to confirm.', time: '10:04 AM, day before' }
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
    businessName: 'Mehta Family Clinic',
    messages: [
      { sender: 'system', text: 'Staff marked the appointment Completed.', time: '12:15 PM' },
      { sender: 'ai', text: 'Hi Priya, thank you for visiting today! Hope you feel better soon - reach out anytime 🙏', time: '12:15 PM' },
      {
        sender: 'ai',
        text: 'Quick follow-up from Dr. Mehta: continue Paracetamol 500mg twice daily, stay hydrated, and avoid strenuous exercise for a week.',
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

const FEATURE_LABELS: Record<string, string> = {
  ManualStaffReply: 'Staff can reply by hand and take chats back from the AI',
  ReadReceipts: 'WhatsApp read receipts and "typing…" indicator',
  AppointmentBooking: 'AI books, reschedules, and cancels appointments',
  MessageTranslation: 'On-demand message translation',
  AppointmentReminders: 'Automatic appointment reminders',
  AppointmentOutcomeMessages: 'Automatic completed / missed-appointment messages',
  AppointmentFollowUps: 'Post-appointment care follow-ups',
  PerformanceDashboard: 'Performance dashboard'
};

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.scss'
})
export class LandingComponent implements OnInit, OnDestroy {
  visibleCount = signal(0);
  isTyping = signal(false);
  activeScenarioIndex = signal(0);

  readonly scenarios = DEMO_SCENARIOS;
  readonly activeScenario = computed(() => this.scenarios[this.activeScenarioIndex()]);

  readonly features: Feature[] = [
    {
      icon: '24/7',
      title: 'Always-on AI receptionist',
      description: 'Every WhatsApp message gets an instant, on-brand reply - day or night, weekends included.'
    },
    {
      icon: '📅',
      title: 'Real appointment booking',
      description: 'Books, reschedules, and cancels against your actual calendar - no double-bookings, no back-and-forth.'
    },
    {
      icon: '💬',
      title: 'FAQ auto-reply',
      description: 'Teach it your policies once. It answers common questions the same way, every time.'
    },
    {
      icon: '🤝',
      title: 'Human handoff',
      description: "When a conversation needs a person, it says so and flags your team - nothing falls through the cracks."
    }
  ];

  readonly steps: Step[] = [
    { number: '1', title: 'Connect your WhatsApp number', description: 'Link your business WhatsApp in minutes - no new number needed.' },
    { number: '2', title: 'Tell us about your business', description: 'Services, hours, holidays, and FAQs - set once from your dashboard.' },
    { number: '3', title: 'Let Receply answer', description: 'Customers chat like normal. Receply handles it, and loops you in when it matters.' }
  ];

  plans = signal<PlanOption[]>([]);

  private timer: ReturnType<typeof setTimeout> | null = null;

  constructor(readonly auth: AuthService, private readonly api: ApiService) {}

  ngOnInit(): void {
    this.playDemo();
    this.api.listPublicPlans().subscribe({
      next: (plans) => this.plans.set(plans),
      error: () => this.plans.set([])
    });
  }

  featureLabel(key: string): string {
    return FEATURE_LABELS[key] ?? key;
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
