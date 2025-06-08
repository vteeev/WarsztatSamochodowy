using Riok.Mapperly.Abstractions;
using worbench.Models;
using worbench.DTOs;
using System.Linq;
using System.Collections.Generic;

namespace worbench.Mappers;

[Mapper]
public static partial class ServiceOrderMapper
{
    [MapperIgnoreTarget(nameof(ServiceOrderDto.VehicleInfo))]
    [MapperIgnoreTarget(nameof(ServiceOrderDto.TotalCost))]
    [MapperIgnoreTarget(nameof(ServiceOrderDto.Tasks))]
    public static partial ServiceOrderDto ToDto(ServiceOrder order);

    [MapperIgnoreTarget(nameof(ServiceTaskDto.Parts))]
    public static partial ServiceTaskDto ToDto(ServiceTask task);

    [MapperIgnoreTarget(nameof(PartDto.Name))]
    [MapperIgnoreTarget(nameof(PartDto.UnitPrice))]
    public static partial PartDto ToDto(UsedPart part);

    public static ServiceOrderDto ToDtoWithCustom(ServiceOrder order)
    {
        var dto = ToDto(order);
        dto.VehicleInfo = $"{order.Vehicle?.Make} {order.Vehicle?.Model} ({order.Vehicle?.RegistrationNumber})";
        dto.Tasks = order.ServiceTasks?.Select(ToDtoWithCustom).ToList() ?? new List<ServiceTaskDto>();
        dto.TotalCost = dto.Tasks.Sum(t => t.LaborCost + t.Parts.Sum(p => p.UnitPrice * p.Quantity));
        dto.MechanicEmail = order.AssignedMechanic?.Email;
        dto.CreatedAt = order.CreatedAt;
        return dto;
    }

    public static ServiceTaskDto ToDtoWithCustom(ServiceTask task)
    {
        var dto = ToDto(task);
        dto.Parts = task.UsedParts?.Select(ToDtoWithCustom).ToList() ?? new List<PartDto>();
        return dto;
    }

    public static PartDto ToDtoWithCustom(UsedPart part)
    {
        var dto = ToDto(part);
        dto.Name = part.Part?.Name ?? string.Empty;
        dto.UnitPrice = part.Part?.UnitPrice ?? 0;
        return dto;
    }
} 