using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSession();

var app = builder.Build();

// MySQL Connection Test (before app starts)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

try
{
    using var connection = new MySqlConnection(connectionString);
    connection.Open();
    Console.WriteLine("✅ Connected to MySQL database!");
}
catch (Exception ex)
{
    Console.WriteLine("❌ Database connection failed:");
    Console.WriteLine(ex.Message);
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}"
);

app.Run();
