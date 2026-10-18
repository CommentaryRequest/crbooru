namespace CRbooru.Models;

public class Pool
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<int> PostIds { get; set; } = [];
}
