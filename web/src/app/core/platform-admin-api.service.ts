import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { BusinessEnquiry, BusinessListItem, PlatformStats } from './models';

@Injectable({ providedIn: 'root' })
export class PlatformAdminApiService {
  private readonly base = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  listBusinesses(): Observable<BusinessListItem[]> {
    return this.http.get<BusinessListItem[]>(`${this.base}/api/platform-admin/businesses`);
  }

  getStats(): Observable<PlatformStats> {
    return this.http.get<PlatformStats>(`${this.base}/api/platform-admin/stats`);
  }

  listEnquiries(): Observable<BusinessEnquiry[]> {
    return this.http.get<BusinessEnquiry[]>(`${this.base}/api/platform-admin/enquiries`);
  }

  createEnquiry(payload: {
    businessName: string;
    businessType: string;
    contactName: string;
    phoneNumber: string;
    email: string | null;
    notes: string | null;
  }): Observable<string> {
    return this.http.post<string>(`${this.base}/api/platform-admin/enquiries`, payload);
  }

  convertEnquiry(enquiryId: string, timeZoneId: string): Observable<string> {
    return this.http.post<string>(`${this.base}/api/platform-admin/enquiries/${enquiryId}/convert`, { timeZoneId });
  }

  dismissEnquiry(enquiryId: string): Observable<void> {
    return this.http.post<void>(`${this.base}/api/platform-admin/enquiries/${enquiryId}/dismiss`, {});
  }
}
