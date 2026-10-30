import { Routes } from '@angular/router';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { TimesheetDetailComponent } from './features/timesheets/timesheet-detail.component';
import { LoginComponent } from './features/auth/login.component';

export const routes: Routes = [
  { path: '', component: DashboardComponent },
  { path: 'timesheets/:id', component: TimesheetDetailComponent },
  { path: 'login', component: LoginComponent },
  { path: '**', redirectTo: '' }
];

