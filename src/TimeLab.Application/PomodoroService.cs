using TimeLab.Core;

namespace TimeLab.Application;

public class PomodoroService
{
    private readonly ISessionRepository _sessionRepo;
    private readonly TimerState _timerState = new();
    private DateTime _sessionStartTime;
    private Guid? _currentTaskId;

    public PomodoroService(ISessionRepository sessionRepo)
    {
        _sessionRepo = sessionRepo;
    }

    public TimerState GetTimerState() => _timerState;
    public Guid? CurrentTaskId => _currentTaskId;

    public void Start(Guid? taskId = null)
    {
        if (_timerState.Status == TimerStatus.Idle)
        {
            _timerState.ElapsedTime = TimeSpan.Zero;
            _sessionStartTime = DateTime.Now;
            _currentTaskId = taskId;
        }

        _timerState.Status = TimerStatus.Running;
        _timerState.StartTime = DateTime.Now;
    }

    public void Pause()
    {
        if (_timerState.Status != TimerStatus.Running) return;
        _timerState.Status = TimerStatus.Paused;
        _timerState.ElapsedTime += DateTime.Now - (_timerState.StartTime ?? DateTime.Now);
        _timerState.StartTime = null;
    }

    public async Task<IReadOnlyList<PomodoroSession>> GetAllSessionsAsync()
    {
        return await _sessionRepo.GetAllAsync();
    }

    public async Task<PomodoroSession> StopAsync()
    {
        var now = DateTime.Now;

        if (_timerState.Status == TimerStatus.Running)
        {
            _timerState.ElapsedTime += now - (_timerState.StartTime ?? now);
        }

        var session = new PomodoroSession
        {
            StartTime = _sessionStartTime,
            EndTime = now,
            Duration = _timerState.ElapsedTime,
            TaskId = _currentTaskId
        };

        await _sessionRepo.AddAsync(session);

        _timerState.Status = TimerStatus.Idle;
        _timerState.StartTime = null;
        _timerState.ElapsedTime = TimeSpan.Zero;
        _currentTaskId = null;

        return session;
    }
}
