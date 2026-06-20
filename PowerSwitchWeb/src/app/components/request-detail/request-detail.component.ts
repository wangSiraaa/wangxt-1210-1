import { Component, OnInit, ViewChild, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators, FormControl } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatTabsModule, MatTabGroup } from '@angular/material/tabs';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatStepperModule } from '@angular/material/stepper';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ApiService } from '../../services/api.service';
import {
  PowerSwitchRequest,
  AffectedDevice,
  SwitchStep,
  AlarmRecord,
  BusinessImpact,
  DualPowerCheck,
  DeviceTopology,
  RequestStatus,
  StatusTextMap,
  StatusClassMap,
  StepStatusText,
  StepStatusClass,
  AlarmSeverityText,
  AlarmClassMap,
  DeviceTypeText
} from '../../models';

@Component({
  selector: 'app-request-detail',
  standalone: true,
  imports: [
    CommonModule, RouterLink, FormsModule, ReactiveFormsModule,
    MatButtonModule, MatCardModule, MatTabsModule, MatIconModule,
    MatFormFieldModule, MatInputModule, MatSelectModule, MatCheckboxModule,
    MatDatepickerModule, MatTableModule, MatChipsModule, MatDividerModule,
    MatTooltipModule, MatSnackBarModule, MatDialogModule, MatStepperModule,
    MatProgressSpinnerModule
  ],
  template: `
    <ng-container *ngIf="request; else loading">
      <div class="page-header">
        <div>
          <button mat-icon-button routerLink="/requests">
            <mat-icon>arrow_back</mat-icon>
          </button>
          <span style="font-size:20px;font-weight:500;vertical-align:middle;">
            {{request.requestNo}} - {{request.title}}
          </span>
        </div>
        <span class="status-badge" [ngClass]="getStatusClass(request.status)">{{getStatusText(request.status)}}</span>
      </div>

      <mat-card class="section-card">
        <mat-card-title style="font-size:16px;">
          <mat-icon style="vertical-align:middle;color:#f44336;">warning</mat-icon>
          共享风险窗口
        </mat-card-title>
        <mat-divider style="margin:8px 0 16px;"></mat-divider>
        <div class="grid-3">
          <div>
            <div class="muted">开始时间</div>
            <div style="font-size:18px;font-weight:500;">{{formatDate(request.riskWindowStart)}}</div>
          </div>
          <div>
            <div class="muted">结束时间</div>
            <div style="font-size:18px;font-weight:500;">{{formatDate(request.riskWindowEnd)}}</div>
          </div>
          <div>
            <div class="muted">状态</div>
            <div class="chip-list" style="margin-top:4px;">
              <mat-chip [disabled]="!request.dutyManagerName" [color]="request.dutyManagerName ? 'primary' : undefined" selected>
                <mat-icon>person</mat-icon>
                值班经理：{{request.dutyManagerName || '未登记'}}
              </mat-chip>
              <mat-chip [disabled]="!request.engineerName" [color]="request.engineerName ? 'accent' : undefined" selected>
                <mat-icon>build</mat-icon>
                工程师：{{request.engineerName || '未录入'}}
              </mat-chip>
              <mat-chip [disabled]="!request.businessOwnerName" [color]="request.businessOwnerName ? 'warn' : undefined" selected>
                <mat-icon>business</mat-icon>
                业务负责人：{{request.businessOwnerName || '未确认'}}
              </mat-chip>
            </div>
          </div>
        </div>
        <div *ngIf="request.description" style="margin-top:12px;">
          <div class="muted">描述</div>
          <div>{{request.description}}</div>
        </div>
      </mat-card>

      <mat-tab-group #tabs (selectedTabChange)="onTabChange($event)">
        <mat-tab label="1. 影响范围登记">
          <div class="form-panel">
            <div class="flex-between" style="margin-bottom:16px;">
              <h3 style="margin:0;">值班经理登记 UPS / 母线 / 机柜影响范围</h3>
              <button mat-raised-button color="primary" (click)="addDeviceRow()" [disabled]="request.status >= RequestStatus.EngineerFilled">
                <mat-icon>add</mat-icon> 添加设备
              </button>
            </div>

            <form [formGroup]="dutyForm" (ngSubmit)="saveAffectedDevices()">
              <div class="grid-2" style="margin-bottom:16px;">
                <mat-form-field class="full-width">
                  <mat-label>值班经理姓名</mat-label>
                  <input matInput formControlName="dutyManagerName" [readonly]="request.status >= RequestStatus.EngineerFilled">
                </mat-form-field>
              </div>

              <div formArrayName="devices">
                <table class="full-width" style="border-collapse: collapse;">
                  <thead>
                    <tr style="background:#f5f5f5;">
                      <th style="padding:8px;text-align:left;width:18%;">设备类型</th>
                      <th style="padding:8px;text-align:left;width:22%;">设备编码</th>
                      <th style="padding:8px;text-align:left;width:22%;">设备名称</th>
                      <th style="padding:8px;text-align:left;width:25%;">位置</th>
                      <th style="padding:8px;text-align:left;width:10%;">操作</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr *ngFor="let fg of devicesArray.controls; let i=index" [formGroupName]="i" style="border-bottom:1px solid #eee;">
                      <td style="padding:8px;">
                        <mat-form-field class="full-width" style="font-size:12px;">
                          <mat-select formControlName="deviceType" [disabled]="request.status >= RequestStatus.EngineerFilled">
                            <mat-option [value]="1">UPS</mat-option>
                            <mat-option [value]="2">母线</mat-option>
                            <mat-option [value]="3">机柜</mat-option>
                            <mat-option [value]="4">ATS</mat-option>
                            <mat-option [value]="5">PDU</mat-option>
                          </mat-select>
                        </mat-form-field>
                      </td>
                      <td style="padding:8px;">
                        <mat-form-field class="full-width" style="font-size:12px;">
                          <mat-select formControlName="deviceCode" [disabled]="request.status >= RequestStatus.EngineerFilled" (selectionChange)="onDeviceSelect(i)">
                            <mat-option *ngFor="let dev of deviceOptions" [value]="dev.deviceCode">{{dev.deviceCode}}</mat-option>
                          </mat-select>
                        </mat-form-field>
                      </td>
                      <td style="padding:8px;">
                        <mat-form-field class="full-width" style="font-size:12px;">
                          <input matInput formControlName="deviceName" [readonly]="request.status >= RequestStatus.EngineerFilled">
                        </mat-form-field>
                      </td>
                      <td style="padding:8px;">
                        <mat-form-field class="full-width" style="font-size:12px;">
                          <input matInput formControlName="location" [readonly]="request.status >= RequestStatus.EngineerFilled">
                        </mat-form-field>
                      </td>
                      <td style="padding:8px;">
                        <button mat-icon-button color="warn" (click)="removeDeviceRow(i)" [disabled]="request.status >= RequestStatus.EngineerFilled" *ngIf="devicesArray.length > 0">
                          <mat-icon>close</mat-icon>
                        </button>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>

              <div class="action-bar" style="margin-top:16px;">
                <button mat-raised-button color="primary" type="submit" [disabled]="dutyForm.invalid || request.status >= RequestStatus.EngineerFilled">
                  <mat-icon>save</mat-icon> 保存影响范围
                </button>
                <span class="muted" *ngIf="request.status >= RequestStatus.EngineerFilled">工程师已录入步骤，影响范围锁定</span>
              </div>
            </form>
          </div>
        </mat-tab>

        <mat-tab label="2. 切换步骤录入">
          <div class="form-panel">
            <div class="flex-between" style="margin-bottom:16px;">
              <h3 style="margin:0;">强电工程师录入切换步骤</h3>
              <button mat-raised-button color="accent" (click)="addStepRow()" [disabled]="request.status >= RequestStatus.BusinessConfirmed">
                <mat-icon>add</mat-icon> 添加步骤
              </button>
            </div>

            <form [formGroup]="stepForm" (ngSubmit)="saveSwitchSteps()">
              <div class="grid-2" style="margin-bottom:16px;">
                <mat-form-field class="full-width">
                  <mat-label>强电工程师姓名</mat-label>
                  <input matInput formControlName="engineerName" [readonly]="request.status >= RequestStatus.BusinessConfirmed">
                </mat-form-field>
              </div>

              <div formArrayName="steps">
                <div *ngFor="let fg of stepsArray.controls; let i=index" [formGroupName]="i"
                     class="timeline-step"
                     [ngClass]="{'active': request.status >= RequestStatus.Executing && getStepStatus(i) === 1, 'done': getStepStatus(i) === 2}">
                  <div class="flex-between" style="margin-bottom:8px;">
                    <strong>步骤 {{i + 1}}</strong>
                    <span [ngClass]="StepStatusClass[getStepStatus(i)]">{{StepStatusText[getStepStatus(i)] || '待执行'}}</span>
                  </div>
                  <div class="grid-3">
                    <mat-form-field class="full-width" style="font-size:12px;">
                      <mat-label>步骤类型</mat-label>
                      <mat-select formControlName="stepType" [disabled]="request.status >= RequestStatus.BusinessConfirmed">
                        <mat-option value="电源切换">电源切换</mat-option>
                        <mat-option value="负载转移">负载转移</mat-option>
                        <mat-option value="设备操作">设备操作</mat-option>
                        <mat-option value="状态确认">状态确认</mat-option>
                        <mat-option value="回退步骤">回退步骤</mat-option>
                      </mat-select>
                    </mat-form-field>
                    <mat-form-field class="full-width" style="font-size:12px;">
                      <mat-label>预计时长（秒）</mat-label>
                      <input matInput type="number" formControlName="estimatedDurationSeconds" [readonly]="request.status >= RequestStatus.BusinessConfirmed">
                    </mat-form-field>
                    <mat-form-field class="full-width" style="font-size:12px;">
                      <mat-label>是否回退步骤</mat-label>
                      <mat-select formControlName="isRollbackStep" [disabled]="request.status >= RequestStatus.BusinessConfirmed">
                        <mat-option [value]="false">否</mat-option>
                        <mat-option [value]="true">是</mat-option>
                      </mat-select>
                    </mat-form-field>
                  </div>
                  <mat-form-field class="full-width" style="font-size:12px;">
                    <mat-label>步骤描述</mat-label>
                    <input matInput formControlName="description" [readonly]="request.status >= RequestStatus.BusinessConfirmed">
                  </mat-form-field>
                  <mat-form-field class="full-width" style="font-size:12px;">
                    <mat-label>操作细节</mat-label>
                    <textarea matInput rows="2" formControlName="operationDetail" [readonly]="request.status >= RequestStatus.BusinessConfirmed"></textarea>
                  </mat-form-field>
                  <div class="flex-between" *ngIf="request.status < RequestStatus.BusinessConfirmed">
                    <span></span>
                    <button mat-icon-button color="warn" (click)="removeStepRow(i)" [disabled]="stepsArray.length <= 0">
                      <mat-icon>delete</mat-icon>
                    </button>
                  </div>
                  <div *ngIf="request.status >= RequestStatus.Executing" class="action-bar">
                    <button *ngIf="getStepStatus(i) === 0" mat-raised-button color="primary" (click)="startStep(i)">
                      <mat-icon>play_arrow</mat-icon> 开始执行
                    </button>
                    <button *ngIf="getStepStatus(i) === 1" mat-raised-button color="accent" (click)="completeStep(i)">
                      <mat-icon>check</mat-icon> 完成
                    </button>
                    <button *ngIf="getStepStatus(i) === 1" mat-button (click)="skipStep(i)">跳过</button>
                  </div>
                </div>
              </div>

              <div class="action-bar" style="margin-top:16px;">
                <button mat-raised-button color="accent" type="submit" [disabled]="stepForm.invalid || request.status >= RequestStatus.BusinessConfirmed">
                  <mat-icon>save</mat-icon> 保存切换步骤
                </button>
                <span class="muted" *ngIf="request.status >= RequestStatus.BusinessConfirmed">业务负责人已确认，步骤锁定</span>
              </div>
            </form>
          </div>
        </mat-tab>

        <mat-tab label="3. 低峰时段确认">
          <div class="form-panel">
            <h3 style="margin-top:0;">业务负责人确认低峰时段</h3>
            <form [formGroup]="businessForm" (ngSubmit)="confirmLowPeak()">
              <div class="grid-2">
                <mat-form-field class="full-width">
                  <mat-label>业务负责人姓名</mat-label>
                  <input matInput formControlName="businessOwnerName" [readonly]="request.isLowPeakConfirmed">
                </mat-form-field>
              </div>
              <mat-form-field class="full-width">
                <mat-label>低峰时段确认备注</mat-label>
                <textarea matInput rows="3" formControlName="lowPeakRemark" [readonly]="request.isLowPeakConfirmed"
                  placeholder="确认本次切换在业务低峰时段，预计影响最小化"></textarea>
              </mat-form-field>
              <div class="action-bar">
                <button mat-raised-button color="warn" type="submit"
                        [disabled]="businessForm.invalid || request.isLowPeakConfirmed">
                  <mat-icon>done_all</mat-icon> 确认低峰时段
                </button>
                <span *ngIf="request.isLowPeakConfirmed" style="color:#2e7d32;">
                  <mat-icon>check_circle</mat-icon> 已于 {{formatDate(request.businessConfirmedAt)}} 由 {{request.businessOwnerName}} 确认
                </span>
              </div>
            </form>
          </div>
        </mat-tab>

        <mat-tab label="4. 双路供电校验">
          <div class="form-panel">
            <div class="flex-between" style="margin-bottom:16px;">
              <h3 style="margin:0;">双路供电校验（未通过禁止进入执行）</h3>
              <button mat-raised-button (click)="generateChecks()" *ngIf="checksArray.length === 0" [disabled]="request.status >= RequestStatus.ReadyForExecution">
                <mat-icon>refresh</mat-icon> 从影响范围生成
              </button>
            </div>

            <div *ngIf="request.dualPowerCheckPassed" style="margin-bottom:12px;color:#2e7d32;">
              <mat-icon>check_circle</mat-icon> 双路供电已校验通过
            </div>

            <form [formGroup]="checkForm" (ngSubmit)="submitDualPowerCheck()">
              <div formArrayName="checks">
                <table class="full-width" style="border-collapse: collapse;">
                  <thead>
                    <tr style="background:#f5f5f5;">
                      <th style="padding:8px;text-align:left;">设备</th>
                      <th style="padding:8px;text-align:center;">A路供电</th>
                      <th style="padding:8px;text-align:center;">B路供电</th>
                      <th style="padding:8px;text-align:center;">双路健康</th>
                      <th style="padding:8px;text-align:left;">备注</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr *ngFor="let fg of checksArray.controls; let i=index" [formGroupName]="i" style="border-bottom:1px solid #eee;">
                      <td style="padding:8px;">
                        <strong>{{fg.value.deviceCode}}</strong>
                        <div class="muted">{{DeviceTypeText[fg.value.deviceType]}}</div>
                      </td>
                      <td style="padding:8px;text-align:center;">
                        <mat-checkbox formControlName="routeAPowered" [disabled]="request.status >= RequestStatus.ReadyForExecution" color="primary"></mat-checkbox>
                      </td>
                      <td style="padding:8px;text-align:center;">
                        <mat-checkbox formControlName="routeBPowered" [disabled]="request.status >= RequestStatus.ReadyForExecution" color="primary"></mat-checkbox>
                      </td>
                      <td style="padding:8px;text-align:center;">
                        <span [style.color]="(fg.value.routeAPowered && fg.value.routeBPowered) ? '#2e7d32' : '#c62828'">
                          <mat-icon>{{fg.value.routeAPowered && fg.value.routeBPowered ? 'check_circle' : 'cancel'}}</mat-icon>
                        </span>
                      </td>
                      <td style="padding:8px;">
                        <mat-form-field class="full-width" style="font-size:12px;">
                          <input matInput formControlName="checkRemark" [readonly]="request.status >= RequestStatus.ReadyForExecution">
                        </mat-form-field>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>

              <div class="grid-2" style="margin-top:16px;">
                <mat-form-field class="full-width">
                  <mat-label>校验人</mat-label>
                  <input matInput formControlName="checkedBy" [readonly]="request.status >= RequestStatus.ReadyForExecution">
                </mat-form-field>
                <mat-form-field class="full-width">
                  <mat-label>整体备注</mat-label>
                  <input matInput formControlName="remark" [readonly]="request.status >= RequestStatus.ReadyForExecution">
                </mat-form-field>
              </div>

              <div class="action-bar" style="margin-top:16px;">
                <button mat-raised-button type="submit" [disabled]="checkForm.invalid || request.status >= RequestStatus.ReadyForExecution">
                  <mat-icon>verified_user</mat-icon> 提交校验结果
                </button>
              </div>
            </form>
          </div>
        </mat-tab>

        <mat-tab label="5. 执行与告警">
          <div class="form-panel">
            <div class="flex-between" style="margin-bottom:16px;">
              <h3 style="margin:0;">切换执行</h3>
              <div class="action-bar">
                <button *ngIf="request.status < RequestStatus.Executing"
                        mat-raised-button color="primary" (click)="startExecution()">
                  <mat-icon>play_circle_filled</mat-icon> 开始执行
                </button>
                <button *ngIf="request.status === RequestStatus.Executing || request.status === RequestStatus.AlarmsPending"
                        mat-raised-button color="warn" (click)="openRollbackDialog()">
                  <mat-icon>undo</mat-icon> 回退
                </button>
                <button *ngIf="(request.status === RequestStatus.Executing || request.status === RequestStatus.AlarmsPending) && !hasUnconfirmedCritical"
                        mat-raised-button color="accent" (click)="completeExecution()">
                  <mat-icon>check_circle</mat-icon> 完成切换
                </button>
                <button mat-button (click)="openAlarmDialog()">
                  <mat-icon>add_alert</mat-icon> 登记告警
                </button>
              </div>
            </div>

            <div *ngIf="hasUnconfirmedCritical" style="padding:12px;background:#ffcdd2;border-radius:4px;margin-bottom:16px;color:#c62828;">
              <mat-icon>warning</mat-icon> 存在未确认的告警，无法结束切换，请先确认告警
            </div>

            <mat-card style="margin-bottom:16px;">
              <mat-card-title style="font-size:14px;">告警记录</mat-card-title>
              <table *ngIf="request.alarmRecords?.length" class="full-width" style="border-collapse: collapse;">
                <thead>
                  <tr style="background:#f5f5f5;">
                    <th style="padding:8px;text-align:left;">级别</th>
                    <th style="padding:8px;text-align:left;">告警消息</th>
                    <th style="padding:8px;text-align:left;">来源设备</th>
                    <th style="padding:8px;text-align:left;">告警时间</th>
                    <th style="padding:8px;text-align:left;">确认状态</th>
                    <th style="padding:8px;text-align:left;">操作</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let a of request.alarmRecords" style="border-bottom:1px solid #eee;">
                    <td style="padding:8px;">
                      <span class="status-badge" [ngClass]="AlarmClassMap[a.severity]">{{AlarmSeverityText[a.severity]}}</span>
                    </td>
                    <td style="padding:8px;">{{a.alarmMessage}}</td>
                    <td style="padding:8px;">{{a.sourceDevice || '-'}}</td>
                    <td style="padding:8px;">{{formatDate(a.alarmTime)}}</td>
                    <td style="padding:8px;">
                      <span *ngIf="a.isConfirmed" style="color:#2e7d32;">
                        <mat-icon>check_circle</mat-icon> {{a.confirmedBy}} @ {{formatDate(a.confirmedAt)}}
                      </span>
                      <span *ngIf="!a.isConfirmed" style="color:#c62828;">
                        <mat-icon>error</mat-icon> 待确认
                      </span>
                    </td>
                    <td style="padding:8px;">
                      <button *ngIf="!a.isConfirmed" mat-button color="primary" (click)="openConfirmAlarmDialog(a)">
                        确认
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
              <div *ngIf="!request.alarmRecords?.length" class="muted" style="padding:16px;">暂无告警</div>
            </mat-card>

            <mat-card>
              <mat-card-title style="font-size:14px;">执行日志</mat-card-title>
              <div *ngIf="request.executionStartedAt" class="muted" style="padding:8px;">
                开始时间：{{formatDate(request.executionStartedAt)}}
              </div>
              <div *ngIf="request.executionCompletedAt" class="muted" style="padding:8px;">
                完成时间：{{formatDate(request.executionCompletedAt)}}
              </div>
              <div *ngIf="request.rolledBackAt" style="padding:8px;color:#c62828;">
                <mat-icon>undo</mat-icon> 已回退：{{request.rollbackReason}}
              </div>
            </mat-card>
          </div>
        </mat-tab>

        <mat-tab label="6. 影响业务清单">
          <div class="form-panel">
            <h3 style="margin-top:0;">恢复后自动生成的影响业务清单</h3>
            <div *ngIf="request.status < RequestStatus.Completed" class="muted" style="padding:16px;">
              切换完成后将根据机柜拓扑自动生成影响业务清单
            </div>
            <table *ngIf="request.businessImpacts?.length" class="full-width" style="border-collapse: collapse;">
              <thead>
                <tr style="background:#f5f5f5;">
                  <th style="padding:8px;text-align:left;">业务系统</th>
                  <th style="padding:8px;text-align:left;">受影响机柜数</th>
                  <th style="padding:8px;text-align:left;">实际影响开始</th>
                  <th style="padding:8px;text-align:left;">实际影响结束</th>
                  <th style="padding:8px;text-align:left;">影响描述</th>
                  <th style="padding:8px;text-align:left;">恢复详情</th>
                  <th style="padding:8px;text-align:left;">验证</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let bi of request.businessImpacts" style="border-bottom:1px solid #eee;">
                  <td style="padding:8px;">
                    <strong>{{bi.businessSystemName}}</strong>
                  </td>
                  <td style="padding:8px;">{{bi.affectedCabinetCount}}</td>
                  <td style="padding:8px;">{{formatDate(bi.actualImpactStart)}}</td>
                  <td style="padding:8px;">{{formatDate(bi.actualImpactEnd)}}</td>
                  <td style="padding:8px;">{{bi.impactDescription}}</td>
                  <td style="padding:8px;">{{bi.recoveryDetail}}</td>
                  <td style="padding:8px;">
                    <div *ngIf="bi.verifiedBy">
                      <mat-icon style="color:#2e7d32;vertical-align:middle;">check_circle</mat-icon>
                      {{bi.verifiedBy}} @ {{formatDate(bi.verifiedAt)}}
                    </div>
                    <button *ngIf="!bi.verifiedBy" mat-button color="primary" (click)="verifyImpact(bi)">
                      验证
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </mat-tab>
      </mat-tab-group>
    </ng-container>

    <ng-template #loading>
      <div style="text-align:center;padding:64px;">
        <mat-spinner diameter="32"></mat-spinner>
        <p class="muted" style="margin-top:16px;">加载中...</p>
      </div>
    </ng-template>
  `
})
export class RequestDetailComponent implements OnInit {
  request!: PowerSwitchRequest | null;
  requestId!: string;

