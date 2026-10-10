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

export interface BusinessBranding {
  tenantId: string;
  name: string;
  businessEmail: string | null;
  website: string | null;
  logoDataUrl: string | null;
  ownerName: string | null;
  designation: string | null;
  ownerPhotoDataUrl: string | null;
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
  clientId: string;
  clientName: string;
  clientPhoneNumber: string;
  serviceName: string;
  resourceName: string;
  branchId: string;
  startUtc: string;
  endUtc: string;
  status: string;
}

export interface PlanOption {
  plan: string;
  displayName: string;
  priceLabel: string;
  description: string;
  features: string[];
}

export interface SubscriptionStatus {
  plan: string;
  status: string;
  hasActiveAccess: boolean;
  trialEndsUtc: string | null;
  planRenewsUtc: string | null;
  availablePlans: PlanOption[];
}

export interface LandingChatMessage {
  role: 'user' | 'assistant';
  text: string;
}

export interface LandingChatReply {
  reply: string;
  enquirySubmitted: boolean;
}

export interface BusinessListItem {
  id: string;
  name: string;
  businessType: string;
  status: string;
  plan: string;
  hasActiveAccess: boolean;
  daysLeft: number | null;
  createdOn: string;
}

export interface PlatformStats {
  totalBusinesses: number;
  newThisWeek: number;
  newThisMonth: number;
  pendingEnquiries: number;
}

export interface BusinessEnquiry {
  id: string;
  businessName: string;
  businessType: string;
  contactName: string;
  phoneNumber: string;
  email: string | null;
  notes: string | null;
  status: string;
  convertedTenantId: string | null;
  createdOn: string;
}

export interface StaffListItem {
  id: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  role: string;
  isActive: boolean;
}

export interface StaffRoleOption {
  value: string;
  label: string;
}

export const STAFF_ROLES: readonly StaffRoleOption[] = [
  { value: 'Admin', label: 'Admin' },
  { value: 'FrontDesk', label: 'Front Desk' },
  { value: 'Provider', label: 'Provider' }
];

export interface BusinessTypeOption {
  value: string;
  label: string;
}

// Must match Receply.Domain.Tenancy.BusinessType exactly - these values are parsed server-side
// with Enum.TryParse, so a mismatched value here fails signup/settings with a 400.
export const BUSINESS_TYPES: readonly BusinessTypeOption[] = [
  { value: 'Clinic', label: 'Clinic' },
  { value: 'Salon', label: 'Salon' },
  { value: 'Gym', label: 'Gym' },
  { value: 'CarService', label: 'Car Service' },
  { value: 'Other', label: 'Other' }
];
export const DAYS_OF_WEEK = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'] as const;
