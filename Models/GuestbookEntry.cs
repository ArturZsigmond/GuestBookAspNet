namespace GuestbookApp.Models;

public class GuestbookEntry
{
    public int Id { get; set; }
    public string AuthorEmail { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public DateTime DateCreated { get; set; }
    public int UserId { get; set; }
    public int Rating { get; set; }
}
