using EFCoreDbExample;
using EFCoreDbExample.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Database"));
});


var app = builder.Build();

#region Naive approach
app.MapPut("increase-salaries", async (int companyId, AppDbContext context) =>
{
    var company = await context
        .Set<Company>()
        .Include(c => c.Employees)
        .FirstOrDefaultAsync(c => c.Id == companyId);

    if (company is null)
    {
        return Results.NotFound($"The Company with Id '{companyId}' was not found");
    }

    foreach (var employee in company.Employees)
    {
        employee.Salary *= 1.1m;
    }

    company.LastSalaryUpdate = DateTime.Now;
    await context.SaveChangesAsync();

    return Results.NoContent();
});

/*output
UPDATE [Employees] SET [Salary] = @p0
OUTPUT 1
WHERE [Id] = @p1;
UPDATE [Employees] SET [Salary] = @p2
OUTPUT 1
WHERE [Id] = @p3;
UPDATE [Employees] SET [Salary] = @p4
OUTPUT 1
WHERE [Id] = @p5;
UPDATE [Employees] SET [Salary] = @p6
OUTPUT 1
WHERE [Id] = @p7; .............

Until 1000*/


#endregion

#region using sql => better approach
app.MapPut("increase-salaries-sql", async (int companyId, AppDbContext context) =>
{
    var company = await context
        .Set<Company>()
        .Include(c => c.Employees)
        .FirstOrDefaultAsync(c => c.Id == companyId);

    if (company is null)
    {
        return Results.NotFound($"The Company with Id '{companyId}' was not found");
    }
    
    await context.Database.BeginTransactionAsync();

    await context.Database.ExecuteSqlInterpolatedAsync(
        $"UPDATE Employees SET Salary = Salary * 1.1 Where CompanyId = {companyId}");

    company.LastSalaryUpdate = DateTime.Now;

    await context.SaveChangesAsync();
    
    await context.Database.CommitTransactionAsync();

    return Results.NoContent();
});

/*Output
UPDATE [Companies] SET [LastSalaryUpdate] = @p0
OUTPUT 1
WHERE [Id] = @p1 
*/;

#endregion




if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
