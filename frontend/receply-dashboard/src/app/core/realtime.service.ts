import { Injectable, OnDestroy, inject, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';
import { ConversationMessage, HandoffStatus } from './conversations.service';

export interface MessageAddedEvent {
  conversationId: string;
  message: ConversationMessage;
}

export interface StatusChangedEvent {
  conversationId: string;
  status: HandoffStatus;
}

export type ConnectionStatus = 'connecting' | 'connected' | 'reconnecting' | 'disconnected';

@Injectable({ providedIn: 'root' })
export class RealtimeService implements OnDestroy {
  private readonly auth = inject(AuthService);
  private connection: HubConnection | null = null;

  readonly status = signal<ConnectionStatus>('disconnected');
  readonly messageAdded$ = new Subject<MessageAddedEvent>();
  readonly statusChanged$ = new Subject<StatusChangedEvent>();
  /** Fires after a dropped connection comes back, so callers can re-sync anything they missed. */
  readonly reconnected$ = new Subject<void>();

  async start(): Promise<void> {
    if (this.connection && this.connection.state !== HubConnectionState.Disconnected) return;

    this.connection = new HubConnectionBuilder()
      .withUrl(`${environment.apiBaseUrl}/hubs/conversations`, {
        accessTokenFactory: () => this.auth.session()?.token ?? '',
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    this.connection.on('messageAdded', (event: MessageAddedEvent) => this.messageAdded$.next(event));
    this.connection.on('statusChanged', (event: StatusChangedEvent) => this.statusChanged$.next(event));
    this.connection.onreconnecting(() => this.status.set('reconnecting'));
    this.connection.onreconnected(() => {
      this.status.set('connected');
      this.reconnected$.next();
    });
    this.connection.onclose(() => this.status.set('disconnected'));

    this.status.set('connecting');
    try {
      await this.connection.start();
      this.status.set('connected');
    } catch {
      this.status.set('disconnected');
    }
  }

  async stop(): Promise<void> {
    await this.connection?.stop();
    this.connection = null;
    this.status.set('disconnected');
  }

  ngOnDestroy(): void {
    void this.stop();
  }
}
