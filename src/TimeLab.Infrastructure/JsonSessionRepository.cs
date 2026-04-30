using System.Text.Json;
using TimeLab.Application;
using TimeLab.Core;

namespace TimeLab.Infrastructure;

public class JsonSessionRepository : ISessionRepository
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private readonly string _filePath;
    private List<PomodoroSession> _cache = new();

    public JsonSessionRepository()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TimeLab");
        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, "sessions.json");
        Load();
    }

    private void Load()
    {
        if (!File.Exists(_filePath)) return;
        var json = File.ReadAllText(_filePath);
        _cache = JsonSerializer.Deserialize<List<PomodoroSession>>(json) ?? new();
    }

    private async Task SaveAsync()
    {
        var json = JsonSerializer.Serialize(_cache, Options);
        await File.WriteAllTextAsync(_filePath, json);
    }

    public Task<IReadOnlyList<PomodoroSession>> GetAllAsync()
        => Task.FromResult((IReadOnlyList<PomodoroSession>)_cache.AsReadOnly());

    public async Task AddAsync(PomodoroSession session)
    {
        _cache.Add(session);
        await SaveAsync();
    }
}
