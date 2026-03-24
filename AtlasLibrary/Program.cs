var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// HttpClient f�r UsersApi
builder.Services.AddHttpClient("UsersApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:UsersApiBaseUrl"]!);
});

// HttpClient f�r LoansApi
builder.Services.AddHttpClient("LoansApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:LoansApiBaseUrl"]!);
});

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();


builder.Services.AddHttpClient("itemsApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5079/");
});






var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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
