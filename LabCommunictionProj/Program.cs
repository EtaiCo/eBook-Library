var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Add session with configuration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set the session timeout to 30 minutes
    options.Cookie.HttpOnly = true; // Make the session cookie HTTP-only for security
    options.Cookie.IsEssential = true; // Ensure the session cookie is always sent
    options.Cookie.SameSite = SameSiteMode.Strict; // Prevent cross-site usage of the session cookie
});

// Optionally, add distributed memory cache for session storage
builder.Services.AddDistributedMemoryCache();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add session middleware before authorization
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
