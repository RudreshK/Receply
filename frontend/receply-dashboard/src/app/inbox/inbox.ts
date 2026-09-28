import { Component, DestroyRef, ElementRef, OnInit, computed, inject, signal, viewChild } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../core/auth.service';
import {
  ConversationListItem,
  ConversationMessage,
  ConversationsService,
  HandoffStatus,
  MessageSender,
} from '../core/conversations.service';
import { FeatureKey, FeaturesService } from '../core/features.service';
import { MeService } from '../core/me.service';
import { MessageAddedEvent, MessageStatusChangedEvent, RealtimeService, StatusChangedEvent } from '../core/realtime.service';

interface MessageGroup {
  label: string;
  messages: ConversationMessage[];
}

interface TranslationState {
  status: 'loading' | 'shown' | 'error';
  text: string;
}

const STATUS_LABELS: Record<HandoffStatus, string> = {
  AiHandled: 'AI',
  HandoffRequested: 'Needs staff',
  HumanAssigned: 'With staff',
  Resolved: 'Resolved',
};

/** No language-detection service is wired up - this is a cheap, purely client-side heuristic
 * (no AI call) to decide whether a message is plain English text, so the Translate button can hide
 * itself for messages that are already in the staff member's target language. */
function looksLikeEnglish(text: string): boolean {
  return /^[\x00-\x7F]*$/.test(text);
}

/** Common languages a business owner/staff might want the dashboard translated into. */
const LANGUAGES: { code: string; name: string }[] = [
  { code: 'en', name: 'English' },
  { code: 'hi', name: 'Hindi' },
  { code: 'te', name: 'Telugu' },
  { code: 'ta', name: 'Tamil' },
  { code: 'kn', name: 'Kannada' },
  { code: 'ml', name: 'Malayalam' },
  { code: 'bn', name: 'Bengali' },
  { code: 'mr', name: 'Marathi' },
  { code: 'gu', name: 'Gujarati' },
  { code: 'pa', name: 'Punjabi' },
  { code: 'ur', name: 'Urdu' },
];

@Component({
  selector: 'app-inbox',
  imports: [FormsModule],
  templateUrl: './inbox.html',
  styleUrl: './inbox.scss',
})
export class Inbox implements OnInit {
  private readonly conversationsApi = inject(ConversationsService);
  private readonly realtime = inject(RealtimeService);
  private readonly auth = inject(AuthService);
  private readonly meApi = inject(MeService);
  private readonly featuresApi = inject(FeaturesService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  private readonly chatScroll = viewChild<ElementRef<HTMLElement>>('chatScroll');

  protected readonly session = this.auth.session;
  protected readonly connection = this.realtime.status;
  protected readonly languages = LANGUAGES;

  protected readonly translations = signal<Record<string, TranslationState>>({});
  protected readonly savingLanguage = signal(false);

  protected readonly conversations = signal<ConversationListItem[]>([]);
  protected readonly loadingList = signal(true);
  protected readonly listError = signal<string | null>(null);
  protected readonly search = signal('');

  protected readonly selectedId = signal<string | null>(null);
  protected readonly messages = signal<ConversationMessage[]>([]);
  protected readonly loadingMessages = signal(false);
  protected readonly messagesError = signal<string | null>(null);

  protected readonly draft = signal('');
  protected readonly sending = signal(false);
  protected readonly replyError = signal<string | null>(null);
  protected readonly resumingAi = signal(false);

  protected readonly sortedConversations = computed(() => {
    const term = this.search().trim().toLowerCase();
    return this.conversations()
      .filter(c => !term || c.clientName.toLowerCase().includes(term) || c.clientPhoneNumber.includes(term))
      .sort((a, b) => this.activityTime(b) - this.activityTime(a));
  });

  protected readonly selected = computed(() =>
    this.conversations().find(c => c.conversationId === this.selectedId()) ?? null);

  protected readonly stats = computed(() => {
    const now = new Date();
    const startOfToday = new Date(now.getFullYear(), now.getMonth(), now.getDate()).getTime();
    const startOfWeek = startOfToday - 6 * 24 * 60 * 60 * 1000;
    const created = this.conversations().map(c => new Date(c.clientCreatedOnUtc).getTime());
    return {
      total: new Set(this.conversations().map(c => c.clientId)).size,
      today: created.filter(t => t >= startOfToday).length,
      week: created.filter(t => t >= startOfWeek).length,
      needsStaff: this.conversations().filter(c => c.status === 'HandoffRequested').length,
    };
  });

  protected readonly messageGroups = computed<MessageGroup[]>(() => {
    const groups: MessageGroup[] = [];
    for (const message of this.messages()) {
      const label = this.dayLabel(message.sentAtUtc);
      const last = groups.at(-1);
      if (last && last.label === label) last.messages.push(message);
      else groups.push({ label, messages: [message] });
    }
    return groups;
  });

  ngOnInit(): void {
    this.loadConversations();
    void this.featuresApi.load();

    this.realtime.messageAdded$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(event => this.onMessageAdded(event));

    this.realtime.statusChanged$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(event => this.onStatusChanged(event));

    this.realtime.messageStatusChanged$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(event => this.onMessageStatusChanged(event));

    this.realtime.reconnected$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.loadConversations();
      const selected = this.selectedId();
      if (selected) this.loadMessages(selected);
    });

