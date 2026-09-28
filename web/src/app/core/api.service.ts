import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  AppointmentListItem,
  Branch,
  BranchWorkingHours,
  DashboardStats,
  Faq,
  Holiday,
  TenantSettings
} from './models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly base = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  getDashboardStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.base}/api/dashboard/stats`);
  }

  getTenantSettings(): Observable<TenantSettings> {
    return this.http.get<TenantSettings>(`${this.base}/api/tenant/settings`);
  }

  updateTenantSettings(payload: { name: string; businessType: string; timeZoneId: string }): Observable<void> {
    return this.http.put<void>(`${this.base}/api/tenant/settings`, payload);
  }

  listBranches(): Observable<Branch[]> {
    return this.http.get<Branch[]>(`${this.base}/api/branches`);
  }

  getWorkingHours(branchId: string): Observable<BranchWorkingHours[]> {
    return this.http.get<BranchWorkingHours[]>(`${this.base}/api/branches/${branchId}/working-hours`);
  }

  setWorkingHours(branchId: string, entries: BranchWorkingHours[]): Observable<void> {
    return this.http.put<void>(`${this.base}/api/branches/${branchId}/working-hours`, { entries });
  }

  listHolidays(from: string, to: string): Observable<Holiday[]> {
    return this.http.get<Holiday[]>(`${this.base}/api/holidays`, { params: { from, to } });
  }

  createHoliday(payload: { branchId: string | null; date: string; name: string }): Observable<string> {
    return this.http.post<string>(`${this.base}/api/holidays`, payload);
  }

  deleteHoliday(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/api/holidays/${id}`);
  }

  listFaqs(): Observable<Faq[]> {
    return this.http.get<Faq[]>(`${this.base}/api/faqs`);
  }

  createFaq(payload: { question: string; answer: string }): Observable<string> {
    return this.http.post<string>(`${this.base}/api/faqs`, payload);
  }

  updateFaq(id: string, payload: { question: string; answer: string; isActive: boolean }): Observable<void> {
    return this.http.put<void>(`${this.base}/api/faqs/${id}`, payload);
  }

  deleteFaq(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/api/faqs/${id}`);
  }

  listAppointments(from: string, to: string): Observable<AppointmentListItem[]> {
    return this.http.get<AppointmentListItem[]>(`${this.base}/api/appointments`, { params: { from, to } });
  }
}
