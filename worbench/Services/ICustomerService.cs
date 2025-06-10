namespace worbench.Services;
using worbench.Models;

public interface ICustomerService
{
    Task<Customer> GetCustomerByIdAsync(int id);  // Pobierz klienta po ID
    Task<List<Customer>> GetAllCustomersAsync();  // Dodajemy tę metodę
    Task AddCustomerAsync(Customer customer);     // Dodaj nowego klienta
    Task<List<Vehicle>> GetVehiclesByCustomerIdAsync(int customerId); // Pobierz pojazdy klienta po ID
}