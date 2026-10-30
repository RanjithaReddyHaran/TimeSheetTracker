import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { TimesheetDetailComponent } from './features/timesheets/timesheet-detail.component';
import { LoginComponent } from './features/auth/login.component';

const routes: Routes = [
  { path: '', component: DashboardComponent },
  { path: 'timesheets/:id', component: TimesheetDetailComponent },
  { path: 'login', component: LoginComponent },
  { path: '**', redirectTo: '' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}

