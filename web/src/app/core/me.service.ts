import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class MeService {
  private readonly http = inject(HttpClient);

  updatePreferredLanguage(languageCode: string | null): Observable<void> {
    return this.http.put<void>(`${environment.apiBaseUrl}/api/me/preferred-language`, { languageCode });
  }
}
