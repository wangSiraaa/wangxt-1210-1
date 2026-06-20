export enum RequestStatus {
  Draft = 0,
  DutyManagerFilled = 10,
  EngineerFilled = 20,
  BusinessConfirmed = 30,
  DualPowerChecked = 40,
  ReadyForExecution = 50,
  Executing = 60,
  AlarmsPending = 65,
  Recovering = 70,
  Completed = 80,
  RolledBack = 90,
  Cancelled = 100
}

export enum DeviceType {
  UPS = 1,
  BusBar = 2,
  Cabinet = 3,
  ATS = 4,
  PDU = 5
}

export enum AlarmSeverity {
  Info = 1,
  Warning = 2,
  Critical = 3
}

export enum StepStatus {
  Pending = 0,
  Executing = 1,
  Completed = 2,
  Skipped = 3,
  Failed = 4
}

export const DeviceTypeText: Record<number, string> = {
  1: 'UPS',
  2: '母线',
  3: '机柜',
  4: 'ATS',
  5: 'PDU'
};

export const StatusTextMap: Record<number, string> = {
  0: '草稿',
  10: '值班经理已登记',
  20: '强电工程师已录入',
  30: '业务负责人已确认',
  40: '双路供电已校验',
  50: '待执行',
  60: '执行中',
  65: '告警待确认',
  70: '恢复中',
  80: '已完成',
  90: '已回退',
  100: '已取消'
};

export const StatusClassMap: Record<number, string> = {
  0: 'status-draft',
  10: 'status-duty',
  20: 'status-engineer',
  30: 'status-business',
  40: 'status-checked',
  50: 'status-ready',
  60: 'status-executing',
  65: 'status-alarm',
  70: 'status-recovering',
  80: 'status-completed',
  90: 'status-rolledback',
  100: 'status-cancelled'
};

export const StepStatusText: Record<number, string> = {
  0: '待执行',
  1: '执行中',
  2: '已完成',
  3: '已跳过',
  4: '失败'
};

export const StepStatusClass: Record<number, string> = {
  0: 'step-pending',
  1: 'step-executing',
  2: 'step-completed',
  3: 'step-skipped',
  4: 'step-failed'
};

export const AlarmSeverityText: Record<number, string> = {
  1: '信息',
  2: '警告',
  3: '严重'
};

export const AlarmClassMap: Record<number, string> = {
  1: 'alarm-info',
  2: 'alarm-warning',
  3: 'alarm-critical'
};

export interface PowerSwitchRequest {
  id: string;
  requestNo: string;
  title: string;
  description?: string;
  status: RequestStatus;
  statusText: string;
  riskWindowStart: Date;
  riskWindowEnd: Date;
  dutyManagerName?: string;
  dutyManagerFilledAt?: Date;
  engineerName?: string;
  engineerFilledAt?: Date;
  businessOwnerName?: string;
  businessConfirmedAt?: Date;
  isLowPeakConfirmed: boolean;
  lowPeakRemark?: string;
  dualPowerCheckPassed: boolean;
  dualPowerCheckedAt?: Date;
  dualPowerCheckRemark?: string;
  executionStartedAt?: Date;
  executionCompletedAt?: Date;
  rollbackReason?: string;
  rolledBackAt?: Date;
  createdAt: Date;
  updatedAt: Date;
  affectedDevices: AffectedDevice[];
  switchSteps: SwitchStep[];
  alarmRecords: AlarmRecord[];
  businessImpacts: BusinessImpact[];
}

export interface AffectedDevice {
  id: string;
  deviceType: DeviceType;
  deviceCode: string;
  deviceName: string;
  location?: string;
  impactDescription?: string;
}

export interface SwitchStep {
  id: string;
  sequence: number;
  stepType: string;
  description: string;
  operationDetail?: string;
  estimatedDurationSeconds: number;
  status: StepStatus;
  statusText: string;
  startedAt?: Date;
  completedAt?: Date;
  operatorName?: string;
  remark?: string;
  isRollbackStep: boolean;
}

export interface AlarmRecord {
  id: string;
  severity: AlarmSeverity;
  severityText: string;
  alarmMessage: string;
  sourceDevice?: string;
  alarmTime: Date;
  isConfirmed: boolean;
  confirmedBy?: string;
  confirmedAt?: Date;
  confirmRemark?: string;
}

export interface BusinessImpact {
  id: string;
  businessSystemName: string;
  systemCode?: string;
  affectedCabinetCount: number;
  actualImpactStart?: Date;
  actualImpactEnd?: Date;
  impactDescription?: string;
  recoveryDetail?: string;
  verifiedBy?: string;
  verifiedAt?: Date;
}

export interface DualPowerCheck {
  id: string;
  deviceCode: string;
  deviceType: DeviceType;
  routeAPowered: boolean;
  routeBPowered: boolean;
  bothRoutesHealthy: boolean;
  checkRemark?: string;
  checkedBy?: string;
  checkedAt: Date;
}

export interface DeviceTopology {
  id: string;
  deviceType: DeviceType;
  deviceTypeText: string;
  deviceCode: string;
  deviceName: string;
  location?: string;
  parentId?: string;
  hasDualPower: boolean;
  routeASource?: string;
  routeBSource?: string;
  connectedBusinessSystems?: string;
  children: DeviceTopology[];
}
