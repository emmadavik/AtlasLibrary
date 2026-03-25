using Microsoft.EntityFrameworkCore;
using items.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<ItemsDbContext>(options =>
    options.UseSqlite("Data Source=items.db"));

builder.Services.AddOpenApi();

builder.Services.AddHttpClient("adminApi", client =>
{
    client.BaseAddress = new Uri("https://atlaslibraryitemsobject-gddwfucvfuetbmbe.swedencentral-01.azurewebsites.net/");
});

var app = builder.Build();

//if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ItemsDbContext>();
    db.Database.EnsureCreated();
}

app.Run();