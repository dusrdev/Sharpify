namespace Sharpify.Data;

/// <summary>
/// The type of changed that occurred on a key
/// </summary>
public enum DataChangeType : byte {
    /// <summary>
    /// A key was inserted or updated
    /// </summary>
    Upsert = 1 << 0,
    /// <summary>
    /// A key was removed
    /// </summary>
    Remove = 1 << 1
}