using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using TimeLab.Application;
using TimeLab.Core;

namespace TimeLab.App.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly TaskService _taskService;
    private readonly PomodoroService _pomodoroService;
    private readonly DispatcherTimer _timer;

    public MainViewModel(TaskService taskService, PomodoroService pomodoroService)
    {
        _taskService = taskService;
        _pomodoroService = pomodoroService;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.1) };
        _timer.Tick += OnTimerTick;
        _timer.Start();

        AddTaskCommand = new RelayCommand(async () => await AddTaskAsync(), () => !string.IsNullOrWhiteSpace(NewTaskTitle));
        CompleteTaskCommand = new RelayCommand<TaskItem>(async t => await CompleteTaskAsync(t!));
        DeleteTaskCommand = new RelayCommand<TaskItem>(async t => await DeleteTaskAsync(t!));
        StartCommand = new RelayCommand(Start, () => Status == TimerStatus.Idle || Status == TimerStatus.Paused);
        PauseCommand = new RelayCommand(Pause, () => Status == TimerStatus.Running);
        StopCommand = new RelayCommand(async () => await StopAsync(), () => Status != TimerStatus.Idle);

        _ = LoadAsync();
    }

    // ── Todo ──

    public ObservableCollection<TaskItem> Tasks { get; } = new();
    public ObservableCollection<TaskItem> ActiveTasks { get; } = new();

    private TaskItem? _selectedTask;
    public TaskItem? SelectedTask
    {
        get => _selectedTask;
        set => Set(ref _selectedTask, value);
    }

    private string _newTaskTitle = string.Empty;
    public string NewTaskTitle
    {
        get => _newTaskTitle;
        set
        {
            if (Set(ref _newTaskTitle, value))
                ((RelayCommand)AddTaskCommand).RaiseCanExecuteChanged();
        }
    }

    public ICommand AddTaskCommand { get; }
    public ICommand CompleteTaskCommand { get; }
    public ICommand DeleteTaskCommand { get; }

    private async Task AddTaskAsync()
    {
        await _taskService.CreateTaskAsync(NewTaskTitle);
        NewTaskTitle = string.Empty;
        await RefreshTasksAsync();
    }

    private async Task CompleteTaskAsync(TaskItem task)
    {
        await _taskService.CompleteTaskAsync(task.Id);
        await RefreshTasksAsync();
    }

    private async Task DeleteTaskAsync(TaskItem task)
    {
        await _taskService.DeleteTaskAsync(task.Id);
        await RefreshTasksAsync();
    }

    // ── Timer ──

    private TimerStatus _status = TimerStatus.Idle;
    public TimerStatus Status
    {
        get => _status;
        set
        {
            if (Set(ref _status, value))
            {
                OnPropertyChanged(nameof(IsRunning));
                OnPropertyChanged(nameof(StatusText));
                ((RelayCommand)StartCommand).RaiseCanExecuteChanged();
                ((RelayCommand)PauseCommand).RaiseCanExecuteChanged();
                ((RelayCommand)StopCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsRunning => Status == TimerStatus.Running;

    public string StatusText => Status switch
    {
        TimerStatus.Idle => "空闲",
        TimerStatus.Running => "运行中",
        TimerStatus.Paused => "已暂停",
        _ => ""
    };

    private string _elapsedTimeDisplay = "00:00:00";
    public string ElapsedTimeDisplay
    {
        get => _elapsedTimeDisplay;
        set => Set(ref _elapsedTimeDisplay, value);
    }

    private string _linkedTaskTitle = string.Empty;
    public string LinkedTaskTitle
    {
        get => _linkedTaskTitle;
        set => Set(ref _linkedTaskTitle, value);
    }

    public ICommand StartCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand StopCommand { get; }

    private void Start()
    {
        _pomodoroService.Start(SelectedTask?.Id);
        Status = TimerStatus.Running;
        UpdateLinkedTaskTitle();
    }

    private void Pause()
    {
        _pomodoroService.Pause();
        Status = TimerStatus.Paused;
    }

    private async Task StopAsync()
    {
        await _pomodoroService.StopAsync();
        Status = TimerStatus.Idle;
        await RefreshSessionsAsync();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        var state = _pomodoroService.GetTimerState();
        var prevStatus = Status;
        Status = state.Status;

        var display = state.Status switch
        {
            TimerStatus.Running => state.ElapsedTime + (DateTime.Now - (state.StartTime ?? DateTime.Now)),
            TimerStatus.Paused => state.ElapsedTime,
            _ => TimeSpan.Zero
        };

        ElapsedTimeDisplay = display.ToString(@"hh\:mm\:ss");

        if (Status != prevStatus && Status == TimerStatus.Idle)
            LinkedTaskTitle = string.Empty;
    }

    private void UpdateLinkedTaskTitle()
    {
        var taskId = _pomodoroService.CurrentTaskId;
        LinkedTaskTitle = taskId is null
            ? string.Empty
            : Tasks.FirstOrDefault(t => t.Id == taskId)?.Title ?? string.Empty;
    }

    // ── Session Log ──

    public ObservableCollection<string> Sessions { get; } = new();

    // ── Load ──

    private async Task LoadAsync()
    {
        await RefreshTasksAsync();
        await RefreshSessionsAsync();
    }

    private async Task RefreshTasksAsync()
    {
        var tasks = await _taskService.GetAllTasksAsync();
        Tasks.Clear();
        ActiveTasks.Clear();
        foreach (var t in tasks)
        {
            Tasks.Add(t);
            if (!t.IsCompleted)
                ActiveTasks.Add(t);
        }
    }

    private async Task RefreshSessionsAsync()
    {
        var sessions = await _pomodoroService.GetAllSessionsAsync();
        var taskNames = Tasks.ToDictionary(t => t.Id, t => t.Title);

        Sessions.Clear();
        foreach (var s in sessions)
        {
            var taskName = s.TaskId is not null && taskNames.TryGetValue(s.TaskId.Value, out var name)
                ? name
                : "";
            var display = taskName.Length > 0
                ? $"{s.Duration:hh\\:mm\\:ss}  {s.StartTime:HH:mm} ~ {s.EndTime:HH:mm}  {taskName}"
                : $"{s.Duration:hh\\:mm\\:ss}  {s.StartTime:HH:mm} ~ {s.EndTime:HH:mm}";
            Sessions.Add(display);
        }
    }
}
