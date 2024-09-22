using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using MvcBlog.Data;
using Microsoft.AspNetCore.Identity;
using MvcBlog.Areas.Identity.Data;



var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<MvcBlogContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING") ?? throw new InvalidOperationException("Connection string 'MvcBlogContext' not found.")));
builder.Services.AddDbContext<MvcBlogAuth>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING") ?? throw new InvalidOperationException("Connection string 'MvcBlogContext' not found.")));

builder.Services.AddDefaultIdentity<MvcBlogUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<MvcBlogAuth>();


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(options =>
    options.Conventions.AuthorizePage("/AdminsOnly", "Admin"));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireClaim("IsAdmin"));
});

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

app.MapControllerRoute(
    name: "blog",
    pattern: "Blog",
    defaults: new { controller = "BlogPost", action = "Index" });

app.MapControllerRoute(
    name: "blogpost",
    pattern: "BlogPost/Index/{searchtag?}/{pagenumber?}",
    defaults: new { controller = "BlogPost", action = "Index" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
