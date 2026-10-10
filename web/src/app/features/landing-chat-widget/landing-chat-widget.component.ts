import { Component, ElementRef, ViewChild, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/api.service';
import { LandingChatMessage } from '../../core/models';

@Component({
  selector: 'app-landing-chat-widget',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './landing-chat-widget.component.html',
  styleUrl: './landing-chat-widget.component.scss'
})
export class LandingChatWidgetComponent {
  @ViewChild('scrollAnchor') private scrollAnchor?: ElementRef<HTMLDivElement>;

  readonly open = signal(false);
  readonly messages = signal<LandingChatMessage[]>([
    { role: 'assistant', text: "Hi! I'm Receply's assistant. Ask me anything about how Receply works, or tell me about your business and I'll have our team reach out." }
  ]);
  readonly sending = signal(false);
  readonly submitted = signal(false);

  draft = '';

  constructor(private readonly api: ApiService) {}

  toggle(): void {
    this.open.set(!this.open());
  }

  send(): void {
    const text = this.draft.trim();
    if (!text || this.sending()) return;

    const history = this.messages();
    this.messages.set([...history, { role: 'user', text }]);
    this.draft = '';
    this.sending.set(true);
    this.scrollToBottom();

    this.api.sendLandingChatMessage(history, text).subscribe({
      next: (result) => {
        this.messages.set([...this.messages(), { role: 'assistant', text: result.reply }]);
        if (result.enquirySubmitted) this.submitted.set(true);
        this.sending.set(false);
        this.scrollToBottom();
      },
      error: () => {
        this.messages.set([
          ...this.messages(),
          { role: 'assistant', text: "Sorry, something went wrong on my end. You can also reach us directly - we'll get back to you." }
        ]);
        this.sending.set(false);
        this.scrollToBottom();
      }
    });
  }

  private scrollToBottom(): void {
    requestAnimationFrame(() => {
      this.scrollAnchor?.nativeElement.scrollIntoView({ behavior: 'smooth', block: 'end' });
    });
  }
}
