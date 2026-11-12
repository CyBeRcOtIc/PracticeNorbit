namespace DbCrudConsole;

public class Game
{
    public int GameID { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public bool IsAvailable { get; set; }
}