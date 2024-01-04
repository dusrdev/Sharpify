namespace Sharpify.Data;

/// <summary>
/// Database options
/// </summary>
[Flags]
public enum DatabaseOptions : byte {
    /// <summary>
    /// Serialize database after upsert or delete.
    /// </summary>
    /// <remarks>
    /// If <see cref="Database{T}.RemoveAny(Func{T, bool})"/> is called serialization will only be done once, after execution is finished.
    /// </remarks>
    SerializeOnUpdate = 1 << 0,

    /// <summary>
    /// Trigger the OnDataChanged event on upsert and delete.
    /// </summary>
    TriggerUpdateEvents = 1 << 1,

    /// <summary>
    /// Specifies that the database should ignore key cases.
    /// </summary>
    IgnoreKeyCases = 1 << 2,
}