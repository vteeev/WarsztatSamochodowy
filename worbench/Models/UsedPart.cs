namespace worbench.Models;

public class UsedPart
{
    public int Id { get; set; }
    public int ServiceTaskId { get; set; }
    public int PartId { get; set; }
    public int Quantity { get; set; }

    public ServiceTask ServiceTask { get; set; }
    public Part Part { get; set; }
}