using System.Runtime.CompilerServices;

namespace Sharpify;

/// <summary>
/// A representation a result with status and message
/// </summary>
public readonly record struct Result {
    /// <summary>
    /// Status
    /// </summary>
    public readonly bool IsOk { get; init; }

    /// <summary>
    /// <see langword="true"/> if the status is not Ok
    /// </summary>
    public readonly bool IsFail => !IsOk;

    /// <summary>
    /// Message to pass along with the status
    /// </summary>
    public readonly string Message { get; init; }

    /// <summary>
    /// DO NOT USE CONSTRUCTOR, USE <see cref="Ok(string?)"/> or <see cref="Fail(string?)"/> or their overloads INSTEAD
    /// </summary>
    public Result() => throw new InvalidOperationException("Result cannot be instantiated directly. Use Ok or Fail methods.");

    internal Result(bool isOk, string message) {
        IsOk = isOk;
        Message = message;
    }

    /// <summary>
    /// Deconstructs the <see cref="Result"/> into <paramref name="isOk"/> and <paramref name="message"/>
    /// </summary>
    public void Deconstruct(
        out bool isOk,
        out string message) => (isOk, message) = (IsOk, Message);

    /// <summary>
    /// Returns a result with status success and <paramref name="message"/>
    /// </summary>
    public static Result Ok(string message = "") => new(true, message);

    /// <summary>
    /// Returns a result with status success and <paramref name="message"/> and <paramref name="value"/>
    /// </summary>
    public static Result<T> Ok<T>(string message, T value) => new(true, message, value);

    /// <summary>
    /// Returns a result with status success and <paramref name="value"/>
    /// </summary>
    public static Result<T> Ok<T>(T value) => new(true, "", value);

    /// <summary>
    /// Returns a result with status failed and <paramref name="message"/>
    /// </summary>
    public static Result Fail(string message = "") => new(false, message);

    /// <summary>
    /// Creates a failed result with the specified value.
    /// </summary>
    public static Result<T> Fail<T>(T value) => new(false, "", value);

    /// <summary>
    /// Creates a failed result with the specified massage and value.
    /// </summary>
    public static Result<T> Fail<T>(string message, T value) => new(false, message, value);

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
public readonly record struct Result<T> {
    /// <summary>
    /// Status
    /// </summary>
    public readonly bool IsOk { get; init; }

    /// <summary>
    /// <see langword="true"/> if the status is not Ok
    /// </summary>
    public readonly bool IsFail => !IsOk;

    /// <summary>
    /// Message to pass along with the status
    /// </summary>
    public readonly string Message { get; init; }

    /// <summary>
    /// Inner value
    /// </summary>
    public readonly T? Value { get; init; }

    /// <summary>
    /// DO NOT USE CONSTRUCTOR, USE <see cref="Result.Ok(string?)"/> or <see cref="Result.Fail(string?)"/> or their overloads INSTEAD
    /// </summary>
    public Result() => throw new InvalidOperationException("Result cannot be instantiated directly. Use Ok or Fail methods.");

    internal Result(bool isOk, string message, T? value) {
        IsOk = isOk;
        Message = message;
        Value = value;
    }

    /// <summary>
    /// Deconstructs the <see cref="Result{T}"/> into <paramref name="isOk"/>, <paramref name="message"/> and <paramref name="value"/>
    /// </summary>
    public void Deconstruct(
        out bool isOk,
        out string message,
        out T? value) => (isOk, message, value) = (IsOk, Message, Value);

    /// <summary>
    /// Converts a <see cref="Result{T}"/> to a <see cref="Result"/>
    /// </summary>
    /// <param name="result"></param>
    public static implicit operator Result(Result<T> result) => new(result.IsOk, result.Message);

    /// <summary>
    /// Converts a <see cref="Result"/> to a <see cref="Result{T}"/> with <see cref="Value"/> set to <see langword="default"/>
    /// </summary>
    public static implicit operator Result<T>(Result result) => new(result.IsOk, result.Message, default);

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
        return new(result.IsOk, result.Message, value);
    }
}