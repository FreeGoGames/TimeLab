using TimeLab.Core;

namespace TimeLab.Application;

public interface ISessionRepository
{
    Task<IReadOnlyList<PomodoroSession>> GetAllAsync();
    Task AddAsync(PomodoroSession session);
}