  RequestStatus = RequestStatus;
  StatusTextMap = StatusTextMap;
  StatusClassMap = StatusClassMap;
  StepStatusText = StepStatusText;
  StepStatusClass = StepStatusClass;
  AlarmSeverityText = AlarmSeverityText;
  AlarmClassMap = AlarmClassMap;
  DeviceTypeText = DeviceTypeText;

  dutyForm!: FormGroup;
  stepForm!: FormGroup;
  businessForm!: FormGroup;
  checkForm!: FormGroup;

  deviceOptions: DeviceTopology[] = [];

  @ViewChild('tabs') tabs!: MatTabGroup;

  constructor(
    private route: ActivatedRoute,
    private api: ApiService,
    private fb: FormBuilder,
    private snack: MatSnackBar,
    private dialog: MatDialog,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.requestId = this.route.snapshot.paramMap.get('id')!;
    this.initForms();
    this.loadDeviceOptions();
    this.loadDetail();
  }

  initForms(): void {
    this.dutyForm = this.fb.group({
      dutyManagerName: ['', Validators.required],
      devices: this.fb.array([])
    });
    this.stepForm = this.fb.group({
      engineerName: ['', Validators.required],
      steps: this.fb.array([])
    });
    this.businessForm = this.fb.group({
      businessOwnerName: ['', Validators.required],
      lowPeakRemark: ['']
    });
    this.checkForm = this.fb.group({
      checkedBy: ['', Validators.required],
      remark: [''],
      checks: this.fb.array([])
    });
  }

