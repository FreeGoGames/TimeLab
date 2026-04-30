using TimeLab.Application;
using TimeLab.Core;

namespace TimeLab.App;

/// <summary>
/// Temporary in-memory repositories for MVP. Will be replaced by Infrastructure layer.
/// </summary>
public class InMemoryTaskRepository : ITaskRepository
{
    private readonly List<TaskItem> _tasks = new();

    public Task<IReadOnlyList<TaskItem>> GetAllAsync()
        => Task.FromResult((IReadOnlyList<TaskItem>)_tasks.AsReadOnly());

    public Task AddAsync(TaskItem task)
    {
        _tasks.Add(task);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TaskItem task)
        => Task.CompletedTask;

    public Task DeleteAsync(Guid id)
    {
        _tasks.RemoveAll(t => t.Id == id);
        return Task.CompletedTask;
    }
}

public class InMemorySessionRepository : ISessionRepository
{
    private readonly List<PomodoroSession> _sessions = new();

    public Task<IReadOnlyList<PomodoroSession>> GetAllAsync()
        => Task.FromResult((IReadOnlyList<PomodoroSession>)_sessions.AsReadOnly());

    public Task AddAsync(PomodoroSession session)
    {
        _sessions.Add(session);
        return Task.CompletedTask;
    }
}
