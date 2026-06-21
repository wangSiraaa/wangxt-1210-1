using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PowerSwitchApi.Data;
using PowerSwitchApi.Models;

namespace PowerSwitchApi.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider, bool autoSeed = true)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            await context.Database.EnsureCreatedAsync();
            if (autoSeed)
            {
                await SeedDeviceTopologyAsync(context);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("数据库初始化失败，请检查连接字符串或数据库服务是否启动", ex);
        }
    }

    public static async Task SeedDeviceTopologyAsync(AppDbContext context)
    {
        if (await context.DeviceTopologies.AnyAsync())
            return;

        var ups1 = new DeviceTopology
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000001"),
            DeviceType = DeviceType.UPS,
            DeviceCode = "UPS-A01",
            DeviceName = "1号UPS",
            Location = "A区配电室1层",
            ParentId = null,
            HasDualPower = true,
            RouteASource = "市电A路",
            RouteBSource = "市电B路"
        };

        var ups2 = new DeviceTopology
        {
            Id = Guid.Parse("10000000-0000-0000-0000-000000000002"),
            DeviceType = DeviceType.UPS,
            DeviceCode = "UPS-B01",
            DeviceName = "2号UPS",
            Location = "B区配电室1层",
            ParentId = null,
            HasDualPower = true,
            RouteASource = "市电A路",
            RouteBSource = "市电B路"
        };

        var bus1 = new DeviceTopology
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000001"),
            DeviceType = DeviceType.BusBar,
            DeviceCode = "BUS-A1",
            DeviceName = "A区1段母线",
            Location = "A区配电室1层",
            ParentId = ups1.Id,
            HasDualPower = true,
            RouteASource = "UPS-A01-A路",
            RouteBSource = "UPS-B01-B路"
        };

        var bus2 = new DeviceTopology
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000002"),
            DeviceType = DeviceType.BusBar,
            DeviceCode = "BUS-A2",
            DeviceName = "A区2段母线",
            Location = "A区配电室1层",
            ParentId = ups1.Id,
            HasDualPower = true,
            RouteASource = "UPS-A01-A路",
            RouteBSource = "UPS-B01-B路"
        };

        var bus3 = new DeviceTopology
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000003"),
            DeviceType = DeviceType.BusBar,
            DeviceCode = "BUS-B1",
            DeviceName = "B区1段母线",
            Location = "B区配电室1层",
            ParentId = ups2.Id,
            HasDualPower = true,
            RouteASource = "UPS-A01-A路",
            RouteBSource = "UPS-B01-B路"
        };

        var bus4 = new DeviceTopology
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000004"),
            DeviceType = DeviceType.BusBar,
            DeviceCode = "BUS-B2",
            DeviceName = "B区2段母线",
            Location = "B区配电室1层",
            ParentId = ups2.Id,
            HasDualPower = true,
            RouteASource = "UPS-A01-A路",
            RouteBSource = "UPS-B01-B路"
        };

        var cab1 = new DeviceTopology
        {
            Id = Guid.Parse("30000000-0000-0000-0000-000000000001"),
            DeviceType = DeviceType.Cabinet,
            DeviceCode = "CAB-A1-01",
            DeviceName = "A1-01机柜",
            Location = "A区机房1排1列",
            ParentId = bus1.Id,
            HasDualPower = true,
            RouteASource = "BUS-A1-A路",
            RouteBSource = "BUS-A2-B路",
            ConnectedBusinessSystems = "交易系统,支付网关"
        };

        var cab2 = new DeviceTopology
        {
            Id = Guid.Parse("30000000-0000-0000-0000-000000000002"),
            DeviceType = DeviceType.Cabinet,
            DeviceCode = "CAB-A1-02",
            DeviceName = "A1-02机柜",
            Location = "A区机房1排2列",
            ParentId = bus1.Id,
            HasDualPower = true,
            RouteASource = "BUS-A1-A路",
            RouteBSource = "BUS-A2-B路",
            ConnectedBusinessSystems = "核心交易数据库"
        };

        var cab3 = new DeviceTopology
        {
            Id = Guid.Parse("30000000-0000-0000-0000-000000000003"),
            DeviceType = DeviceType.Cabinet,
            DeviceCode = "CAB-A1-03",
            DeviceName = "A1-03机柜",
            Location = "A区机房1排3列",
            ParentId = bus2.Id,
            HasDualPower = true,
            RouteASource = "BUS-A2-A路",
            RouteBSource = "BUS-A1-B路",
            ConnectedBusinessSystems = "风控系统,报表系统"
        };

        var cab4 = new DeviceTopology
        {
            Id = Guid.Parse("30000000-0000-0000-0000-000000000004"),
            DeviceType = DeviceType.Cabinet,
            DeviceCode = "CAB-B1-01",
            DeviceName = "B1-01机柜",
            Location = "B区机房1排1列",
            ParentId = bus3.Id,
            HasDualPower = true,
            RouteASource = "BUS-B1-A路",
            RouteBSource = "BUS-B2-B路",
            ConnectedBusinessSystems = "客户系统,用户中心"
        };

        var cab5 = new DeviceTopology
        {
            Id = Guid.Parse("30000000-0000-0000-0000-000000000005"),
            DeviceType = DeviceType.Cabinet,
            DeviceCode = "CAB-B1-02",
            DeviceName = "B1-02机柜",
            Location = "B区机房1排2列",
            ParentId = bus3.Id,
            HasDualPower = true,
            RouteASource = "BUS-B1-A路",
            RouteBSource = "BUS-B2-B路",
            ConnectedBusinessSystems = "消息中间件集群"
        };

        var cab6 = new DeviceTopology
        {
            Id = Guid.Parse("30000000-0000-0000-0000-000000000006"),
            DeviceType = DeviceType.Cabinet,
            DeviceCode = "CAB-B1-03",
            DeviceName = "B1-03机柜",
            Location = "B区机房1排3列",
            ParentId = bus4.Id,
            HasDualPower = true,
            RouteASource = "BUS-B2-A路",
            RouteBSource = "BUS-B1-B路",
            ConnectedBusinessSystems = "日志分析,监控平台"
        };

        context.DeviceTopologies.AddRange(ups1, ups2, bus1, bus2, bus3, bus4, cab1, cab2, cab3, cab4, cab5, cab6);
        await context.SaveChangesAsync();
    }
}