  get devicesArray(): FormArray {
    return this.dutyForm.get('devices') as FormArray;
  }

  get stepsArray(): FormArray {
    return this.stepForm.get('steps') as FormArray;
  }

  get checksArray(): FormArray {
    return this.checkForm.get('checks') as FormArray;
  }

  get hasUnconfirmedCritical(): boolean {
    if (!this.request?.alarmRecords) return false;
    return this.request.alarmRecords.some(a => !a.isConfirmed && a.severity >= 2);
  }

  loadDeviceOptions(): void {
    this.api.getDeviceTopologyTree().subscribe({
      next: (data) => {
        this.deviceOptions = this.flattenTree(data);
      }
    });
  }

  private flattenTree(nodes: DeviceTopology[]): DeviceTopology[] {
    let result: DeviceTopology[] = [];
    nodes.forEach(n => {
      result.push(n);
      if (n.children?.length) {
        result = result.concat(this.flattenTree(n.children));
      }
    });
    return result;
  }

  loadDetail(): void {
    this.api.getRequest(this.requestId).subscribe({
      next: (data) => {
        this.request = data;
        this.populateForms(data);
        this.setTabByStatus(data.status);
      },
      error: () => this.snack.open('加载失败', '关闭', { duration: 3000 })
    });
  }

  setTabByStatus(status: number): void {
    if (!this.tabs) return;
    let idx = 0;
    if (status >= RequestStatus.DutyManagerFilled) idx = Math.max(idx, 1);
    if (status >= RequestStatus.EngineerFilled) idx = Math.max(idx, 2);
    if (status >= RequestStatus.BusinessConfirmed) idx = Math.max(idx, 3);
    if (status >= RequestStatus.DualPowerChecked) idx = Math.max(idx, 4);
    if (status >= RequestStatus.ReadyForExecution) idx = Math.max(idx, 5);
    setTimeout(() => { if (this.tabs) this.tabs.selectedIndex = idx; }, 100);
  }

