using EldisMeterReadingsService.Web.Components;
using EldisMeterReadingsService.Web.Services;
using Microsoft.AspNetCore.Authentication.Negotiate;

var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Windows Authentication (Active Directory)
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

// Авторизация
builder.Services.AddAuthorization();

// Доступ к HTTP-контексту (нужен для получения информации о пользователе)
builder.Services.AddHttpContextAccessor();

// Регистрация наших сервисов
builder.Services.AddScoped<DbTestService>();
builder.Services.AddScoped<ObjectsService>();
builder.Services.AddScoped<DevicesService>();
builder.Services.AddScoped<RawDataQueryService>();
builder.Services.AddScoped<MeteringPointsService>();
builder.Services.AddScoped<UserAccessService>();
builder.Services.AddScoped<DebugService>();

builder.Services.AddMemoryCache(); //  ДОБАВИТЬ ЭТУ СТРОЧКУ

var app = builder.Build();

// Настройка окружения
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// ВАЖНО: Порядок middleware! Сначала аутентификация, потом авторизация
app.UseAuthentication();
app.UseAuthorization();

// Подключение Razor компонентов
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();