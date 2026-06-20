import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  PowerSwitchRequest,
  AffectedDevice,
  SwitchStep,
  AlarmRecord,
  BusinessImpact,
  DualPowerCheck,
  DeviceTopology
} from '../models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private baseUrl = '/api';

  constructor(private http: HttpClient) {}

  getRequests(): Observable<PowerSwitchRequest[]> {
    return this.http.get<PowerSwitchRequest[]>(`${this.baseUrl}/PowerSwitchRequests`);
  }

  getRequest(id: string): Observable<PowerSwitchRequest> {
    return this.http.get<PowerSwitchRequest>(`${this.baseUrl}/PowerSwitchRequests/${id}`);
  }

  createRequest(data: { title: string; description?: string; riskWindowStart: Date; riskWindowEnd: Date }): Observable<PowerSwitchRequest> {
    return this.http.post<PowerSwitchRequest>(`${this.baseUrl}/PowerSwitchRequests`, data);
  }

  updateRequest(id: string, data: { title: string; description?: string; riskWindowStart: Date; riskWindowEnd: Date }): Observable<PowerSwitchRequest> {
    return this.http.put<PowerSwitchRequest>(`${this.baseUrl}/PowerSwitchRequests/${id}`, data);
  }

  deleteRequest(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/PowerSwitchRequests/${id}`);
  }

  saveAffectedDevices(data: { requestId: string; devices: AffectedDevice[]; dutyManagerName: string }): Observable<PowerSwitchRequest> {
    return this.http.post<PowerSwitchRequest>(`${this.baseUrl}/PowerSwitchRequests/affected-devices`, data);
  }

  saveSwitchSteps(data: { requestId: string; steps: SwitchStep[]; engineerName: string }): Observable<PowerSwitchRequest> {
    return this.http.post<PowerSwitchRequest>(`${this.baseUrl}/PowerSwitchRequests/switch-steps`, data);
  }

  confirmLowPeak(data: { requestId: string; businessOwnerName: string; lowPeakRemark?: string }): Observable<PowerSwitchRequest> {
    return this.http.post<PowerSwitchRequest>(`${this.baseUrl}/PowerSwitchRequests/confirm-lowpeak`, data);
  }

  canExecute(id: string): Observable<boolean> {
    return this.http.get<boolean>(`${this.baseUrl}/PowerSwitchRequests/${id}/can-execute`);
  }

  startExecution(data: { requestId: string; operatorName: string }): Observable<PowerSwitchRequest> {
    return this.http.post<PowerSwitchRequest>(`${this.baseUrl}/PowerSwitchRequests/start-execution`, data);
  }

  completeExecution(data: { requestId: string; operatorName: string }): Observable<PowerSwitchRequest> {
    return this.http.post<PowerSwitchRequest>(`${this.baseUrl}/PowerSwitchRequests/complete-execution`, data);
  }

  rollback(data: { requestId: string; operatorName: string; reason: string }): Observable<PowerSwitchRequest> {
    return this.http.post<PowerSwitchRequest>(`${this.baseUrl}/PowerSwitchRequests/rollback`, data);
  }

  getDeviceTopologyTree(): Observable<DeviceTopology[]> {
    return this.http.get<DeviceTopology[]>(`${this.baseUrl}/DeviceTopology/tree`);
  }

  getDevicesByType(type: number): Observable<DeviceTopology[]> {
    return this.http.get<DeviceTopology[]>(`${this.baseUrl}/DeviceTopology/by-type/${type}`);
  }

  startStep(data: { stepId: string; operatorName: string }): Observable<SwitchStep> {
    return this.http.post<SwitchStep>(`${this.baseUrl}/SwitchSteps/start`, data);
  }

  completeStep(stepId: string, remark?: string): Observable<SwitchStep> {
    return this.http.post<SwitchStep>(`${this.baseUrl}/SwitchSteps/${stepId}/complete`, { remark });
  }

  skipStep(stepId: string, remark?: string): Observable<SwitchStep> {
    return this.http.post<SwitchStep>(`${this.baseUrl}/SwitchSteps/${stepId}/skip`, { remark });
  }

  getAlarms(requestId: string): Observable<AlarmRecord[]> {
    return this.http.get<AlarmRecord[]>(`${this.baseUrl}/Alarms/by-request/${requestId}`);
  }

  createAlarm(requestId: string, data: Partial<AlarmRecord>): Observable<AlarmRecord> {
    return this.http.post<AlarmRecord>(`${this.baseUrl}/Alarms/request/${requestId}`, data);
  }

  confirmAlarm(data: { alarmId: string; confirmedBy: string; confirmRemark?: string }): Observable<AlarmRecord> {
    return this.http.post<AlarmRecord>(`${this.baseUrl}/Alarms/confirm`, data);
  }

  getBusinessImpacts(requestId: string): Observable<BusinessImpact[]> {
    return this.http.get<BusinessImpact[]>(`${this.baseUrl}/BusinessImpacts/by-request/${requestId}`);
  }

  verifyImpact(id: string, verifiedBy: string): Observable<BusinessImpact> {
    return this.http.post<BusinessImpact>(`${this.baseUrl}/BusinessImpacts/${id}/verify`, { verifiedBy });
  }

  getDualPowerChecks(requestId: string): Observable<DualPowerCheck[]> {
    return this.http.get<DualPowerCheck[]>(`${this.baseUrl}/DualPowerChecks/by-request/${requestId}`);
  }

  submitDualPowerCheck(data: { requestId: string; checks: DualPowerCheck[]; checkedBy: string; remark?: string }): Observable<PowerSwitchRequest> {
    return this.http.post<PowerSwitchRequest>(`${this.baseUrl}/DualPowerChecks`, data);
  }
}
