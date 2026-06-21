# 供电切换申请系统 - 部署验证指南

## 🔧 问题修复清单

### 1. 循环依赖 500 错误 ✅ 已修复
**问题**: `DualPowerCheckService` 构造函数注入 `IPowerSwitchRequestService`，导致依赖注入循环或解析失败。

**修复**:
- 移除 `DualPowerCheckService` 对 `IPowerSwitchRequestService` 的依赖
- 新增私有方法 `GetFullRequestById()` 直接查询数据库
- 文件: [OtherServices.cs](file:///Users/mingyuan/workspace/sihuo/wangxtw3/1210/PowerSwitchApi/Services/OtherServices.cs#L219-L285)

### 2. 数据库自动初始化 ✅ 已添加
**新增**:
- `DatabaseInitializer.InitializeDatabaseAsync()`: EnsureCreated + 种子数据
- 12 个设备拓扑种子（2 UPS + 4 母线 + 6 机柜，关联业务系统）
- 文件: [DatabaseInitializer.cs](file:///Users/mingyuan/workspace/sihuo/wangxtw3/1210/PowerSwitchApi/Data/DatabaseInitializer.cs)

### 3. 全局异常处理 ✅ 已添加
**新增**:
- `GlobalExceptionMiddleware`: 返回结构化 JSON 错误，避免 500 空白页
- `UseDeveloperExceptionPage()`: 开发环境显示详细堆栈
- 文件: [GlobalExceptionMiddleware.cs](file:///Users/mingyuan/workspace/sihuo/wangxtw3/1210/PowerSwitchApi/Middleware/GlobalExceptionMiddleware.cs)

### 4. 连接字符串兼容性 ✅ 已修复
- 默认使用 `(localdb)\MSSQLLocalDB`，无需单独安装 SQL Server
- 同时支持完整版 SQL Server 和 Docker SQL Server
- 支持通过环境变量覆盖
- 文件: [appsettings.json](file:///Users/mingyuan/workspace/sihuo/wangxtw3/1210/PowerSwitchApi/appsettings.json)

### 5. Program.cs 加固 ✅ 已重写
- JSON 循环引用处理（ReferenceHandler.IgnoreCycles）
- EF Core 连接失败自动重试
- 健康检查端点 `/health`
- Swagger 作为根路径
- 启动时自动初始化数据库并记录日志
- 文件: [Program.cs](file:///Users/mingyuan/workspace/sihuo/wangxtw3/1210/PowerSwitchApi/Program.cs)

---

## 🚀 启动方式

### 方式一: LocalDB（零配置，推荐 ✅）

LocalDB 是 Visual Studio 自带的轻量 SQL Server 版本，无需单独安装服务。

```bash
# Windows
scripts\start-api.ps1

# macOS/Linux
bash scripts/start-api.sh
```

**如果 LocalDB 未安装**:
- Visual Studio Installer → 勾选 "Data storage and processing" 工作负载
- 或单独安装: https://learn.microsoft.com/zh-cn/sql/database-engine/configure-windows/sql-server-express-localdb

### 方式二: 完整版 SQL Server

修改 `PowerSwitchApi/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=PowerSwitchDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 方式三: Docker SQL Server

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong!Passw0rd" \
  -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest

ConnectionStrings__DefaultConnection="Server=localhost;Database=PowerSwitchDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;" \
  dotnet run --project PowerSwitchApi
```

---

## ✅ 全链路验证脚本

```bash
# 确保 API 已启动后，执行:
bash scripts/test-full-flow.sh
```

**脚本验证内容**:
| 阶段 | 验证项 | 预期结果 |
|---|---|---|
| 0 | `GET /api/DeviceTopology/tree` | 返回 12 个设备的树结构 |
| 0 | `GET /api/PowerSwitchRequests` | 返回空数组或已有数据 |
| 1 | `POST /api/PowerSwitchRequests` | 创建申请，状态=草稿 |
| 2 | `POST /api/PowerSwitchRequests/affected-devices` | 值班经理保存UPS/母线/机柜 |
| 3 | `POST /api/PowerSwitchRequests/switch-steps` | 工程师保存4个切换步骤 |
| 4 | `POST /api/PowerSwitchRequests/confirm-lowpeak` | 业务负责人确认低峰 |
| 5 | `POST /api/DualPowerChecks` (有设备未通过) | 保存校验结果，但状态不变 |
| 5 | `GET /api/PowerSwitchRequests/{id}/can-execute` | 返回 **false** ✅ 拦截验证 |
| 5 | `POST /api/DualPowerChecks` (全部通过) | 状态变为 "双路供电已校验" |
| 5 | `GET /api/PowerSwitchRequests/{id}/can-execute` | 返回 **true** ✅ 可执行 |
| 6 | `POST /api/PowerSwitchRequests/start-execution` | 状态变为 "执行中" |
| 6 | `POST /api/Alarms/request/{id}` | 登记一条警告级告警 |
| 6 | `POST /api/PowerSwitchRequests/complete-execution` | 返回 **HTTP 400** ✅ 告警拦截验证 |
| 6 | `POST /api/Alarms/confirm` | 确认告警 |
| 6 | `POST /api/PowerSwitchRequests/complete-execution` | 状态变为 "已完成" |
| 7 | `GET /api/BusinessImpacts/by-request/{id}` | 返回自动生成的影响业务清单 ✅ |

---

## 🔗 关键代码参考

### 双路供电校验拦截逻辑
[PowerSwitchRequestService.cs](file:///Users/mingyuan/workspace/sihuo/wangxtw3/1210/PowerSwitchApi/Services/PowerSwitchRequestService.cs#L186-L210)
```csharp
public async Task<bool> CanExecuteAsync(Guid requestId)
{
    // 1. 双路校验必须通过
    if (!request.DualPowerCheckPassed) return false;
    // 2. 低峰时段必须确认
    if (!request.IsLowPeakConfirmed) return false;
    // 3. 状态必须 >= DualPowerChecked
    if (request.Status < RequestStatus.DualPowerChecked) return false;
    // 4. 不能有未确认的警告/严重告警
    var unconfirmedAlarms = await _db.AlarmRecords
        .AnyAsync(a => a.RequestId == requestId && !a.IsConfirmed && a.Severity >= AlarmSeverity.Warning);
    if (unconfirmedAlarms) return false;
    return true;
}
```

### 完成时告警拦截逻辑
[PowerSwitchRequestService.cs](file:///Users/mingyuan/workspace/sihuo/wangxtw3/1210/PowerSwitchApi/Services/PowerSwitchRequestService.cs#L212-L255)
```csharp
public async Task<PowerSwitchRequestDto?> CompleteExecutionAsync(...)
{
    var unconfirmedCritical = request.AlarmRecords
        .Any(a => !a.IsConfirmed && a.Severity >= AlarmSeverity.Warning);
    if (unconfirmedCritical) return null; // 拦截，返回 null → 控制器返回 HTTP 400
    ...
}
```

### 自动生成影响业务清单
[PowerSwitchRequestService.cs](file:///Users/mingyuan/workspace/sihuo/wangxtw3/1210/PowerSwitchApi/Services/PowerSwitchRequestService.cs#L270-L312)
```csharp
private async Task GenerateBusinessImpactList(PowerSwitchRequest request)
{
    // 根据机柜设备编码查找拓扑
    // 从拓扑 ConnectedBusinessSystems 字段解析关联的业务系统
    // 按业务系统聚合计数，生成 BusinessImpact 记录
}
```

---

## 📊 数据库结构

### 种子设备拓扑
| 层级 | 设备编码 | 名称 | 关联业务系统 |
|---|---|---|---|
| L1 | UPS-A01 | 1号UPS | - |
| L1 | UPS-B01 | 2号UPS | - |
| L2 | BUS-A1 / BUS-A2 / BUS-B1 / BUS-B2 | 4条母线 | - |
| L3 | CAB-A1-01 | A1-01机柜 | 交易系统, 支付网关 |
| L3 | CAB-A1-02 | A1-02机柜 | 核心交易数据库 |
| L3 | CAB-A1-03 | A1-03机柜 | 风控系统, 报表系统 |
| L3 | CAB-B1-01 | B1-01机柜 | 客户系统, 用户中心 |
| L3 | CAB-B1-02 | B1-02机柜 | 消息中间件集群 |
| L3 | CAB-B1-03 | B1-03机柜 | 日志分析, 监控平台 |

### 状态流转
```
草稿 → 值班经理已登记 → 强电工程师已录入 → 业务负责人已确认
                                                             ↓
                                              双路供电已校验（自动拦截）
                                                             ↓
                                                   待执行 → 执行中 ⇄ 告警待确认
                                                             ↓
                                                    已完成（自动生成影响清单）
                                                             ↓
                                                          或 已回退
```

---

## ❌ 常见错误排查

### 错误: "数据库初始化失败"
- 确认 LocalDB / SQL Server 正在运行
- 检查连接字符串中的服务器名称
- 执行 `sqllocaldb info MSSQLLocalDB` 查看 LocalDB 状态
- 启动 LocalDB: `sqllocaldb start MSSQLLocalDB`

### 错误: 500 循环依赖异常
- 已修复，不会再出现。旧代码 DualPowerCheckService 注入了 IPowerSwitchRequestService

### 错误: 请求列表/拓扑返回空
- 检查 `/health` 端点确认服务正常
- 首次启动会自动执行 EnsureCreated 和种子数据插入
- 查看日志: "数据库初始化完成"
