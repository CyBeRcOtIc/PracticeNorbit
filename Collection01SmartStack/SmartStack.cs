using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SmartStack<T> : IEnumerable<T>
{
    private T[] _items;
    private int _count;

    public int Count => _count;
    public int Capacity => _items.Length;

    private void EnsureCapacity()
    {
        if (_count == _items.Length)
        {
            T[] temp = new T[_items.Length * 2];

            for (int i = 0; i < _count; i++)
            {
                temp[i] = _items[i];
            }

            _items = temp;
        }
    }

    public SmartStack()
    {
        _items = new T[4];
        _count = 0;
    }

    public SmartStack(int n)
    {
        if (n <= 0)
            throw new ArgumentOutOfRangeException(nameof(n));

        _items = new T[n];
        _count = 0;
    }

    public SmartStack(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        _items = new T[4];
        _count = 0;

        foreach (var item in collection)
        {
            EnsureCapacity();
            _items[_count] = item;
            _count++;
        }
    }

    public void Push(T element)
    {
        EnsureCapacity();

        _items[_count] = element;
        _count++;
    }

    public void PushRange(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);
        
        foreach (var item in collection)
        {
            EnsureCapacity();

            _items[_count] = item;
            _count++;
        }
    }

    public T Pop()
    {
        if(_count == 0)
            throw new InvalidOperationException();

        _count--;
        return _items[_count];
    }

    public T Peek()
    {
        if(_count == 0)
            throw new InvalidOperationException();

        return _items[_count - 1];
    }

    public bool Contains(T element)
    {
        for (int i = 0; i < _count; i++)
        {
            if(EqualityComparer<T>.Default.Equals(element, _items[i]))
            {
                return true;
            }
        }

        return false;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = _count - 1; i >= 0; i--)
        {
            yield return _items[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public T this[int index]
    {
        get
        {
            if(index < 0 || index >= _count)
                throw new ArgumentOutOfRangeException();

            return _items[_count - 1 - index];
        }
    }
}