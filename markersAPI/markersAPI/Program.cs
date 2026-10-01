using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Application.DI;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MarkersDBContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = services.GetRequiredService<UserManager<UserEntity>>();

        await context.Database.MigrateAsync();

        foreach (var role in new[] { "Admin", "user" })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));


        var adminEmail = builder.Configuration["AdminSeed:Email"] ?? "admin";
        var adminPassword = builder.Configuration["AdminSeed:Password"] ?? "admin";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new UserEntity
            {
                UserName = "Admin",
                Email = adminEmail,
                NormalizedEmail = adminEmail.ToUpper(),
                NormalizedUserName = "ADMIN",

            };
            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }


    }
    catch (Exception ex)
    {
        services.GetRequiredService<ILogger<Program>>()
                .LogError(ex, "An error occurred while updating the database.");
    }
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