  populateForms(data: PowerSwitchRequest): void {
    this.dutyForm.patchValue({ dutyManagerName: data.dutyManagerName || '' });
    while (this.devicesArray.length) this.devicesArray.removeAt(0);
    (data.affectedDevices || []).forEach(d => this.devicesArray.push(this.fb.group({
      id: [d.id],
      deviceType: [d.deviceType, Validators.required],
      deviceCode: [d.deviceCode, Validators.required],
      deviceName: [d.deviceName, Validators.required],
      location: [d.location || ''],
      impactDescription: [d.impactDescription || '']
    })));

    this.stepForm.patchValue({ engineerName: data.engineerName || '' });
    while (this.stepsArray.length) this.stepsArray.removeAt(0);
    (data.switchSteps || []).forEach(s => this.stepsArray.push(this.fb.group({
      id: [s.id],
      sequence: [s.sequence],
      stepType: [s.stepType, Validators.required],
      description: [s.description, Validators.required],
      operationDetail: [s.operationDetail || ''],
      estimatedDurationSeconds: [s.estimatedDurationSeconds || 60],
      status: [s.status],
      isRollbackStep: [s.isRollbackStep || false]
    })));

    this.businessForm.patchValue({
      businessOwnerName: data.businessOwnerName || '',
      lowPeakRemark: data.lowPeakRemark || ''
    });

    this.checkForm.patchValue({
      checkedBy: '',
      remark: data.dualPowerCheckRemark || ''
    });
  }

