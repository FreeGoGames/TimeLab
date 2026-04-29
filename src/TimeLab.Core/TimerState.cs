namespace TimeLab.Core;

public class TimerState
{
    public TimerStatus Status { get; set; } = TimerStatus.Idle;
    public DateTime? StartTime { get; set; }
    public TimeSpan ElapsedTime { get; set; }
}
