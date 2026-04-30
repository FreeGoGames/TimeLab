using TimeLab.Core;

namespace TimeLab.Application;

public class TaskService
{
    private readonly ITaskRepository _taskRepo;

    public TaskService(ITaskRepository taskRepo)
    {
        _taskRepo = taskRepo;
    }

    public async Task<IReadOnlyList<TaskItem>> GetAllTasksAsync()
    {
        return await _taskRepo.GetAllAsync();
    }

    public async Task<TaskItem> CreateTaskAsync(string title)
    {
        var task = new TaskItem { Title = title };
        await _taskRepo.AddAsync(task);
        return task;
    }

    public async Task CompleteTaskAsync(Guid id)
    {
        var tasks = await _taskRepo.GetAllAsync();
        var task = tasks.FirstOrDefault(t => t.Id == id);
        if (task is null || task.IsCompleted) return;

        task.IsCompleted = true;
        task.CompletedAt = DateTime.Now;
        await _taskRepo.UpdateAsync(task);
    }

    public async Task DeleteTaskAsync(Guid id)
    {
        await _taskRepo.DeleteAsync(id);
    }
}
