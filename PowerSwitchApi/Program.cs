using Microsoft.EntityFrameworkCore;
using PowerSwitchApi.Data;
using PowerSwitchApi.Middleware;
using PowerSwitchApi.Services;
using PowerSwitchApi.Mappings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PowerSwitchApi", Version = "v1" });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    }));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddScoped<IPowerSwitchRequestService, PowerSwitchRequestService>();
builder.Services.AddScoped<IDeviceTopologyService, DeviceTopologyService>();
builder.Services.AddScoped<ISwitchStepService, SwitchStepService>();
builder.Services.AddScoped<IAlarmService, AlarmService>();
builder.Services.AddScoped<IBusinessImpactService, BusinessImpactService>();
builder.Services.AddScoped<IDualPowerCheckService, DualPowerCheckService>();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PowerSwitchApi v1");
        c.RoutePrefix = string.Empty;
    });
}
else
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAngular");
app.UseRouting();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

var autoSeed = builder.Configuration.GetValue<bool>("Database:AutoSeed", true);
try
{
    await DatabaseInitializer.InitializeDatabaseAsync(app.Services, autoSeed);
    app.Logger.LogInformation("数据库初始化完成，连接字符串: {Conn}", connectionString);
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "数据库初始化失败。请检查: (1) LocalDB/SQL Server是否已启动 (2) 连接字符串是否正确 (3) 当前用户是否有数据库权限");
    app.Logger.LogInformation("如未安装LocalDB，可安装: https://learn.microsoft.com/zh-cn/sql/database-engine/configure-windows/sql-server-express-localdb");
}

app.Logger.LogInformation("供电切换API已启动，Swagger: http://localhost:5000");
await app.RunAsync();