  onTabChange(event: any): void {}

  getStatusText(status: number): string {
    return StatusTextMap[status] || '未知';
  }

  getStatusClass(status: number): string {
    return StatusClassMap[status] || 'status-draft';
  }

  getStepStatus(i: number): number {
    const s = this.stepsArray.at(i);
    return s?.get('status')?.value || 0;
  }

  formatDate(d: any): string {
    if (!d) return '-';
    return new Date(d).toLocaleString('zh-CN');
  }

  addDeviceRow(): void {
    this.devicesArray.push(this.fb.group({
      id: [''],
      deviceType: [3, Validators.required],
      deviceCode: ['', Validators.required],
      deviceName: ['', Validators.required],
      location: [''],
      impactDescription: ['']
    }));
  }

  removeDeviceRow(i: number): void {
    this.devicesArray.removeAt(i);
  }

  onDeviceSelect(i: number): void {
    const code = this.devicesArray.at(i).get('deviceCode')?.value;
    const dev = this.deviceOptions.find(d => d.deviceCode === code);
    if (dev) {
      this.devicesArray.at(i).patchValue({
        deviceType: dev.deviceType,
        deviceName: dev.deviceName,
        location: dev.location || ''
      });
    }
  }

  saveAffectedDevices(): void {
    if (this.dutyForm.invalid || !this.request) return;
    const v = this.dutyForm.value;
    const devices: AffectedDevice[] = v.devices.map((d: any) => ({
      id: d.id || '',
      deviceType: d.deviceType,
      deviceCode: d.deviceCode,
      deviceName: d.deviceName,
      location: d.location,
      impactDescription: d.impactDescription
    }));
    this.api.saveAffectedDevices({ requestId: this.request.id, devices, dutyManagerName: v.dutyManagerName })
      .subscribe({
        next: (r) => {
          this.request = r;
          this.snack.open('影响范围已保存', '关闭', { duration: 2000 });
          this.loadDetail();
        },
        error: () => this.snack.open('保存失败', '关闭', { duration: 3000 })
      });
  }

