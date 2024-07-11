namespace Sharpify.Routines;

/// <summary>
/// Represents a routine that executes a list of actions at a specified interval.
/// </summary>
public class Routine : IDisposable {
    private readonly System.Timers.Timer _timer;
    private volatile bool _disposed;

    /// <summary>
    /// List of actions to be executed by the routine.
    /// </summary>
    public readonly List<Action> Actions = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="Routine"/> class with the specified interval.
    /// </summary>
    /// <param name="intervalInMilliseconds">The time interval between timer events, in milliseconds.</param>
    public Routine(double intervalInMilliseconds) {
        _timer = new System.Timers.Timer(intervalInMilliseconds);
        _timer.Elapsed += OnTimerElapsed;
    }

    /// <summary>
    /// Adds an action to the routine.
    /// </summary>
    /// <param name="action">The action to add.</param>
    /// <returns>The updated routine.</returns>
    public Routine Add(Action action) {
        Actions.Add(action);
        return this;
    }

    /// <summary>
    /// Adds a collection of actions to the routine.
    /// </summary>
    /// <param name="actions">The collection of actions to add.</param>
    /// <returns>The updated routine.</returns>
    public Routine AddRange(IEnumerable<Action> actions) {
        Actions.AddRange(actions);
        return this;
    }

    /// <summary>
    /// Starts the routine timer.
    /// </summary>
    /// <returns>The current Routine instance.</returns>
    public Routine Start() {
        _timer.Start();
        return this;
    }

    /// <summary>
    /// Stops the routine.
    /// </summary>
    public void Stop() {
        _timer.Stop();
    }

    private void OnTimerElapsed(object? sender, EventArgs args) {
        foreach (var action in Actions) {
            action();
        }
    }

    /// <summary>
    /// Disposes the timer and suppresses finalization of the object.
    /// </summary>
    public void Dispose() {
        if (_disposed) {
            return;
        }
        _timer?.Close();
        _disposed = true;
    }
}