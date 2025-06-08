using System.Collections.Generic;

namespace worbench.DTOs;

public class ServiceOrderDto
{
    public int Id { get; set; }
    public string VehicleInfo { get; set; }
    public string Status { get; set; }
    public decimal TotalCost { get; set; }
    public List<ServiceTaskDto> Tasks { get; set; }
    public string? MechanicEmail { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ServiceTaskDto
{
    public string Description { get; set; }
    public decimal LaborCost { get; set; }
    public List<PartDto> Parts { get; set; }
}

public class PartDto
{
    public string Name { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
} 