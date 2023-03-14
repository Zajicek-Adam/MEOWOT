using MEOWOT.Data;
using MEOWOT.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using tusdotnet;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();


builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequiredLength = 5;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddScoped<FileManager>();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapTus("/files", async httpContext => new()
{
    // This method is called on each request so different configurations can be returned per user, domain, path etc.
    // Return null to disable tusdotnet for the current request.

    // Where to store data?

    Store = new tusdotnet.Stores.TusDiskStore(Path.Combine(app.Environment.ContentRootPath, "Uploads")),
    MaxAllowedUploadSizeInBytes = 10000000,
    Events = new()
    {
        // What to do when file is completely uploaded?
        OnFileCompleteAsync = async eventContext =>
        {
            var fsm = httpContext.RequestServices.GetService<FileManager>();

            tusdotnet.Interfaces.ITusFile file = await eventContext.GetFileAsync();
          //  Dictionary<string, tusdotnet.Models.Metadata> metadata = await file.GetMetadataAsync(eventContext.CancellationToken);

            await fsm.StoreTus(file, eventContext.CancellationToken);
        }
    }
});

app.MapRazorPages();

app.Run();
