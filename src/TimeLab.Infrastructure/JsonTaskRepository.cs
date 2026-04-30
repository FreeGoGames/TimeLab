using System.Text.Json;
using TimeLab.Application;
using TimeLab.Core;

namespace TimeLab.Infrastructure;

public class JsonTaskRepository : ITaskRepository
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private readonly string _filePath;
    private List<TaskItem> _cache = new();

    public JsonTaskRepository()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TimeLab");
        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, "tasks.json");
        Load();
    }

    private void Load()
    {
        if (!File.Exists(_filePath)) return;
        var json = File.ReadAllText(_filePath);
        _cache = JsonSerializer.Deserialize<List<TaskItem>>(json) ?? new();
    }

    private async Task SaveAsync()
    {
        var json = JsonSerializer.Serialize(_cache, Options);
        await File.WriteAllTextAsync(_filePath, json);
    }

    public Task<IReadOnlyList<TaskItem>> GetAllAsync()
        => Task.FromResult((IReadOnlyList<TaskItem>)_cache.AsReadOnly());

    public async Task AddAsync(TaskItem task)
    {
        _cache.Add(task);
        await SaveAsync();
    }

    public async Task UpdateAsync(TaskItem task)
    {
        await SaveAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        _cache.RemoveAll(t => t.Id == id);
        await SaveAsync();
    }
}
