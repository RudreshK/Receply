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

export interface BusinessTypeOption {
  value: string;
  label: string;
}

export const BUSINESS_TYPES: readonly BusinessTypeOption[] = [
  { value: 'Clinic', label: 'Clinic' },
  { value: 'ScanningCenter', label: 'Scanning Center' },
  { value: 'Saloon', label: 'Saloon' },
  { value: 'Gym', label: 'Gym' },
  { value: 'ServiceCenter', label: 'Service Center' }
];
export const DAYS_OF_WEEK = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'] as const;
