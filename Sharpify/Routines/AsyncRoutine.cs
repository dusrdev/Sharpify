using System.Diagnostics;

namespace Sharpify.Routines;

/// <summary>
/// Represents an asynchronous routine that executes a list of asynchronous actions at a specified interval.
/// </summary>
public class AsyncRoutine : IDisposable {
    private readonly PeriodicTimer _timer;
    private volatile bool _isRunning;
    private RoutineOptions _options;
    private readonly CancellationTokenSource _cancellationTokenSource;

    /// <summary>
    /// List of asynchronous actions to be executed.
    /// </summary>
    public readonly List<Func<CancellationToken, Task>> Actions = new();

    private Task[] _tasks;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncRoutine"/> class with the specified interval, options, and cancellation token source.
    /// </summary>
    /// <param name="interval">The time interval between each iteration of the routine.</param>
    /// <param name="options">The options to configure the behavior of the routine.</param>
    /// <param name="cancellationTokenSource">The cancellation token source used to cancel the routine.</param>
    public AsyncRoutine(TimeSpan interval, RoutineOptions options, CancellationTokenSource cancellationTokenSource) {
        _options = options;
        _timer = new PeriodicTimer(interval);
        _isRunning = true;
        _cancellationTokenSource = cancellationTokenSource;
        _tasks = Array.Empty<Task>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncRoutine"/> class with the specified time interval between iterations.
    /// </summary>
    /// <param name="interval">The time interval between iterations.</param>
    public AsyncRoutine(TimeSpan interval) : this(interval, RoutineOptions.None, new()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncRoutine"/> class with the specified interval and cancellation token source.
    /// </summary>
    /// <param name="interval">The time interval between each iteration of the routine.</param>
    /// <param name="cancellationTokenSource">The cancellation token source used to cancel the routine.</param>
    public AsyncRoutine(TimeSpan interval, CancellationTokenSource cancellationTokenSource) : this(interval, RoutineOptions.None, cancellationTokenSource) { }

    /// <summary>
    /// Adds an asynchronous action to the routine.
    /// </summary>
    /// <param name="action">The asynchronous action to add.</param>
    /// <returns>The current <see cref="AsyncRoutine"/> instance.</returns>
    public AsyncRoutine Add(Func<CancellationToken, Task> action) {
        Actions.Add(action);
        return this;
    }

    /// <summary>
    /// Adds a collection of asynchronous actions to the routine.
    /// </summary>
    /// <param name="actions">The collection of asynchronous actions to add.</param>
    /// <returns>The current <see cref="AsyncRoutine"/> instance.</returns>
    public AsyncRoutine AddRange(IEnumerable<Func<CancellationToken, Task>> actions) {
        Actions.AddRange(actions);
        return this;
    }

    /// <summary>
    /// Changes the options of the current AsyncRoutine instance.
    /// </summary>
    /// <param name="options">The new options to apply.</param>
    /// <returns>The current AsyncRoutine instance.</returns>
    public AsyncRoutine ChangeOptions(RoutineOptions options) {
        _options = options;
        return this;
    }

    /// <summary>
    /// Starts the async routine and executes the registered actions either sequentially or in parallel.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task Start() {
        Debug.Assert(Actions.Count > 0, "Actions.Count must be > 0");
        try {
            while (_isRunning
                   && Actions.Count > 0
                   && await _timer.WaitForNextTickAsync(_cancellationTokenSource.Token)) {
                if (_options.HasFlag(RoutineOptions.ExecuteInParallel)) {
                    _tasks = new Task[Actions.Count];
                    for (int i = 0; i < Actions.Count; i++) {
                        _tasks[i] = Actions[i](_cancellationTokenSource.Token);
                    }
                    await Task.WhenAll(_tasks).WaitAsync(_cancellationTokenSource.Token);
                    continue;
                }
                // Execute actions sequentially
                foreach (var action in Actions) {
                    await action(_cancellationTokenSource.Token);
                }
            }
        } catch (TaskCanceledException) {
            if (_options.HasFlag(RoutineOptions.ThrowOnCancellation)) {
                throw;
            }
        }
    }

    /// <summary>
    /// Stops the routine.
    /// </summary>
    public void Stop() {
        _isRunning = false;
    }

    /// <summary>
    /// Resumes the execution of the asynchronous routine.
    /// </summary>
    public void Resume() {
        _isRunning = true;
    }

    /// <summary>
    /// Disposes the timer and suppresses finalization of the object.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the method is called before cancelling the original cancellation token source, the method will cancel the token source itself and then dispose it.
    /// </para>
    /// <para>
    /// To prevent <see cref="NullReferenceException"/> either cancel the token source before disposing the routine or do not attempt to interact with the token source after disposing the routine and consider it disposed.
    /// </para>
    /// </remarks>
    public void Dispose() {
        if (_cancellationTokenSource is not null && !_cancellationTokenSource.IsCancellationRequested) {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
        _timer.Dispose();
        GC.SuppressFinalize(this);
    }
}