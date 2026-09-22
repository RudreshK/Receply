import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth.service';

type Sender = 'client' | 'ai' | 'system';

interface DemoMessage {
  sender: Sender;
  text: string;
  time: string;
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

const DEMO_SCRIPT: DemoMessage[] = [
  { sender: 'client', text: 'Hi, I need to book an appointment with Dr. Mehta', time: '10:32 AM' },
  {
    sender: 'ai',
    text: "Hello! I'm the Receply receptionist for Mehta Family Clinic. Happy to help you book an appointment.",
    time: '10:32 AM'
  },
  { sender: 'ai', text: 'Dr. Mehta has slots available Tuesday and Thursday this week - which works best?', time: '10:32 AM' },
  { sender: 'client', text: 'Thursday please, morning if possible', time: '10:33 AM' },
  { sender: 'ai', text: 'I have 10:00 AM and 11:30 AM open Thursday. Which would you prefer?', time: '10:33 AM' },
  { sender: 'client', text: '10am works!', time: '10:33 AM' },
  {
    sender: 'system',
    text: 'Appointment confirmed - Dr. Mehta, Thursday 10:00 AM, Mehta Family Clinic. Reminder sent 2 hours before.',
    time: '10:33 AM'
  }
];

const STEP_DELAY_MS = 1500;
const RESTART_PAUSE_MS = 3200;

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

  readonly demoScript = DEMO_SCRIPT;

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

  private timer: ReturnType<typeof setTimeout> | null = null;

  constructor(readonly auth: AuthService) {}

  ngOnInit(): void {
    this.playDemo();
  }

  ngOnDestroy(): void {
    if (this.timer) {
      clearTimeout(this.timer);
    }
  }

  private playDemo(): void {
    if (this.visibleCount() >= this.demoScript.length) {
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
