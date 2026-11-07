namespace CRbooru.Models;

public class TagImplication
{
    public int Id { get; set; }
    public Tag Antecedent { get; set; }
    public int AntecedentId { get; set; }
    public Tag Consequent { get; set; }
    public int ConsequentId { get; set; }
}
