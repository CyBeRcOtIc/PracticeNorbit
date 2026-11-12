using Microsoft.Data.SqlClient;

namespace DbCrudConsole;

public class AdoGameRepository : IGameRepository
{
    private readonly string _connectionString;

    public AdoGameRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private const string SelectColumns =
        "GameID, Title, Description, Price, ReleaseDate, IsAvailable";

    public void Create(Game game)
    {
        const string sql = @"
            INSERT INTO Games (Title, Description, Price, ReleaseDate, IsAvailable)
            VALUES (@Title, @Description, @Price, @ReleaseDate, @IsAvailable)";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        AddParameters(command, game);

        connection.Open();
        command.ExecuteNonQuery();
    }
    public List<Game> GetAll()
    {
        var games = new List<Game>();

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand($"SELECT {SelectColumns} FROM Games", connection);

        connection.Open();
        using var reader = command.ExecuteReader();
        while (reader.Read())
            games.Add(MapGame(reader));

        return games;
    }

    public Game? GetById(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(
            $"SELECT {SelectColumns} FROM Games WHERE GameID = @Id", connection);
        command.Parameters.AddWithValue("@Id", id);

        connection.Open();
        using var reader = command.ExecuteReader();
        return reader.Read() ? MapGame(reader) : null;
    }
    public void Update(Game game)
    {
        const string sql = @"
            UPDATE Games
            SET Title = @Title, Description = @Description, Price = @Price,
                ReleaseDate = @ReleaseDate, IsAvailable = @IsAvailable
            WHERE GameID = @Id";

        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand(sql, connection);
        AddParameters(command, game);
        command.Parameters.AddWithValue("@Id", game.GameID);

        connection.Open();
        command.ExecuteNonQuery();
    }
    public void Delete(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("DELETE FROM Games WHERE GameID = @Id", connection);
        command.Parameters.AddWithValue("@Id", id);

        connection.Open();
        command.ExecuteNonQuery();
    }

    private static Game MapGame(SqlDataReader reader)
    {
        int descIndex = reader.GetOrdinal("Description");
        int dateIndex = reader.GetOrdinal("ReleaseDate");

        return new Game
        {
            GameID = reader.GetInt32(reader.GetOrdinal("GameID")),
            Title = reader.GetString(reader.GetOrdinal("Title")),
            Description = reader.IsDBNull(descIndex) ? null : reader.GetString(descIndex),
            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
            ReleaseDate = reader.IsDBNull(dateIndex) ? null : reader.GetDateTime(dateIndex),
            IsAvailable = reader.GetBoolean(reader.GetOrdinal("IsAvailable"))
        };
    }

    private static void AddParameters(SqlCommand command, Game game)
    {
        command.Parameters.AddWithValue("@Title", game.Title);
        command.Parameters.AddWithValue("@Description", (object?)game.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@Price", game.Price);
        command.Parameters.AddWithValue("@ReleaseDate", (object?)game.ReleaseDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsAvailable", game.IsAvailable);
    }
}
    