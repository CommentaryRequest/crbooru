namespace CRbooru.Tests;

public static class FileFixture
{
    public static string GetFixturePath(string filename)
    {
        return Path.Combine(AppContext.BaseDirectory, "Fixtures", filename);
    }
}
