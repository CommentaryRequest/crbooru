namespace CRbooru.Models;

public class PoolVersion
{
    public int Id { get; set; }
    public int UpdaterId { get; set; }
    public User Updater { get; set; } = null!;
    public int PoolId { get; set; }
    public Pool Pool { get; set; } = null!;
    public List<int>? NewState { get; set; }
    public string? NewName { get; set; }
    public string? NewDescription { get; set; }
    public bool DescriptionChanged { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
