using Microsoft.AspNetCore.Mvc;
using GuestbookApp.Models;
using MySql.Data.MySqlClient;

namespace GuestbookApp.Controllers;

public class GuestbookController : Controller
{
    private readonly IConfiguration _config;

    public GuestbookController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet]
    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("email") == null)
            return RedirectToAction("Login", "Auth");

        return View();
    }

    [HttpPost]
    public IActionResult Add(string title, string comment, int rating)
    {
        var userId = HttpContext.Session.GetInt32("user_id");
        var email = HttpContext.Session.GetString("email");

        if (userId == null || email == null)
            return Unauthorized();

        if (title.Length < 3 || comment.Length < 5)
            return BadRequest("Validation failed");

        var connectionString = _config.GetConnectionString("DefaultConnection");
        using var conn = new MySqlConnection(connectionString);
        conn.Open();

        var cmd = new MySqlCommand("INSERT INTO entries (author_email, title, comment, rating, user_id) VALUES (@e, @t, @c, @r, @uid)", conn);
        cmd.Parameters.AddWithValue("@e", email);
        cmd.Parameters.AddWithValue("@t", title);
        cmd.Parameters.AddWithValue("@c", comment);
        cmd.Parameters.AddWithValue("@r", rating);
        cmd.Parameters.AddWithValue("@uid", userId);

        cmd.ExecuteNonQuery();

        return RedirectToAction("Index");
    }

    [HttpGet]
[HttpGet]
public JsonResult Fetch(int page = 1, string groupBy = "none", string sortBy = "newest")
{
    var entries = new List<GuestbookEntry>();
    var connectionString = _config.GetConnectionString("DefaultConnection");

    using var conn = new MySqlConnection(connectionString);
    conn.Open();

    int pageSize = 4;
    int offset = (page - 1) * pageSize;

    var query = "SELECT * FROM entries";

    // Sorting (only in SQL)
    query += sortBy switch
    {
        "rating" => " ORDER BY rating DESC",
        "oldest" => " ORDER BY date_created ASC",
        _ => " ORDER BY date_created DESC"
    };

    query += " LIMIT @limit OFFSET @offset";

    var cmd = new MySqlCommand(query, conn);
    cmd.Parameters.AddWithValue("@limit", pageSize);
    cmd.Parameters.AddWithValue("@offset", offset);

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        entries.Add(new GuestbookEntry
        {
            Id = Convert.ToInt32(reader["id"]),
            AuthorEmail = reader["author_email"].ToString()!,
            Title = reader["title"].ToString()!,
            Comment = reader["comment"].ToString()!,
            Rating = Convert.ToInt32(reader["rating"]),
            DateCreated = Convert.ToDateTime(reader["date_created"]),
            UserId = Convert.ToInt32(reader["user_id"])
        });
    }

    // Grouping in memory
if (groupBy == "title")
{
    entries = entries
        .GroupBy(e => e.Title)
        .Select(g => g.OrderByDescending(e => e.DateCreated).First())
        .ToList();
}
else if (groupBy == "author")
{
    entries = entries
        .GroupBy(e => e.AuthorEmail)
        .Select(g => g.OrderByDescending(e => e.DateCreated).First())
        .ToList();
}

// Apply final sort to grouped result
entries = sortBy switch
{
    "rating" => entries.OrderByDescending(e => e.Rating).ToList(),
    "oldest" => entries.OrderBy(e => e.DateCreated).ToList(),
    _ => entries.OrderByDescending(e => e.DateCreated).ToList()
};


    return Json(entries);
}



    [HttpGet]
    public IActionResult Edit(int id)
    {
        if (HttpContext.Session.GetString("role") != "admin")
            return Unauthorized();

        var connStr = _config.GetConnectionString("DefaultConnection");
        using var conn = new MySqlConnection(connStr);
        conn.Open();

        var cmd = new MySqlCommand("SELECT * FROM entries WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            var entry = new GuestbookEntry
            {
                Id = Convert.ToInt32(reader["id"]),
                AuthorEmail = reader["author_email"].ToString()!,
                Title = reader["title"].ToString()!,
                Comment = reader["comment"].ToString()!,
                Rating = Convert.ToInt32(reader["rating"]),
                DateCreated = Convert.ToDateTime(reader["date_created"]),
                UserId = Convert.ToInt32(reader["user_id"])
            };

            return View(entry);
        }

        return NotFound();
    }

    [HttpPost]
    public IActionResult Edit(int id, string title, string comment, int rating)
    {
        if (HttpContext.Session.GetString("role") != "admin")
            return Unauthorized();

        if (title.Length < 3 || comment.Length < 5)
            return BadRequest("Validation failed");

        var connStr = _config.GetConnectionString("DefaultConnection");
        using var conn = new MySqlConnection(connStr);
        conn.Open();

        var cmd = new MySqlCommand("UPDATE entries SET title = @t, comment = @c, rating = @r WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@t", title);
        cmd.Parameters.AddWithValue("@c", comment);
        cmd.Parameters.AddWithValue("@r", rating);

        cmd.ExecuteNonQuery();

        return RedirectToAction("Index");
    }
}
