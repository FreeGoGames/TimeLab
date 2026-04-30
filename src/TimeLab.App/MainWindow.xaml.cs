using System.Windows;
using TimeLab.App.ViewModels;
using TimeLab.Application;
using TimeLab.Infrastructure;

namespace TimeLab.App;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var taskRepo = new JsonTaskRepository();
        var sessionRepo = new JsonSessionRepository();

        var taskService = new TaskService(taskRepo);
        var pomodoroService = new PomodoroService(sessionRepo);

        DataContext = new MainViewModel(taskService, pomodoroService);
    }
}
