using Microsoft.EntityFrameworkCore;
using PowerSwitchApi.Data;
using PowerSwitchApi.Services;
using PowerSwitchApi.Mappings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "PowerSwitchApi", Version = "v1" });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");
app.UseAuthorization();
app.MapControllers();

app.Run();
