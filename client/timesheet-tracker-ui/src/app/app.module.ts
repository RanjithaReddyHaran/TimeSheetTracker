import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { TimesheetDetailComponent } from './features/timesheets/timesheet-detail.component';
import { LoginComponent } from './features/auth/login.component';
import { CredentialsInterceptor } from './core/credentials.interceptor';

@NgModule({
  declarations: [AppComponent, DashboardComponent, TimesheetDetailComponent, LoginComponent],
  imports: [BrowserModule, HttpClientModule, FormsModule, ReactiveFormsModule, AppRoutingModule],
  providers: [{ provide: HTTP_INTERCEPTORS, useClass: CredentialsInterceptor, multi: true }],
  bootstrap: [AppComponent]
})
export class AppModule {}

