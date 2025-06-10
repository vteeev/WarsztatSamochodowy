namespace worbench.Models;
using System.ComponentModel.DataAnnotations;

public class Customer
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Imię jest wymagane")]
    public string FirstName { get; set; } = default!;
    [Required(ErrorMessage = "Nazwisko jest wymagane")]
    public string LastName { get; set; } = default!;
    [Required(ErrorMessage = "Email jest wymagany")]
    [EmailAddress(ErrorMessage = "Nieprawidłowy adres email")]
    public string Email { get; set; } = default!;
    [Required(ErrorMessage = "Telefon jest wymagany")]
    public string Phone { get; set; } = default!;
    [Required(ErrorMessage = "Adres jest wymagany")]
    public string Address { get; set; } = default!;
    
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public int UserId { get; set; }
    // Jeśli potrzebujesz nawigacji:
    // public ApplicationUser User { get; set; }
}