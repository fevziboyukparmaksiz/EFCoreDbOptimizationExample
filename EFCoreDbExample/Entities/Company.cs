namespace EFCoreDbExample.Entities;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime? LastSalaryUpdate { get; set; }    

    //Nav Property
    public List<Employee> Employees { get; set; }
}
