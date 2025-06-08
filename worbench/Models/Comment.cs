namespace worbench.Models;

using System;

public class Comment
{
    public int Id { get; set; }
    public int ServiceOrderId { get; set; }
    public int AuthorId { get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }

    public ServiceOrder ServiceOrder { get; set; }
    public ApplicationUser Author { get; set; }
}