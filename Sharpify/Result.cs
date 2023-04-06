using System.Runtime.CompilerServices;

namespace Sharpify;

/// <summary>
/// A representation a result with status and message
/// </summary>
public readonly record struct Result {
    /// <summary>
    /// Status
    /// </summary>
    public required readonly bool IsOk { get; init; }

    /// <summary>
    /// <see langword="true"/> if the status is not Ok
    /// </summary>
    public readonly bool IsFail => !IsOk;

    /// <summary>
    /// Message to pass along with the status
    /// </summary>
    public readonly string? Message { get; init; }

    /// <summary>
    /// Deconstructs the <see cref="Result"/> into <paramref name="isOk"/> and <paramref name="message"/>
    /// </summary>
    /// <param name="isOk"></param>
    /// <param name="message"></param>
    public void Deconstruct(
        out bool isOk,
        out string? message) => (isOk, message) = (IsOk, Message);

    private static Result<T> InternalOk<T>(
        T value,
        string? message) => new() {
        IsOk = true,
        Message = message,
        Value = value
    };

    /// <summary>
    /// Returns a result with status success and <paramref name="message"/>
    /// </summary>
    /// <param name="message"></param>
    public static Result Ok(string? message = null) => new() { IsOk = true, Message = message };

    /// <summary>
    /// Returns a result with status success and <paramref name="message"/> and <paramref name="value"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    /// <param name="value"></param>
    public static Result<T> Ok<T>(
        string message, T value) => InternalOk(value, message);

    /// <summary>
    /// Returns a result with status success and <paramref name="value"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    public static Result<T> Ok<T>(T value) => InternalOk(value, null);

    /// <summary>
    /// Returns a result with status failed and <paramref name="message"/>
    /// </summary>
    /// <param name="message"></param>
    public static Result Fail(string? message = null) => new() {
        IsOk = false,
        Message = message
    };

    /// <summary>
    /// Returns the <see cref="Result"/> as a <see cref="Task"/>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<Result> AsTask() => Task.FromResult(this);

    /// <summary>
    /// Returns the <see cref="Result"/> as a <see cref="ValueTask"/>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask<Result> AsValueTask() => ValueTask.FromResult(this);
}

/// <summary>
/// A representation of <see cref="Result"/> with a value
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly record struct Result<T> {
    /// <summary>
    /// Status
    /// </summary>
    public required readonly bool IsOk { get; init; }

    /// <summary>
    /// <see langword="true"/> if the status is not Ok
    /// </summary>
    public readonly bool IsFail => !IsOk;

    /// <summary>
    /// Message to pass along with the status
    /// </summary>
    public readonly string? Message { get; init; }

    /// <summary>
    /// Inner value
    /// </summary>
    public readonly T? Value { get; init; }

    /// <summary>
    /// Deconstructs the <see cref="Result{T}"/> into <paramref name="isOk"/>, <paramref name="message"/> and <paramref name="value"/>
    /// </summary>
    /// <param name="isOk"></param>
    /// <param name="message"></param>
    /// <param name="value"></param>
    public void Deconstruct(
        out bool isOk,
        out string? message,
        out T? value) => (isOk, message, value) = (IsOk, Message, Value);

    /// <summary>
    /// Converts a <see cref="Result{T}"/> to a <see cref="Result"/>
    /// </summary>
    /// <param name="result"></param>
    public static implicit operator Result(Result<T> result) => new() {
        IsOk = result.IsOk,
        Message = result.Message
    };

    /// <summary>
    /// Converts a <see cref="Result"/> to a <see cref="Result{T}"/> with <see cref="Value"/> set to <see langword="default"/>
    /// </summary>
    /// <param name="result"></param>
    public static implicit operator Result<T>(Result result) => new() {
        IsOk = result.IsOk,
        Message = result.Message,
        Value = default
    };

    /// <summary>
    /// Returns the <see cref="Result{T}"/> as a <see cref="Task"/>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<Result<T>> AsTask() => Task.FromResult(this);

    /// <summary>
    /// Returns the <see cref="Result{T}"/> as a <see cref="ValueTask"/>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueTask<Result<T>> AsValueTask() => ValueTask.FromResult(this);
}

/// <summary>
/// Provides extension methods for <see cref="Result"/> and <see cref="Result{T}"/>
/// </summary>
public static class ResultExtensions {
    /// <summary>
    /// Creates a <see cref="Result{T}"/> from <see cref="Result"/> with <paramref name="value"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="result"></param>
    /// <param name="value"></param>
    /// <remarks>
    /// <para>
    /// Throws <see cref="ArgumentNullException"/> if <paramref name="result"/> is <see langword="null"/>
    /// </para>
    /// <para>
    /// Is slightly less efficient than the <see cref="Result.Ok{T}(T)"/> or <see cref="Result.Ok{T}(string, T)"/> methods
    /// </para>
    /// </remarks>
    public static Result<T> WithValue<T>(this in Result result, in T value) {
        ArgumentNullException.ThrowIfNull(result);
        return new() {
            IsOk = result.IsOk,
            Message = result.Message,
            Value = value
        };
    }
}