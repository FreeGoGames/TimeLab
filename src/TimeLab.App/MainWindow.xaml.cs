using System.Windows;
using TimeLab.App.ViewModels;
using TimeLab.Application;

namespace TimeLab.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var taskRepo = new InMemoryTaskRepository();
        var sessionRepo = new InMemorySessionRepository();

        var taskService = new TaskService(taskRepo);
        var pomodoroService = new PomodoroService(sessionRepo);

        DataContext = new MainViewModel(taskService, pomodoroService);
    }
}
