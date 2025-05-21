using Microsoft.AspNetCore.Mvc;
using GuestbookApp.Models;
using MySql.Data.MySqlClient;

namespace GuestbookApp.Controllers;

public class AuthController : Controller
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Login(string email, string password)
    {
        var connectionString = _config.GetConnectionString("DefaultConnection");
        using var connection = new MySqlConnection(connectionString);
        connection.Open();

        var command = new MySqlCommand("SELECT * FROM users WHERE email = @email", connection);
        command.Parameters.AddWithValue("@email", email);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            var storedHash = reader["password_hash"].ToString()!;
            if (storedHash == password) // For real apps, use BCrypt. For now, match plain text.
            {
                HttpContext.Session.SetString("email", email);
                HttpContext.Session.SetInt32("user_id", Convert.ToInt32(reader["id"]));
                var isAdmin = email.EndsWith("@admin.com");
                HttpContext.Session.SetString("role", isAdmin ? "admin" : "user");
                return RedirectToAction("Index", "Guestbook");
            }
        }

        ViewBag.Error = "Invalid credentials";
        return View();
    }
    [HttpGet]
public IActionResult Register()
{
    return View();
}

[HttpPost]
public IActionResult Register(string email, string password)
{
    var connectionString = _config.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
    {
        ViewBag.Error = "Email and password are required.";
        return View();
    }

    using var connection = new MySqlConnection(connectionString);
    connection.Open();

    // Check if email already exists
    var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM users WHERE email = @e", connection);
    checkCmd.Parameters.AddWithValue("@e", email);
    var exists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;

    if (exists)
    {
        ViewBag.Error = "Email already registered.";
        return View();
    }

    // Store user
    var insertCmd = new MySqlCommand("INSERT INTO users (email, password_hash) VALUES (@e, @p)", connection);
    insertCmd.Parameters.AddWithValue("@e", email);
    insertCmd.Parameters.AddWithValue("@p", password); // For this assignment, plaintext is acceptable

    insertCmd.ExecuteNonQuery();

    return RedirectToAction("Login");
}


    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
