import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export type HandoffStatus = 'AiHandled' | 'HandoffRequested' | 'HumanAssigned' | 'Resolved';
export type MessageDirection = 'Inbound' | 'Outbound';
export type MessageSender = 'Client' | 'Ai' | 'Human';

export interface ConversationListItem {
  conversationId: string;
  clientId: string;
  clientName: string;
  clientPhoneNumber: string;
  status: HandoffStatus;
  lastMessagePreview: string | null;
  lastMessageAtUtc: string | null;
  clientCreatedOnUtc: string;
}

export interface ConversationMessage {
  messageId: string;
  direction: MessageDirection;
  sender: MessageSender;
  body: string;
  sentAtUtc: string;
}

@Injectable({ providedIn: 'root' })
export class ConversationsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/conversations`;

  list(): Observable<ConversationListItem[]> {
    return this.http.get<ConversationListItem[]>(this.baseUrl);
  }

  messages(conversationId: string): Observable<ConversationMessage[]> {
    return this.http.get<ConversationMessage[]>(`${this.baseUrl}/${conversationId}/messages`);
  }

  reply(conversationId: string, body: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${conversationId}/reply`, { body });
  }

  resumeAi(conversationId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${conversationId}/resume-ai`, {});
  }
}
