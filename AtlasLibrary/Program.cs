using AtlasLibrary.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// HttpClient för UsersApi
builder.Services.AddHttpClient("UsersApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:UsersApiBaseUrl"]!);
});

// HttpClient för LoansApi
builder.Services.AddHttpClient("LoansApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:LoansApiBaseUrl"]!);
});

builder.Services.AddHttpClient("ItemsService", client =>
{
    //client.BaseAddress = new Uri(builder.Configuration["ApiSettings:ItemServiceAdress"]!);
    client.BaseAddress = new Uri("http://localhost:5079/");
});


builder.Services.AddHttpClient("equipmentItemsApi", client =>
{
    client.BaseAddress = new Uri("https://atlaslibraryitemsobject-gddwfucvfuetbmbe.swedencentral-01.azurewebsites.net/");
});

//builder.Services.AddHttpClient("ItemsService", (serviceProvider, httpClient) =>
//{
//    var config = serviceProvider.GetRequiredService<IConfiguration>();
//    string? adress = config.GetValue<string>("ApiSettings:ItemServiceAdress");

//    if (string.IsNullOrWhiteSpace(adress))
//    {
//        throw new InvalidOperationException("ItemServiceAdress saknas i appsettings.");
//    }

//    httpClient.BaseAddress = new Uri(adress);
//});

builder.Services.AddScoped<ItemsService>();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();