import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Project, TimesheetDetail, TimesheetSummary } from './models';

@Injectable({ providedIn: 'root' })
export class TimesheetApiService {
  private readonly baseUrl = '/api/timesheets';

  constructor(private readonly http: HttpClient) {}

  list(): Observable<TimesheetSummary[]> {
    return this.http.get<TimesheetSummary[]>(this.baseUrl);
  }

  get(id: number): Observable<TimesheetDetail> {
    return this.http.get<TimesheetDetail>(`${this.baseUrl}/${id}`);
  }

  create(weekStartDate: string): Observable<{ id: number }> {
    return this.http.post<{ id: number }>(this.baseUrl, { weekStartDate });
  }

  projects(): Observable<Project[]> {
    return this.http.get<Project[]>(`${this.baseUrl}/projects`);
  }

  addEntry(id: number, payload: { projectId: number; workDate: string; hours: number; description: string }): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/entries`, payload);
  }

  submit(id: number): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/submit`, {});
  }

  review(id: number, approve: boolean, comment: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${id}/review`, { approve, comment });
  }
}