  addStepRow(): void {
    this.stepsArray.push(this.fb.group({
      id: [''],
      sequence: [this.stepsArray.length + 1],
      stepType: ['电源切换', Validators.required],
      description: ['', Validators.required],
      operationDetail: [''],
      estimatedDurationSeconds: [60],
      status: [0],
      isRollbackStep: [false]
    }));
  }

  removeStepRow(i: number): void {
    this.stepsArray.removeAt(i);
    this.stepsArray.controls.forEach((c, idx) => c.patchValue({ sequence: idx + 1 }));
  }

  saveSwitchSteps(): void {
    if (this.stepForm.invalid || !this.request) return;
    const v = this.stepForm.value;
    const steps: SwitchStep[] = v.steps.map((s: any, i: number) => ({
      id: s.id || '',
      sequence: i + 1,
      stepType: s.stepType,
      description: s.description,
      operationDetail: s.operationDetail,
      estimatedDurationSeconds: s.estimatedDurationSeconds,
      status: s.status,
      statusText: '',
      isRollbackStep: s.isRollbackStep
    }));
    this.api.saveSwitchSteps({ requestId: this.request.id, steps, engineerName: v.engineerName })
      .subscribe({
        next: (r) => {
          this.request = r;
          this.snack.open('切换步骤已保存', '关闭', { duration: 2000 });
          this.loadDetail();
        },
        error: () => this.snack.open('保存失败', '关闭', { duration: 3000 })
      });
  }

