namespace worbench.Models;

public class ServiceTask
{
    public int Id { get; set; }
    public int ServiceOrderId { get; set; }
    public string Description { get; set; }
    public decimal LaborCost { get; set; }

    public ServiceOrder ServiceOrder { get; set; }
    public ICollection<UsedPart> UsedParts { get; set; }
}