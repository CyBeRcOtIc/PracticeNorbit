namespace DbCrudConsole;

public class EfGameRepository : IGameRepository
{
    private readonly string _connectionString;

    public EfGameRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void Create(Game game)
    {
        using var db = new ShopDbContext(_connectionString);
        db.Games.Add(game);
        db.SaveChanges();
    }

    public List<Game> GetAll()
    {
        using var db = new ShopDbContext(_connectionString);
        return db.Games.ToList();
    }

    public Game? GetById(int id)
    {
        using var db = new ShopDbContext(_connectionString);
        return db.Games.Find(id);
    }

    public void Update(Game game)
    {
        using var db = new ShopDbContext(_connectionString);
        db.Games.Update(game);
        db.SaveChanges();
    }

    public void Delete(int id)
    {
        using var db = new ShopDbContext(_connectionString);
        var game = db.Games.Find(id);
        if (game == null) return;

        db.Games.Remove(game);
        db.SaveChanges();
    }
}