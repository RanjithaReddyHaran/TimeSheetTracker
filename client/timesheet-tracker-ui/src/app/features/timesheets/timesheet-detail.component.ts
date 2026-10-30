import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, Validators } from '@angular/forms';
import { Project, TimesheetDetail } from '../../core/models';
import { TimesheetApiService } from '../../core/timesheet-api.service';

@Component({
  selector: 'app-timesheet-detail',
  template: `
    <div class="detail-heading">
      <div><a routerLink="/" class="back-link">← Back to overview</a><h1>Weekly timesheet</h1><p class="subheading" *ngIf="sheet">Week of {{ sheet.weekStartDate | date:'MMM d, y' }} · {{ sheet.employeeName }}</p></div>
      <span *ngIf="sheet" [ngClass]="'status ' + sheet.status.toLowerCase()">{{ sheet.status }}</span>
    </div>
    <div *ngIf="error" class="inline-error">{{ error }}</div>
    <div *ngIf="message" class="inline-success">{{ message }}</div>
    <div *ngIf="loading" class="loading">Loading timesheet…</div>
    <ng-container *ngIf="sheet">
      <div class="stats-grid detail-stats">
        <article class="stat-card"><div class="stat-top"><span>TOTAL HOURS</span><span class="stat-icon purple">◷</span></div><div class="stat-value">{{ sheet.totalHours | number:'1.2-2' }} <small>hrs</small></div></article>
        <article class="stat-card"><div class="stat-top"><span>TIME ENTRIES</span><span class="stat-icon green">▤</span></div><div class="stat-value">{{ sheet.entries.length }} <small>entries</small></div></article>
        <article class="stat-card"><div class="stat-top"><span>WEEKLY TARGET</span><span class="stat-icon blue">◎</span></div><div class="stat-value">{{ sheet.totalHours / 40 * 100 | number:'1.0-0' }}<small>%</small></div></article>
      </div>
      <section class="panel detail-panel">
        <div class="panel-heading"><div><h2>Time entries</h2><p>Daily work recorded against projects</p></div></div>
        <div class="table-wrap"><table><thead><tr><th>WORK DATE</th><th>PROJECT</th><th>DESCRIPTION</th><th>HOURS</th></tr></thead>
          <tbody><tr *ngFor="let entry of sheet.entries"><td>{{ entry.workDate | date:'EEE, MMM d' }}</td><td><b>{{ entry.projectCode }}</b><small>{{ entry.projectName }}</small></td><td>{{ entry.description }}</td><td><b>{{ entry.hours | number:'1.2-2' }}</b></td></tr>
          <tr *ngIf="sheet.entries.length === 0"><td colspan="4" class="empty">No time entries have been added.</td></tr></tbody>
        </table></div>
      </section>
      <section class="panel form-panel" *ngIf="sheet.status === 'Draft' || sheet.status === 'Rejected'">
        <div class="panel-heading"><div><h2>Add a time entry</h2><p>Record time spent on an assigned project</p></div></div>
        <form [formGroup]="entryForm" (ngSubmit)="addEntry()" class="entry-form">
          <div class="form-field"><label>Work date</label><input type="date" formControlName="workDate" [min]="sheet.weekStartDate" [max]="weekEndDate"></div>
          <div class="form-field"><label>Project</label><select formControlName="projectId"><option value="">Select a project</option><option *ngFor="let project of projects" [value]="project.id">{{ project.projectCode }} — {{ project.name }}</option></select></div>
          <div class="form-field"><label>Hours</label><input type="number" min="0.25" max="24" step="0.25" formControlName="hours"></div>
          <div class="form-field description-field"><label>Task description</label><input type="text" maxlength="1000" formControlName="description" placeholder="What did you work on?"></div>
          <button class="primary-button" type="submit" [disabled]="entryForm.invalid || saving">Add entry</button>
        </form>
        <button class="primary-button submit-button" (click)="submitTimesheet()" [disabled]="saving || sheet.entries.length === 0">Submit for approval →</button>
      </section>
      <section class="panel form-panel" *ngIf="sheet.status === 'Submitted'">
        <div class="panel-heading"><div><h2>Manager review</h2><p>Review and approve this employee's timesheet</p></div></div>
        <form [formGroup]="reviewForm" (ngSubmit)="review(false)" class="review-form">
          <div class="form-field"><label>Review comment (required when rejecting)</label><textarea rows="3" formControlName="comment"></textarea></div>
          <button class="primary-button" type="button" (click)="review(true)" [disabled]="saving">Approve timesheet</button>
          <button class="secondary-button" type="submit" [disabled]="saving || !reviewForm.value.comment?.trim()">Reject</button>
        </form>
      </section>
      <div *ngIf="sheet.reviewComment" class="inline-info"><b>Manager feedback:</b> {{ sheet.reviewComment }}</div>
    </ng-container>
  `
})
export class TimesheetDetailComponent implements OnInit {
  sheet: TimesheetDetail | null = null;
  projects: Project[] = [];
  loading = true;
  saving = false;
  error = '';
  message = '';
  id = 0;

