import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { TimesheetApiService } from '../../core/timesheet-api.service';
import { TimesheetSummary } from '../../core/models';

@Component({
  selector: 'app-dashboard',
  template: `
    <div class="welcome-row">
      <div><div class="eyebrow">YOUR WORKSPACE</div><h1>Good morning, Evan <span>✦</span></h1><p class="subheading">Here's what's happening with your time this week.</p></div>
      <button class="primary-button" (click)="createCurrentWeek()" [disabled]="creating">＋ New timesheet</button>
    </div>
    <div class="stats-grid">
      <article class="stat-card"><div class="stat-top"><span>HOURS THIS WEEK</span><span class="stat-icon purple">◷</span></div><div class="stat-value">32.5 <small>/ 40 hrs</small></div><div class="progress"><span style="width:81.25%"></span></div><div class="stat-foot"><b class="positive">81%</b> of your weekly target</div></article>
      <article class="stat-card"><div class="stat-top"><span>SUBMITTED</span><span class="stat-icon green">✓</span></div><div class="stat-value">3 <small>timesheets</small></div><div class="stat-foot">Across the last 4 weeks</div><div class="mini-bars"><i style="height:30%"></i><i style="height:55%"></i><i style="height:43%"></i><i style="height:75%"></i><i style="height:60%"></i><i style="height:92%"></i><i style="height:70%"></i></div></article>
      <article class="stat-card"><div class="stat-top"><span>PENDING APPROVAL</span><span class="stat-icon amber">◷</span></div><div class="stat-value">1 <small>timesheet</small></div><div class="stat-foot"><span class="dot amber-dot"></span> Awaiting manager review</div><div class="stat-link">View pending submission →</div></article>
      <article class="stat-card"><div class="stat-top"><span>APPROVED HOURS</span><span class="stat-icon blue">✓</span></div><div class="stat-value">146.5 <small>hrs</small></div><div class="stat-foot"><b class="positive">+8.5%</b> vs. previous month</div><div class="stat-link">View hours report →</div></article>
    </div>
    <div class="content-grid">
      <section class="panel timesheet-panel">
        <div class="panel-heading"><div><h2>Recent timesheets</h2><p>Track your submissions and approvals</p></div><a href="#">View all →</a></div>
        <div class="table-wrap">
          <table><thead><tr><th>WEEK</th><th>HOURS</th><th>STATUS</th><th>UPDATED</th><th></th></tr></thead>
          <tbody>
            <tr *ngFor="let sheet of sheets">
              <td><b>{{ sheet.weekStartDate | date:'MMM d' }}</b><small>Weekly timesheet</small></td>
              <td><b>{{ sheet.totalHours | number:'1.1-1' }} hrs</b></td>
              <td><span [ngClass]="statusClass(sheet.status)">{{ sheet.status }}</span></td>
              <td>{{ sheet.weekStartDate | date:'MMM d, y' }}</td>
              <td><a class="row-link" [routerLink]="['/timesheets', sheet.id]">View ↗</a></td>
            </tr>
            <tr *ngIf="!loading && sheets.length === 0"><td colspan="5" class="empty">No timesheets yet. Create one to get started.</td></tr>
          </tbody></table>
        </div>
        <div *ngIf="error" class="inline-error">{{ error }}</div>
        <div *ngIf="loading" class="loading">Loading timesheets…</div>
      </section>
      <aside class="right-column">
        <section class="panel week-panel"><div class="panel-heading"><div><h2>This week</h2><p>Weekly progress overview</p></div><span class="calendar-icon">▦</span></div>
          <div class="week-total"><b>32.5</b><span>/ 40 hours</span></div><div class="progress large"><span style="width:81.25%"></span></div>
          <div class="week-days"><div><small>MON</small><b>8.0</b><i class="filled"></i></div><div><small>TUE</small><b>7.5</b><i class="filled"></i></div><div><small>WED</small><b>8.0</b><i class="filled"></i></div><div><small>THU</small><b>9.0</b><i class="filled"></i></div><div><small>FRI</small><b>—</b><i></i></div></div>
          <a class="full-link" routerLink="/">Open weekly timesheet →</a>
        </section>
        <section class="tip-card"><div class="tip-spark">✦</div><b>Stay on top of your time</b><p>Submit your timesheet by Friday at 5 PM to keep approvals on schedule.</p><a href="#">Timesheet guidelines ↗</a></section>
      </aside>
    </div>
  `
})
export class DashboardComponent implements OnInit {
  sheets: TimesheetSummary[] = [];
  loading = true;
  creating = false;
  error = '';

  constructor(private readonly api: TimesheetApiService, private readonly router: Router) {}

  ngOnInit(): void {
    this.api.list().subscribe({
      next: rows => { this.sheets = rows; this.loading = false; },
      error: () => { this.error = 'Could not load timesheets. Please sign in and try again.'; this.loading = false; }
    });
  }

  statusClass(status: string): string { return `status ${status.toLowerCase()}`; }

  createCurrentWeek(): void {
    if (this.creating) return;
    this.creating = true;
    const today = new Date();
    const offset = (today.getDay() + 6) % 7;
    today.setDate(today.getDate() - offset);
    const week = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`;

    this.api.create(week).subscribe({
      next: result => this.router.navigate(['/timesheets', result.id]),
      error: () => { this.error = 'Unable to create the timesheet. Please verify your sign-in and try again.'; this.creating = false; }
    });
  }
}

