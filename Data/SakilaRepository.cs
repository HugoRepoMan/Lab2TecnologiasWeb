using System.Data;
using Microsoft.Data.SqlClient;
using SakilaWebMVC.Models;

namespace SakilaWebMVC.Data;

// Acceso SQL 100 % mediante ADO.NET; sin ORM ni Entity Framework.
public sealed class SakilaRepository
{
    private readonly string _connectionString;

    public SakilaRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection.");
    }

    private SqlConnection Connection() => new(_connectionString);
    private static void Param(SqlCommand command, string name, SqlDbType type, object value, int size = 0)
    {
        var parameter = command.Parameters.Add(name, type);
        if (size > 0) parameter.Size = size;
        parameter.Value = value;
    }

    public async Task<Dashboard> GetDashboardAsync()
    {
        await using var connection = Connection();
        await connection.OpenAsync();
        await using var command = new SqlCommand(
            "SELECT (SELECT COUNT(*) FROM dbo.country), (SELECT COUNT(*) FROM dbo.city)", connection);
        await using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        return new Dashboard { TotalCountries = reader.GetInt32(0), TotalCities = reader.GetInt32(1) };
    }

    public async Task<List<Country>> GetCountriesAsync()
    {
        const string sql = """
            SELECT co.country_id, co.country, co.last_update, COUNT(ci.city_id) AS city_count
            FROM dbo.country AS co
            LEFT JOIN dbo.city AS ci ON ci.country_id = co.country_id
            GROUP BY co.country_id, co.country, co.last_update
            ORDER BY co.country;
            """;
        var items = new List<Country>();
        await using var connection = Connection();
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) items.Add(ReadCountry(reader));
        return items;
    }

    private static Country ReadCountry(SqlDataReader r) => new()
    {
        CountryId = Convert.ToInt32(r["country_id"]),
        CountryName = Convert.ToString(r["country"]) ?? "",
        LastUpdate = Convert.ToDateTime(r["last_update"]),
        CityCount = Convert.ToInt32(r["city_count"])
    };

    public async Task<Country?> GetCountryAsync(int id)
    {
        const string sql = """
            SELECT co.country_id, co.country, co.last_update, COUNT(ci.city_id) AS city_count
            FROM dbo.country AS co
            LEFT JOIN dbo.city AS ci ON ci.country_id = co.country_id
            WHERE co.country_id = @id
            GROUP BY co.country_id, co.country, co.last_update;
            """;
        await using var connection = Connection();
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        Param(command, "@id", SqlDbType.Int, id);
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadCountry(reader) : null;
    }

    public async Task<int> InsertCountryAsync(Country country)
    {
        const string sql = "INSERT INTO dbo.country (country, last_update) VALUES (@name, GETDATE());";
        await using var connection = Connection();
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        Param(command, "@name", SqlDbType.NVarChar, country.CountryName.Trim(), 50);
        return await command.ExecuteNonQueryAsync();
    }

    public async Task<int> UpdateCountryAsync(Country country)
    {
        const string sql = "UPDATE dbo.country SET country = @name, last_update = GETDATE() WHERE country_id = @id;";
        await using var connection = Connection();
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        Param(command, "@name", SqlDbType.NVarChar, country.CountryName.Trim(), 50);
        Param(command, "@id", SqlDbType.Int, country.CountryId);
        return await command.ExecuteNonQueryAsync();
    }

    public async Task<int> DeleteCountryAsync(int id)
    {
        // La FK sigue activa: NO se eliminan países con ciudades asociadas.
        const string sql = "DELETE FROM dbo.country WHERE country_id = @id AND NOT EXISTS (SELECT 1 FROM dbo.city WHERE country_id = @id);";
        await using var connection = Connection();
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        Param(command, "@id", SqlDbType.Int, id);
        return await command.ExecuteNonQueryAsync();
    }

    private static City ReadCity(SqlDataReader r) => new()
    {
        CityId = Convert.ToInt32(r["city_id"]),
        CityName = Convert.ToString(r["city"]) ?? "",
        CountryId = Convert.ToInt32(r["country_id"]),
        CountryName = Convert.ToString(r["country"]) ?? "",
        LastUpdate = Convert.ToDateTime(r["last_update"])
    };

    private const string CitySelect = """
        SELECT ci.city_id, ci.city, ci.country_id, co.country, ci.last_update
        FROM dbo.city AS ci
        INNER JOIN dbo.country AS co ON ci.country_id = co.country_id
        """;

    public async Task<List<City>> GetCitiesAsync()
    {
        var items = new List<City>();
        await using var connection = Connection();
        await connection.OpenAsync();
        await using var command = new SqlCommand(CitySelect + " ORDER BY ci.city;", connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync()) items.Add(ReadCity(reader));
        return items;
    }

    public async Task<City?> GetCityAsync(int id)
    {
        await using var connection = Connection();
        await connection.OpenAsync();
        await using var command = new SqlCommand(CitySelect + " WHERE ci.city_id = @id;", connection);
        Param(command, "@id", SqlDbType.Int, id);
        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadCity(reader) : null;
    }

    public async Task<int> InsertCityAsync(City city)
    {
        const string sql = "INSERT INTO dbo.city (city, country_id, last_update) VALUES (@city, @countryId, GETDATE());";
        await using var connection = Connection();
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        Param(command, "@city", SqlDbType.NVarChar, city.CityName.Trim(), 50);
        Param(command, "@countryId", SqlDbType.Int, city.CountryId);
        return await command.ExecuteNonQueryAsync();
    }

    public async Task<int> UpdateCityAsync(City city)
    {
        const string sql = "UPDATE dbo.city SET city = @city, country_id = @countryId, last_update = GETDATE() WHERE city_id = @id;";
        await using var connection = Connection();
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        Param(command, "@city", SqlDbType.NVarChar, city.CityName.Trim(), 50);
        Param(command, "@countryId", SqlDbType.Int, city.CountryId);
        Param(command, "@id", SqlDbType.Int, city.CityId);
        return await command.ExecuteNonQueryAsync();
    }

    public async Task<int> DeleteCityAsync(int id)
    {
        const string sql = "DELETE FROM dbo.city WHERE city_id = @id;";
        await using var connection = Connection();
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        Param(command, "@id", SqlDbType.Int, id);
        return await command.ExecuteNonQueryAsync();
    }
}
