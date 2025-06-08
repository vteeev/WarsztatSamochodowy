namespace worbench.Models;

using System;

public class ServiceOrder
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; }
    public int? AssignedMechanicId { get; set; }

    public Vehicle Vehicle { get; set; }
    public ApplicationUser AssignedMechanic { get; set; }
    public ICollection<ServiceTask> ServiceTasks { get; set; }
    public ICollection<Comment> Comments { get; set; }
}