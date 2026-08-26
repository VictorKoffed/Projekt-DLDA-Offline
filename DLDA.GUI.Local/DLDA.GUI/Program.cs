using DLDA.GUI.Services;

/// <summary>
/// Application entry point and configuration setup.
/// This file acts as the composition root, wiring up dependency injection, external configurations, 
/// and establishing the middleware pipeline that processes all incoming HTTP requests.
/// </summary>

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Configure distributed memory cache and session management.
// Required to maintain user state and authentication context across HTTP requests, 
// as the MVC frontend relies on sessions rather than stateless JWTs for UI flow control.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    // 30 minutes is a standard security baseline to automatically log out inactive users.
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    
    // Setting HttpOnly prevents client-side scripts from accessing the session cookie, 
    // which acts as a critical mitigation against Cross-Site Scripting (XSS) attacks.
    options.Cookie.HttpOnly = true;
    
    // Marks the cookie as essential so it bypasses GDPR tracking-cookie consent checks.
    // Without this, the authentication mechanism would fail for users who decline optional cookies.
    options.Cookie.IsEssential = true;
});

// Retrieve the API base URL from appsettings.json OR the Docker environment variables.
// Externalizing this value enables seamless transitioning between local development, 
// testing, and containerized deployment environments without requiring recompilation.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"];

// Register a named HttpClient for API communication.
// Utilizing IHttpClientFactory rather than instantiating HttpClient directly prevents socket exhaustion 
// and correctly manages DNS lifecycle updates, which is essential in dynamic Docker environments.
builder.Services.AddHttpClient("DLDA", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl!);
});

// Register all domain services that act as bridges to the DLDA.API.
// Scoped lifetime is chosen so that a fresh instance is created per HTTP request, 
// isolating any potential state to the current user's workflow.
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<QuestionAdminService>();
builder.Services.AddScoped<UserAdminService>();
builder.Services.AddScoped<PatientAssessmentService>();
builder.Services.AddScoped<PatientQuizService>();
builder.Services.AddScoped<PatientResultService>();
builder.Services.AddScoped<PatientStatisticsService>();
builder.Services.AddScoped<StaffAssessmentService>();
builder.Services.AddScoped<StaffQuizService>();
builder.Services.AddScoped<StaffResultService>();
builder.Services.AddScoped<StaffStatisticsService>();

var app = builder.Build();

// Configure the HTTP request middleware pipeline.
// The order of these registrations is strictly sequential and dictates how requests are processed.
if (!app.Environment.IsDevelopment())
{
    // Centralized error handling prevents stack traces from bleeding to the end-user in production environments.
    app.UseExceptionHandler("/Home/Error");
    
    // Enforces Strict-Transport-Security (HSTS), ensuring browsers only ever connect via HTTPS.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Activates session state for the current request.
// This MUST be placed after UseRouting and before mapping endpoints, 
// ensuring session data is fully materialized before any Controller logic attempts to access it.
app.UseSession(); 

// Configure the default routing convention.
// Directing traffic to Account/Login by default ensures that unauthenticated users 
// are immediately presented with the login gateway when accessing the root URL.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();