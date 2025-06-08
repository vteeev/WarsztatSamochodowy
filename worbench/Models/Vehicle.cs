using System.ComponentModel.DataAnnotations;

namespace worbench.Models;

public class Vehicle
{
    public int Id { get; set; }

    [Required]
    public string Make { get; set; }

    [Required]
    public string Model { get; set; }

    [Range(1900, 2100)]
    public int Year { get; set; }

    [Required]
    public string VIN { get; set; }

    [Required]
    public string RegistrationNumber { get; set; }

    public string ImageUrl { get; set; }

    public int CustomerId { get; set; }

    public Customer? Customer { get; set; }

}