  confirmLowPeak(): void {
    if (this.businessForm.invalid || !this.request) return;
    const v = this.businessForm.value;
    this.api.confirmLowPeak({ requestId: this.request.id, businessOwnerName: v.businessOwnerName, lowPeakRemark: v.lowPeakRemark })
      .subscribe({
        next: (r) => {
          this.request = r;
          this.snack.open('低峰时段已确认', '关闭', { duration: 2000 });
          this.loadDetail();
        },
        error: () => this.snack.open('确认失败', '关闭', { duration: 3000 })
      });
  }

  generateChecks(): void {
    if (!this.request) return;
    const devices = this.request.affectedDevices || [];
    while (this.checksArray.length) this.checksArray.removeAt(0);
    devices.forEach(d => {
      this.checksArray.push(this.fb.group({
        id: [''],
        deviceCode: [d.deviceCode],
        deviceType: [d.deviceType],
        routeAPowered: [true],
        routeBPowered: [true],
        bothRoutesHealthy: [true],
        checkRemark: [''],
        checkedBy: [''],
        checkedAt: [new Date()]
      }));
    });
    if (this.checksArray.length === 0) {
      this.snack.open('请先在"影响范围登记"中添加设备', '关闭', { duration: 3000 });
    }
  }

  submitDualPowerCheck(): void {
    if (this.checkForm.invalid || !this.request) return;
    const v = this.checkForm.value;
    const checks: DualPowerCheck[] = v.checks.map((c: any) => ({
      id: c.id,
      deviceCode: c.deviceCode,
      deviceType: c.deviceType,
      routeAPowered: c.routeAPowered,
      routeBPowered: c.routeBPowered,
      bothRoutesHealthy: c.routeAPowered && c.routeBPowered,
      checkRemark: c.checkRemark,
      checkedBy: v.checkedBy,
      checkedAt: new Date()
    }));
    this.api.submitDualPowerCheck({
      requestId: this.request.id, checks, checkedBy: v.checkedBy, remark: v.remark
    }).subscribe({
      next: (r) => {
        this.request = r;
        const passed = checks.every(c => c.bothRoutesHealthy);
        this.snack.open(passed ? '校验通过，可进入执行' : '校验未通过，请排查', '关闭', { duration: 3000 });
        this.loadDetail();
      },
      error: () => this.snack.open('提交失败', '关闭', { duration: 3000 })
    });
  }

  startStep(i: number): void {
    const stepCtrl = this.stepsArray.at(i);
    const stepId = stepCtrl.get('id')?.value;
    if (!stepId || !this.request) return;
    const operatorName = this.request.engineerName || '操作员';
    this.api.startStep({ stepId, operatorName }).subscribe({
      next: (s) => {
        stepCtrl.patchValue({ status: s.status });
        this.loadDetail();
      }
    });
  }

  completeStep(i: number): void {
    const stepCtrl = this.stepsArray.at(i);
    const stepId = stepCtrl.get('id')?.value;
    if (!stepId) return;
    this.api.completeStep(stepId).subscribe({
      next: (s) => {
        stepCtrl.patchValue({ status: s.status });
        this.loadDetail();
      }
    });
  }

