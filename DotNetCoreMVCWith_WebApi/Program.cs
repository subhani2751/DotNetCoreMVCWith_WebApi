using DotNetCoreMVCWith_WebApi.MyDatabaseContext;
using DotNetCoreMVCWith_WebApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<EmployeeDbContext>(Option =>
 Option.UseSqlServer(builder.Configuration.GetConnectionString("Defaultconnection")));
builder.Services.AddControllersWithViews().AddViewOptions(options => {
    options.HtmlHelperOptions.ClientValidationEnabled = false;
});
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<CacheMemory>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Employee}/{action=AddEmployee}/{id?}");

app.Run();
