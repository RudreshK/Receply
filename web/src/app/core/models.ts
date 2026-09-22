export interface DashboardStats {
  totalClients: number;
  newClientsThisMonth: number;
  newClientsLastMonth: number;
}

export interface TenantSettings {
  tenantId: string;
  name: string;
  businessType: string;
  timeZoneId: string;
  status: string;
}

export interface Branch {
  id: string;
  name: string;
  address: string;
  isActive: boolean;
}

export interface BranchWorkingHours {
  dayOfWeek: string;
  openTime: string | null;
  closeTime: string | null;
  isClosed: boolean;
}

export interface Holiday {
  id: string;
  branchId: string | null;
  date: string;
  name: string;
}

export interface Faq {
  id: string;
  question: string;
  answer: string;
  isActive: boolean;
}

export interface AppointmentListItem {
  appointmentId: string;
  clientName: string;
  serviceName: string;
  resourceName: string;
  branchId: string;
  startUtc: string;
  endUtc: string;
  status: string;
}

export const BUSINESS_TYPES = ['Clinic', 'Salon', 'Gym', 'CarService', 'Other'] as const;
export const DAYS_OF_WEEK = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'] as const;
