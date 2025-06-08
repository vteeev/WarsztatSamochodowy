namespace worbench.Models;

using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Diagnostics;

public class ApplicationUser : IdentityUser<int>
{
    // Możesz dodać dodatkowe pola, np.:
    // public string FirstName { get; set; }
    // public string LastName { get; set; }

    public ICollection<ServiceOrder> AssignedServiceOrders { get; set; }
    public ICollection<Comment> Comments { get; set; }
}

public class ApplicationRole : IdentityRole<int>
{
}