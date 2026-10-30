export type TimesheetStatus = 'Draft' | 'Submitted' | 'Approved' | 'Rejected';

export interface TimesheetSummary {
  id: number;
  weekStartDate: string;
  status: TimesheetStatus;
  employeeName: string;
  totalHours: number;
  entryCount: number;
}

export interface TimeEntry {
  id: number;
  workDate: string;
  hours: number;
  description: string;
  projectId: number;
  projectName: string;
  projectCode: string;
}

export interface TimesheetDetail {
  id: number;
  weekStartDate: string;
  status: TimesheetStatus;
  employeeName: string;
  reviewComment?: string | null;
  totalHours: number;
  entries: TimeEntry[];
}

export interface Project {
  id: number;
  projectCode: string;
  name: string;
}

