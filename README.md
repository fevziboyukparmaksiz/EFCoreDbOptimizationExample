

# Update Salary Approaches: Entity Framework vs. Raw SQL

This document compares two approaches to increasing salaries for employees in a company using Entity Framework and raw SQL.

## First Approach: Entity Framework

1. **Run 1000 Separate `UPDATE` Commands:**
   - Each employee's salary is updated individually, resulting in 1000 separate `UPDATE` commands being sent to the database.

2. **Performance Consideration:**
   - This process is slower because each update is sent separately, causing more overhead and latency.

### Code Example
```csharp
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
```
###
## Second Approach: Raw SQL

1. **Run a Single `UPDATE` Command Directly in the Database:**
   - Executes a single `UPDATE` statement to increase the salary of all employees in a specified company.

2. **Performance Consideration:**
   - This approach is significantly faster because a single command updates all relevant records, minimizing overhead and latency.

   
### Code Example
```csharp
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







