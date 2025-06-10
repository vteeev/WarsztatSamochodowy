namespace worbench.Models;

using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Diagnostics;

public class ApplicationUser : IdentityUser<int>
{
    public ICollection<ServiceOrder> AssignedServiceOrders { get; set; }
    public ICollection<Comment> Comments { get; set; }
}

public class ApplicationRole : IdentityRole<int>
{
}