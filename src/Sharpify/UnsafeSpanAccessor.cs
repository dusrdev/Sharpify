using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sharpify;

/// <summary>
/// Represents an unsafe span wrapper that can be used in async methods and be boxed to the heap.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <remarks>
/// Only use it where you can guarantee the scope of the span, it is named "Unsafe" for a reason.
/// </remarks>
public unsafe readonly struct UnsafeSpanIterator<T> : IEnumerable<T>
{
    private readonly void* _pointer;

    /// <summary>
    /// The length of the span
    /// </summary>
    public readonly int Length;

    /// <summary>
    /// Creates a new instance of <see cref="UnsafeSpanIterator{T}"/> over the specified span.
    /// </summary>
    /// <param name="span"></param>
    public UnsafeSpanIterator(ReadOnlySpan<T> span)
    {
        _pointer = Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
        Length = span.Length;
    }

    private UnsafeSpanIterator(void* start, int length)
    {
        _pointer = start;
        Length = length;
    }

    /// <summary>
    /// Returns a slice of the span
    /// </summary>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public UnsafeSpanIterator<T> Slice(int start, int length)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(start + length, Length);
        return new UnsafeSpanIterator<T>(Unsafe.Add<T>(_pointer, start), length);
    }

    /// <summary>
    /// Returns the element at the given index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ref readonly T this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(index, Length);
            void* item = Unsafe.Add<T>(_pointer, index);
            return ref Unsafe.AsRef<T>(item);
        }
    }

    /// <summary>
    /// Generates an IEnumerable of the elements in the span
    /// </summary>
    /// <returns></returns>
    public IEnumerable<T> ToEnumerable()
    {
        for (var i = 0; i < Length; i++)
        {
            yield return this[i];
        }
    }

    /// <summary>
    /// Gets the enumerator for the span
    /// </summary>
    /// <returns></returns>
    public IEnumerator<T> GetEnumerator() => new UnsafeSpanIteratorEnumerator(this);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal struct UnsafeSpanIteratorEnumerator : IEnumerator<T>
    {
        private readonly UnsafeSpanIterator<T> _source;
        private int _index;
        private T? _current;

        internal UnsafeSpanIteratorEnumerator(UnsafeSpanIterator<T> source)
        {
            _source = source;
            _index = 0;
            _current = default;
        }

        public void Dispose() {}

        public bool MoveNext()
        {
            UnsafeSpanIterator<T> local = _source;

            if ((uint)_index < (uint)local.Length)
            {
                _current = local[_index];
                _index++;
                return true;
            }
            return MoveNextRare();
        }

        private bool MoveNextRare()
        {
            _index = _source.Length + 1;
            _current = default;
            return false;
        }

        public readonly T Current => _current!;

        readonly object? IEnumerator.Current
        {
            get
            {
                if ((uint)_index >= _source.Length + 1)
                {
                    throw new InvalidOperationException("The enumerator has not been started or has already finished.");
                }
                return Current;
            }
        }

        void IEnumerator.Reset()
        {
            _index = 0;
            _current = default;
        }
    }
}