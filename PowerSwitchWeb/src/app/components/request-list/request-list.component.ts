import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { ApiService } from '../../services/api.service';
import { PowerSwitchRequest, StatusTextMap, StatusClassMap } from '../../models';

@Component({
  selector: 'app-request-list',
  standalone: true,
  imports: [
    CommonModule, RouterLink, FormsModule, ReactiveFormsModule,
    MatButtonModule, MatCardModule, MatTableModule, MatIconModule,
    MatDialogModule, MatFormFieldModule, MatInputModule, MatDatepickerModule,
    MatSnackBarModule
  ],
  template: `
    <div class="page-header">
      <h1 class="page-title">供电切换申请列表</h1>
      <button mat-raised-button color="primary" (click)="openCreateDialog()">
        <mat-icon>add</mat-icon> 新建申请
      </button>
    </div>

    <mat-card class="section-card">
      <mat-table [dataSource]="requests" class="full-width">
        <ng-container matColumnDef="requestNo">
          <mat-header-cell *matHeaderCellDef>申请编号</mat-header-cell>
          <mat-cell *matCellDef="let r">
            <a [routerLink]="['/requests', r.id]" style="color:#1976d2;cursor:pointer;">{{r.requestNo}}</a>
          </mat-cell>
        </ng-container>

        <ng-container matColumnDef="title">
          <mat-header-cell *matHeaderCellDef>标题</mat-header-cell>
          <mat-cell *matCellDef="let r">{{r.title}}</mat-cell>
        </ng-container>

        <ng-container matColumnDef="status">
          <mat-header-cell *matHeaderCellDef>状态</mat-header-cell>
          <mat-cell *matCellDef="let r">
            <span class="status-badge" [ngClass]="getStatusClass(r.status)">{{getStatusText(r.status)}}</span>
          </mat-cell>
        </ng-container>

        <ng-container matColumnDef="riskWindow">
          <mat-header-cell *matHeaderCellDef>风险窗口</mat-header-cell>
          <mat-cell *matCellDef="let r">
            {{formatDate(r.riskWindowStart)}} ~ {{formatDate(r.riskWindowEnd)}}
          </mat-cell>
        </ng-container>

        <ng-container matColumnDef="createdAt">
          <mat-header-cell *matHeaderCellDef>创建时间</mat-header-cell>
          <mat-cell *matCellDef="let r">{{formatDate(r.createdAt)}}</mat-cell>
        </ng-container>

        <ng-container matColumnDef="actions">
          <mat-header-cell *matHeaderCellDef>操作</mat-header-cell>
          <mat-cell *matCellDef="let r">
            <button mat-icon-button color="primary" [routerLink]="['/requests', r.id]" title="查看详情">
              <mat-icon>visibility</mat-icon>
            </button>
            <button mat-icon-button color="warn" (click)="onDelete(r)" title="删除">
              <mat-icon>delete</mat-icon>
            </button>
          </mat-cell>
        </ng-container>

        <mat-header-row *matHeaderRowDef="displayedColumns"></mat-header-row>
        <mat-row *matRowDef="let row; columns: displayedColumns;"></mat-row>
      </mat-table>

      <div *ngIf="requests.length === 0" style="padding:48px;text-align:center;color:#999;">
        暂无数据，点击右上角「新建申请」开始创建
      </div>
    </mat-card>
  `
})
export class RequestListComponent implements OnInit {
  requests: PowerSwitchRequest[] = [];
  displayedColumns = ['requestNo', 'title', 'status', 'riskWindow', 'createdAt', 'actions'];
  form!: FormGroup;

  constructor(
    private api: ApiService,
    private dialog: MatDialog,
    private fb: FormBuilder,
    private router: Router,
    private snack: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.getRequests().subscribe({
      next: (data) => this.requests = data,
      error: () => this.snack.open('加载失败', '关闭', { duration: 3000 })
    });
  }

  getStatusText(status: number): string {
    return StatusTextMap[status] || '未知';
  }

  getStatusClass(status: number): string {
    return StatusClassMap[status] || 'status-draft';
  }

  formatDate(d: any): string {
    if (!d) return '';
    const date = new Date(d);
    return date.toLocaleString('zh-CN');
  }

  openCreateDialog(): void {
    const dialogRef = this.dialog.open(CreateRequestDialogComponent, { width: '520px' });
    dialogRef.afterClosed().subscribe(result => {
      if (result) this.load();
    });
  }

  onDelete(r: PowerSwitchRequest): void {
    if (!confirm(`确定删除申请 ${r.requestNo}?`)) return;
    this.api.deleteRequest(r.id).subscribe({
      next: () => {
        this.snack.open('已删除', '关闭', { duration: 2000 });
        this.load();
      },
      error: () => this.snack.open('删除失败', '关闭', { duration: 3000 })
    });
  }
}

@Component({
  selector: 'app-create-request-dialog',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule,
    MatDialogModule, MatFormFieldModule, MatInputModule, MatDatepickerModule,
    MatButtonModule
  ],
  template: `
    <h2 mat-dialog-title>新建供电切换申请</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="form-panel">
        <mat-form-field class="full-width">
          <mat-label>标题</mat-label>
          <input matInput formControlName="title" required placeholder="例如：A区1段母线UPS检修切换">
        </mat-form-field>

        <mat-form-field class="full-width">
          <mat-label>描述</mat-label>
          <textarea matInput formControlName="description" rows="3" placeholder="切换原因、预计影响等"></textarea>
        </mat-form-field>

        <div class="grid-2">
          <mat-form-field class="full-width">
            <mat-label>风险窗口开始</mat-label>
            <input matInput [matDatepicker]="startPicker" formControlName="riskWindowStart" required>
            <mat-datepicker-toggle matSuffix [for]="startPicker"></mat-datepicker-toggle>
            <mat-datepicker #startPicker></mat-datepicker>
          </mat-form-field>

          <mat-form-field class="full-width">
            <mat-label>风险窗口结束</mat-label>
            <input matInput [matDatepicker]="endPicker" formControlName="riskWindowEnd" required>
            <mat-datepicker-toggle matSuffix [for]="endPicker"></mat-datepicker-toggle>
            <mat-datepicker #endPicker></mat-datepicker>
          </mat-form-field>
        </div>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>取消</button>
      <button mat-raised-button color="primary" [disabled]="form.invalid" (click)="onSubmit()">
        创建
      </button>
    </mat-dialog-actions>
  `,
  styles: ['mat-form-field { margin-bottom: 8px; }']
})
export class CreateRequestDialogComponent implements OnInit {
  form!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private api: ApiService,
    private dialog: MatDialog,
    private snack: MatSnackBar
  ) {}

  ngOnInit(): void {
    const now = new Date();
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    this.form = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      riskWindowStart: [now, Validators.required],
      riskWindowEnd: [tomorrow, Validators.required]
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.api.createRequest(this.form.value).subscribe({
      next: () => {
        this.snack.open('创建成功', '关闭', { duration: 2000 });
        this.dialog.closeAll();
      },
      error: () => this.snack.open('创建失败', '关闭', { duration: 3000 })
    });
  }
}
