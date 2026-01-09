using Infrastructure.Hubs;


var builder = WebApplication.CreateBuilder(args);

// ==================================================================
// 1. تسجيل الخدمات (Dependency Injection) - هنا كان النقص
// ==================================================================

// استدعاء طبقة البنية التحتية (Database & Repositories)
builder.Services.AddInfrastructureServices(builder.Configuration);

// استدعاء طبقة التطبيق (Services, AutoMapper, Validators)
// هذا السطر هو الذي سيحل مشكلة IPurchaseOrderService
builder.Services.AddApplicationLayer(builder.Configuration);

// استدعاء طبقة العرض (MVC, Localization)
builder.Services.AddPresentationLayer();

// إعدادات الجلسة (Session)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// إعدادات الـ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Add SignalR Service
builder.Services.AddSignalR();

// ==================================================================
// 2. بناء التطبيق (Build App)
// ==================================================================
var app = builder.Build();

// إعدادات الـ Pipeline (Middleware)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication(); // لو ضفت Identity مستقبلاً
app.UseAuthorization();

app.UseSession();

// توجيه الـ Routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// تشغيل الـ SignalR Hub
app.MapHub<Notifications>("/hubs/notifications"); // تأكد أن هذا هو المسار الذي استخدمته في الـ JS

app.Run();