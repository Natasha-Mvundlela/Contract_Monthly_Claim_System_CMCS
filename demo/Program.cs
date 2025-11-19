using Contract_Monthly_Claim_System_CMCS.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Session;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add authentication if needed (optional)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Home/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
    });

// Add HTTP context accessor
builder.Services.AddHttpContextAccessor();

// Add logging
builder.Services.AddLogging();

// Register your services
builder.Services.AddScoped<created_queries>();

// Add MVC services
builder.Services.AddMvc();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    // Development settings
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add session middleware
app.UseSession();

// Add authentication & authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Initialize the database system on startup
try
{
    using (var scope = app.Services.CreateScope())
    {
        var queries = scope.ServiceProvider.GetRequiredService<created_queries>();
        queries.InitializeSystem();
        queries.TestConnection();
        Console.WriteLine("System initialized successfully on startup");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"System initialization failed: {ex.Message}");
}

app.Run();