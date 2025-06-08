namespace worbench.Models;

public class Part
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal UnitPrice { get; set; }

    public ICollection<UsedPart> UsedParts { get; set; }
}