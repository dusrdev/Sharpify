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
internal unsafe readonly struct UnsafeSpanAccessor<T> : IEnumerable<T>
{
    // Should not be required in .NET 9
    private readonly void* _pointer;
    public readonly int Length;

    public UnsafeSpanAccessor(ReadOnlySpan<T> span)
    {
        _pointer = Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
        Length = span.Length;
    }

    private UnsafeSpanAccessor(void* start, int length)
    {
        _pointer = start;
        Length = length;
    }

    public UnsafeSpanAccessor<T> Slice(int start, int length)
    {
        return new UnsafeSpanAccessor<T>(Unsafe.Add<T>(_pointer, start), length);
    }

    public ref readonly T this[int index]
    {
        get
        {
            void* item = Unsafe.Add<T>(_pointer, index);
            return ref Unsafe.AsRef<T>(item);
        }
    }

    public IEnumerable<T> ToEnumerable()
    {
        for (var i = 0; i < Length; i++)
        {
            yield return this[i];
        }
    }

    public IEnumerator<T> GetEnumerator() => new UnsafeSpanAccessorEnumerator(this);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public struct UnsafeSpanAccessorEnumerator : IEnumerator<T>
    {
        internal static IEnumerator<T>? s_emptyEnumerator;

        private readonly UnsafeSpanAccessor<T> _source;
        private int _index;
        private T? _current;

        internal UnsafeSpanAccessorEnumerator(UnsafeSpanAccessor<T> source)
        {
            _source = source;
            _index = 0;
            _current = default;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            UnsafeSpanAccessor<T> local = _source;

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

        public T Current => _current!;

        object? IEnumerator.Current
        {
            get
            {
                if (_index == 0 || _index == _source.Length + 1)
                {
                    throw new Exception();
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