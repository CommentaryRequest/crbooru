namespace CRbooru.Tests;

public sealed class TemporaryDirectory : IDisposable
{
    public string DirectoryPath { get; } = Path.Combine(Path.GetTempPath(), $"crbooru-{Guid.NewGuid():N}");

    public TemporaryDirectory()
    {
        Directory.CreateDirectory(DirectoryPath);
    }

    public string GetPath(string filename) => Path.Combine(DirectoryPath, filename);

    public void Dispose()
    {
        if (Directory.Exists(DirectoryPath)) {
            Directory.Delete(DirectoryPath, true);
        }
    }
}
