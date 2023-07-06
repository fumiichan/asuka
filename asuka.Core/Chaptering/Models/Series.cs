namespace asuka.Core.Chaptering.Models;

public record Series
{
    public string Output { get; set; }
    public IReadOnlyList<Chapter> Chapters { get; set; }
}
