using Microsoft.Data.SqlClient;
using ProyectoU1.Helpers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

SeedUsuarios(app.Configuration);

app.Run();

static void SeedUsuarios(IConfiguration configuration)
{
    string connectionString = configuration.GetConnectionString("SakilaConnection")!;

    var usuarios = new[]
    {
        new { Nombre = "Administrador", Email = "admin@sakila.com",
              Password = "Admin1234",   Rol = "Administrador",
              CustomerId = (int?)null,  StaffId = (int?)1 },

        new { Nombre = "Mary Smith",    Email = "customer@sakila.com",
              Password = "Customer1234", Rol = "Cliente",
              CustomerId = (int?)1,     StaffId = (int?)null },
    };

    using var connection = new SqlConnection(connectionString);

    try
    {
        connection.Open();
    }
    catch
    {
        return;
    }

    foreach (var u in usuarios)
    {
        const string sqlExiste = "SELECT COUNT(1) FROM Usuarios WHERE email = @Email";
        using var cmdExiste = new SqlCommand(sqlExiste, connection);
        cmdExiste.Parameters.AddWithValue("@Email", u.Email);
        int existe = (int)cmdExiste.ExecuteScalar();

        if (existe > 0)
            continue;

        const string sqlInsert = @"
            INSERT INTO Usuarios (nombre, email, password_hash, rol, customer_id, staff_id, activo)
            VALUES (@Nombre, @Email, @PasswordHash, @Rol, @CustomerId, @StaffId, 1)";

        using var cmdInsert = new SqlCommand(sqlInsert, connection);
        cmdInsert.Parameters.AddWithValue("@Nombre",       u.Nombre);
        cmdInsert.Parameters.AddWithValue("@Email",        u.Email);
        cmdInsert.Parameters.AddWithValue("@PasswordHash", SecurityHelper.HashPassword(u.Password));
        cmdInsert.Parameters.AddWithValue("@Rol",          u.Rol);
        cmdInsert.Parameters.AddWithValue("@CustomerId",
            u.CustomerId.HasValue ? (object)u.CustomerId.Value : DBNull.Value);
        cmdInsert.Parameters.AddWithValue("@StaffId",
            u.StaffId.HasValue ? (object)u.StaffId.Value : DBNull.Value);

        cmdInsert.ExecuteNonQuery();
    }
}