    void this.realtime.start();
  }

  protected select(conversation: ConversationListItem): void {
    if (this.selectedId() === conversation.conversationId) return;
    this.selectedId.set(conversation.conversationId);
    this.draft.set('');
    this.replyError.set(null);
    this.translations.set({});
    this.loadMessages(conversation.conversationId);
  }

  protected closeChat(): void {
    this.selectedId.set(null);
    this.messages.set([]);
  }

  protected featureEnabled(feature: FeatureKey): boolean {
    return this.featuresApi.isEnabled(feature);
  }

  protected canReplyManually(conversation: ConversationListItem): boolean {
    return this.featureEnabled('ManualStaffReply')
      && (conversation.status === 'HandoffRequested' || conversation.status === 'HumanAssigned');
  }

  protected sendReply(conversationId: string): void {
    const body = this.draft().trim();
    if (!body || this.sending()) return;

    this.sending.set(true);
    this.replyError.set(null);
    this.conversationsApi.reply(conversationId, body).subscribe({
      next: () => {
        this.draft.set('');
        this.sending.set(false);
        // The reply hands the thread to a human on the server; reflect that immediately without
        // waiting on the statusChanged push (which still arrives and confirms it).
        this.setStatus(conversationId, 'HumanAssigned');
      },
      error: () => {
        this.sending.set(false);
        this.replyError.set('Could not send. Please try again.');
      },
    });
  }

  protected resumeAi(conversationId: string): void {
    if (this.resumingAi()) return;

    this.resumingAi.set(true);
    this.conversationsApi.resumeAi(conversationId).subscribe({
      next: () => {
        this.resumingAi.set(false);
        this.setStatus(conversationId, 'AiHandled');
      },
      error: () => {
        this.resumingAi.set(false);
      },
    });
  }

  /** Hides the Translate button when the message is already in the staff member's target language -
   * today that only recognizes "target is English and the text is plain ASCII", since that's the
   * one direction we can tell apart without spending an AI call just to detect the language. */
  protected canTranslate(message: ConversationMessage): boolean {
    const target = this.session()?.preferredLanguage || 'en';
    return !(target === 'en' && looksLikeEnglish(message.body));
  }

  protected displayText(message: ConversationMessage): string {
    const translation = this.translations()[message.messageId];
    return translation?.status === 'shown' ? translation.text : message.body;
  }

  protected translateLabel(message: ConversationMessage): string {
    switch (this.translations()[message.messageId]?.status) {
      case 'loading': return 'Translating…';
      case 'shown': return 'Show original';
      case 'error': return 'Translate (failed - retry)';
      default: return 'Translate';
    }
  }

  protected toggleTranslate(message: ConversationMessage): void {
    const conversationId = this.selectedId();
    if (!conversationId) return;

    const current = this.translations()[message.messageId];
    if (current?.status === 'shown') {
      // Toggle back to the original text - already in hand, no need to call the server again.
      this.translations.update(map => {
        const next = { ...map };
        delete next[message.messageId];
        return next;
      });
      return;
    }
    if (current?.status === 'loading') return;

    const languageCode = this.session()?.preferredLanguage || 'en';
    this.translations.update(map => ({ ...map, [message.messageId]: { status: 'loading', text: '' } }));

    this.conversationsApi.translate(conversationId, message.messageId, languageCode).subscribe({
      next: result => {
        this.translations.update(map => ({ ...map, [message.messageId]: { status: 'shown', text: result.translatedText } }));
      },
      error: () => {
        this.translations.update(map => ({ ...map, [message.messageId]: { status: 'error', text: '' } }));
      },
    });
  }

  protected setPreferredLanguage(languageCode: string): void {
    this.savingLanguage.set(true);
    this.meApi.updatePreferredLanguage(languageCode || null).subscribe({
      next: () => {
        this.savingLanguage.set(false);
        this.auth.setPreferredLanguage(languageCode || null);
        this.translations.set({}); // Old translations were in the previous language - drop them.
      },
      error: () => this.savingLanguage.set(false),
    });
  }

  protected async logout(): Promise<void> {
    await this.realtime.stop();
    this.auth.logout();
    this.featuresApi.reset();
    this.router.navigate(['/login']);
  }

  protected statusLabel(status: HandoffStatus): string {
    return STATUS_LABELS[status];
  }

  protected senderLabel(sender: MessageSender): string {
    return sender === 'Ai' ? 'AI assistant' : sender === 'Human' ? 'Staff' : '';
  }

  protected initials(name: string): string {
    const letters = name.replace(/[^\p{L}\s]/gu, '').trim().split(/\s+/).filter(Boolean);
    if (letters.length === 0) return '#';
    return (letters[0][0] + (letters[1]?.[0] ?? '')).toUpperCase();
  }

  protected listTime(item: ConversationListItem): string {
    const value = item.lastMessageAtUtc ?? item.clientCreatedOnUtc;
    const date = new Date(value);
    const label = this.dayLabel(value);
    if (label === 'Today') return date.toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' });
    return label;
  }

  protected messageTime(value: string): string {
    return new Date(value).toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' });
  }

  protected showTick(message: ConversationMessage): boolean {
    return message.direction === 'Outbound' && message.deliveryStatus !== 'Pending';
  }

  protected isNewToday(item: ConversationListItem): boolean {
    return this.dayLabel(item.clientCreatedOnUtc) === 'Today';
  }

  private loadConversations(): void {
    this.conversationsApi.list().subscribe({
      next: items => {
        this.conversations.set(items);
        this.loadingList.set(false);
        this.listError.set(null);
      },
      error: () => {
        this.loadingList.set(false);
        this.listError.set('Could not load clients. Check your connection and refresh.');
      },
    });
  }

  private loadMessages(conversationId: string): void {
    this.loadingMessages.set(true);
    this.messagesError.set(null);
    this.messages.set([]);
    this.conversationsApi.messages(conversationId).subscribe({
      next: messages => {
        if (this.selectedId() !== conversationId) return;
        this.messages.set(messages);
        this.loadingMessages.set(false);
        this.scrollToBottom();
      },
      error: () => {
        if (this.selectedId() !== conversationId) return;
        this.loadingMessages.set(false);
        this.messagesError.set('Could not load this conversation.');
      },
    });
  }

  private onMessageAdded({ conversationId, message }: MessageAddedEvent): void {
    const known = this.conversations().some(c => c.conversationId === conversationId);
    if (!known) {
      // A brand-new client/conversation - the list needs its name and creation time from the server.
      this.loadConversations();
    } else {
      this.conversations.update(items => items.map(c => c.conversationId === conversationId
        ? { ...c, lastMessagePreview: message.body, lastMessageAtUtc: message.sentAtUtc }
        : c));
    }

    if (this.selectedId() === conversationId && !this.messages().some(m => m.messageId === message.messageId)) {
      this.messages.update(messages => [...messages, message]);
      this.scrollToBottom();
    }
  }

  private onStatusChanged({ conversationId, status }: StatusChangedEvent): void {
    this.setStatus(conversationId, status);
  }

  private onMessageStatusChanged({ messageId, status }: MessageStatusChangedEvent): void {
    this.messages.update(messages => messages.map(m => m.messageId === messageId ? { ...m, deliveryStatus: status } : m));
  }

  private setStatus(conversationId: string, status: HandoffStatus): void {
    this.conversations.update(items => items.map(c => c.conversationId === conversationId ? { ...c, status } : c));
  }

  private scrollToBottom(): void {
    setTimeout(() => {
      const el = this.chatScroll()?.nativeElement;
      if (el) el.scrollTop = el.scrollHeight;
    });
  }

  private activityTime(item: ConversationListItem): number {
    return new Date(item.lastMessageAtUtc ?? item.clientCreatedOnUtc).getTime();
  }

  private dayLabel(value: string): string {
    const date = new Date(value);
    const today = new Date();
    const startOfToday = new Date(today.getFullYear(), today.getMonth(), today.getDate());
    const startOfDate = new Date(date.getFullYear(), date.getMonth(), date.getDate());
    const days = Math.round((startOfToday.getTime() - startOfDate.getTime()) / (24 * 60 * 60 * 1000));
    if (days === 0) return 'Today';
    if (days === 1) return 'Yesterday';
    return date.toLocaleDateString([], { day: 'numeric', month: 'short', year: date.getFullYear() === today.getFullYear() ? undefined : 'numeric' });
  }
}