  skipStep(i: number): void {
    const stepCtrl = this.stepsArray.at(i);
    const stepId = stepCtrl.get('id')?.value;
    if (!stepId) return;
    this.api.skipStep(stepId, '手动跳过').subscribe({
      next: (s) => {
        stepCtrl.patchValue({ status: s.status });
        this.loadDetail();
      }
    });
  }

  startExecution(): void {
    if (!this.request) return;
    this.api.canExecute(this.request.id).subscribe({
      next: (can) => {
        if (!can) {
          this.snack.open('前置校验未通过，请确认低峰、双路校验及告警', '关闭', { duration: 4000 });
          return;
        }
        const operatorName = this.request.engineerName || '操作员';
        this.api.startExecution({ requestId: this.request!.id, operatorName }).subscribe({
          next: (r) => {
            this.request = r;
            this.snack.open('已开始执行', '关闭', { duration: 2000 });
          },
          error: (e) => this.snack.open(e.error || '启动失败', '关闭', { duration: 4000 })
        });
      }
    });
  }

  completeExecution(): void {
    if (!this.request) return;
    const operatorName = this.request.engineerName || '操作员';
    this.api.completeExecution({ requestId: this.request.id, operatorName }).subscribe({
      next: (r) => {
        this.request = r;
        this.snack.open('切换已完成，影响业务清单已生成', '关闭', { duration: 3000 });
        this.loadDetail();
      },
      error: (e) => this.snack.open(e.error || '完成失败', '关闭', { duration: 4000 })
    });
  }

  openRollbackDialog(): void {
    const reason = prompt('请输入回退原因：');
    if (!reason || !this.request) return;
    const operatorName = this.request.engineerName || '操作员';
    this.api.rollback({ requestId: this.request.id, operatorName, reason }).subscribe({
      next: (r) => {
        this.request = r;
        this.snack.open('已记录回退', '关闭', { duration: 2000 });
        this.loadDetail();
      }
    });
  }

  openAlarmDialog(): void {
    if (!this.request) return;
    const dialogRef = this.dialog.open(AlarmDialogComponent, { width: '480px', data: { requestId: this.request.id } });
    dialogRef.afterClosed().subscribe(result => {
      if (result) this.loadDetail();
    });
  }

  openConfirmAlarmDialog(alarm: AlarmRecord): void {
    const confirmedBy = prompt('请输入确认人姓名：');
    if (!confirmedBy) return;
    const remark = prompt('确认备注（可选）：') || '';
    this.api.confirmAlarm({ alarmId: alarm.id, confirmedBy, confirmRemark: remark }).subscribe({
      next: () => {
        this.snack.open('告警已确认', '关闭', { duration: 2000 });
        this.loadDetail();
      }
    });
  }

  verifyImpact(bi: BusinessImpact): void {
    const verifiedBy = prompt('请输入验证人姓名：');
    if (!verifiedBy) return;
    this.api.verifyImpact(bi.id, verifiedBy).subscribe({
      next: () => {
        this.snack.open('验证已记录', '关闭', { duration: 2000 });
        this.loadDetail();
      }
    });
  }
}

@Component({
  selector: 'app-alarm-dialog',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule,
    MatDialogModule, MatFormFieldModule, MatInputModule, MatSelectModule,
    MatButtonModule
  ],
  template: `
    <h2 mat-dialog-title>登记告警</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="form-panel">
        <mat-form-field class="full-width">
          <mat-label>告警级别</mat-label>
          <mat-select formControlName="severity">
            <mat-option [value]="1">信息</mat-option>
            <mat-option [value]="2">警告</mat-option>
            <mat-option [value]="3">严重</mat-option>
          </mat-select>
        </mat-form-field>
        <mat-form-field class="full-width">
          <mat-label>告警消息</mat-label>
          <textarea matInput rows="3" formControlName="alarmMessage"></textarea>
        </mat-form-field>
        <mat-form-field class="full-width">
          <mat-label>来源设备</mat-label>
          <input matInput formControlName="sourceDevice">
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="dialogRef.close()">取消</button>
      <button mat-raised-button color="primary" [disabled]="form.invalid" (click)="onSubmit()">登记</button>
    </mat-dialog-actions>
  `,
  styles: ['mat-form-field { margin-bottom: 8px; }']
})
export class AlarmDialogComponent implements OnInit {
  form!: FormGroup;

  constructor(
    public dialogRef: MatDialogRef<AlarmDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { requestId: string },
    private fb: FormBuilder,
    private api: ApiService,
    private snack: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      severity: [2, Validators.required],
      alarmMessage: ['', Validators.required],
      sourceDevice: ['']
    });
  }

  onSubmit(): void {
    if (this.form.invalid || !this.data?.requestId) return;
    this.api.createAlarm(this.data.requestId, this.form.value).subscribe({
      next: () => {
        this.snack.open('告警已登记', '关闭', { duration: 2000 });
        this.dialogRef.close(true);
      },
      error: () => this.snack.open('登记失败', '关闭', { duration: 3000 })
    });
  }
}
