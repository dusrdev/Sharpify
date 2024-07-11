namespace Sharpify.Data;

/// <summary>
/// Event arguments for data changed (addition, update or removal of keys)
/// </summary>
public sealed class DataChangedEventArgs : EventArgs {
    /// <summary>
    /// The key that was changed
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// The value that was changed
    /// </summary>
    public required object? Value { get; init; }

    /// <summary>
    /// The type of change that occurred
    /// </summary>
    public required DataChangeType ChangeType { get; init; }
}