  entryForm = this.fb.group({
    workDate: ['', Validators.required],
    projectId: ['', Validators.required],
    hours: [8, [Validators.required, Validators.min(0.25), Validators.max(24)]],
    description: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(1000)]]
  });

  reviewForm = this.fb.group({ comment: [''] });

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly fb: FormBuilder,
    private readonly api: TimesheetApiService
  ) {}

  get weekEndDate(): string {
    if (!this.sheet) return '';
    const date = new Date(`${this.sheet.weekStartDate}T00:00:00`);
    date.setDate(date.getDate() + 6);
    return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`;
  }

  ngOnInit(): void {
    this.id = Number(this.route.snapshot.paramMap.get('id'));
    if (!Number.isInteger(this.id) || this.id <= 0) {
      this.error = 'Invalid timesheet identifier.';
      this.loading = false;
      return;
    }
    this.load();
    this.api.projects().subscribe({ next: rows => this.projects = rows });
  }

  load(): void {
    this.api.get(this.id).subscribe({
      next: value => { this.sheet = value; this.loading = false; },
      error: () => { this.error = 'Unable to load this timesheet. You may not have access.'; this.loading = false; }
    });
  }

  addEntry(): void {
    if (this.entryForm.invalid || this.saving) return;
    const value = this.entryForm.getRawValue();
    this.saving = true;
    this.api.addEntry(this.id, {
      projectId: Number(value.projectId),
      workDate: value.workDate || '',
      hours: Number(value.hours),
      description: value.description || ''
    }).subscribe({
      next: () => { this.message = 'Time entry added successfully.'; this.error = ''; this.saving = false; this.entryForm.patchValue({ description: '' }); this.load(); },
      error: response => { this.error = response.error?.detail || response.error?.message || 'Unable to add the time entry.'; this.saving = false; }
    });
  }

  submitTimesheet(): void {
    if (this.saving) return;
    this.saving = true;
    this.api.submit(this.id).subscribe({
      next: () => { this.message = 'Timesheet submitted for approval.'; this.saving = false; this.load(); },
      error: response => { this.error = response.error?.detail || 'Unable to submit timesheet.'; this.saving = false; }
    });
  }

  review(approve: boolean): void {
    if (this.saving) return;
    const comment = (this.reviewForm.value.comment || '').trim();
    if (!approve && !comment) { this.error = 'A comment is required when rejecting.'; return; }
    this.saving = true;
    this.api.review(this.id, approve, comment).subscribe({
      next: () => { this.message = approve ? 'Timesheet approved.' : 'Timesheet rejected.'; this.saving = false; this.load(); },
      error: response => { this.error = response.error?.detail || 'Unable to review timesheet.'; this.saving = false; }
    });
  }
}

