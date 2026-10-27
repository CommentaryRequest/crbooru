namespace CRbooru.Models;

public class PoolVersionsViewModel
{
    public List<PoolVersion> Versions { get; set; }
    public Pool? Pool { get; set; }
    public User? User { get; set; }
}
