namespace DbCrudConsole;

public interface IGameRepository
{
    void Create(Game game);
    List<Game> GetAll();
    Game? GetById(int id);
    void Update(Game game);
    void Delete(int id);
}