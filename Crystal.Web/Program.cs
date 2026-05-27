using Crystal.Web.Data;
using Crystal.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

// буилдер собирает настройки приложения и сервисы до запуска
var builder = WebApplication.CreateBuilder(args);

// подключаем mvc чтобы работали контроллеры и вьюхи
builder.Services.AddControllersWithViews();
// сервис токенов создается на каждый запрос
builder.Services.AddScoped<ISessionTokenService, SessionTokenService>();
// регистрация схемы аутентификации 
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.SlidingExpiration = false;
        options.Events = new CookieAuthenticationEvents
        {
            // при каждом запросе проверяем жива ли сессия и можно ли ее обновить
            OnValidatePrincipal = async context =>
            {
                var sessionTokenService = context.HttpContext.RequestServices.GetRequiredService<ISessionTokenService>();
                var princ = await sessionTokenService.ValidateOrRefreshAsync(context.Principal!, context.HttpContext);

                if (princ == null)
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    sessionTokenService.DeleteRefreshTokenCookie(context.HttpContext.Response);
                    return;
                }

                if (!ReferenceEquals(princ, context.Principal)) // тоже заумный выкрутас из рефернса
				// по сути можно было бы == но IS-like сравнение объекта. Сделаем вид что все знаю чем отличается сравнение значений от сравнения сущностей
                {
                    context.ReplacePrincipal(princ);
                    context.ShouldRenew = true;
                }
            }
        };
    });

// секур ограничение (по сути мидла, которая дает стучаться в админку только админам)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// подключение бд
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// после регистрации сервисов собираем готовое приложение
var app = builder.Build();

// при старте прогоняем миграции (в проде лучше выносить логику в дополнительный сервис)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

// в продакшене включается отдельная страница ошибок и hsts
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // шо такое хстс? это протокол безопасности. По сути еще одна мидла которая запрещает любые обращения кроме хттп
}

// стандартный конвейер асп. Взято из документации. Сильно не вникал но по названиям все и так понятно
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// маршрут по умолчанию (роут на список задач)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ран
app.Run();
