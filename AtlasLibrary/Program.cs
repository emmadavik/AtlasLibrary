using AtlasLibrary.Services;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();


// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient("ItemsService", (serviceProvider, httpClient) =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    string? adress = config.GetValue<string>("ItemServiceAdress");

    if (string.IsNullOrWhiteSpace(adress))
    {
        throw new InvalidOperationException("ItemServiceAdress saknas i appsettings.");
    }

    httpClient.BaseAddress = new Uri(adress);
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
app.UseRouting();


app.UseAuthorization();


app.MapStaticAssets();


app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();




app.Run();