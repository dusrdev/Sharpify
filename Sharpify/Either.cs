namespace Sharpify;

/// <summary>
/// Discriminated union of two types.
/// </summary>
public readonly record struct Either<T0, T1> {
    private readonly T0? _value0;
    private readonly T1? _value1;

    /// <summary>
    /// Checks if the value is T0.
    /// </summary>
    public readonly bool IsT0;

    /// <summary>
    /// Checks if the value is T1.
    /// </summary>
    public readonly bool IsT1;

    /// <summary>
    /// Gets the value as T0.
    /// </summary>
    public T0 AsT0 => _value0 ?? throw new InvalidOperationException("T0 is null");

    /// <summary>
    /// Gets the value as T1.
    /// </summary>
    public T1 AsT1 => _value1 ?? throw new InvalidOperationException("T1 is null");

    /// <summary>
    /// Creates a new instance of <see cref="Either{T0, T1}"/> with both values set to null.
    /// </summary>
    /// <remarks>
    /// This is an implementation of the default constructor, do not use this. Use only the implicit converters from either T0 or T1.
    /// </remarks>
    public Either() => throw new InvalidOperationException("Either cannot be instantiated directly. Use implicit converters from either T0 or T1.");

    private Either(T0 value) {
        _value0 = value;
        _value1 = default;
        IsT0 = true;
        IsT1 = false;
    }

    private Either(T1 value) {
        _value0 = default;
        _value1 = value;
        IsT0 = false;
        IsT1 = true;
    }

    /// <summary>
    /// Implicitly converts from T0 to <see cref="Either{T0, T1}"/>.
    /// </summary>
    public static implicit operator Either<T0, T1>(T0 value) => new(value);

    /// <summary>
    /// Implicitly converts from T1 to <see cref="Either{T0, T1}"/>.
    /// </summary>
    public static implicit operator Either<T0, T1>(T1 value) => new(value);

    /// <summary>
    /// Switches on the type of the value.
    /// </summary>
    public void Switch(Action<T0> handleT0, Action<T1> handleT1) {
        if (IsT0) {
            handleT0(_value0!);
        } else if (IsT1) {
            handleT1(_value1!);
        } else {
            throw new InvalidOperationException("T0 and T1 are both null");
        }
    }

    /// <summary>
    /// Matches on the type of the value to return a <typeparamref name="TResult"/>.
    /// </summary>
    public TResult Match<TResult>(Func<T0, TResult> handleT0, Func<T1, TResult> handleT1) => IsT0
        ? handleT0(_value0!)
        : handleT1(_value1!);
}