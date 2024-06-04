using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public interface IUpdateable
{
    void UpdateOverride();
}

public interface IFixedUpdateAble
{
    void FixedUpdate();
}

public interface IUpdateHolder
{
    AUpdateCase UPD { get; }
}

/// <summary>
/// unique list, can use with foreach
/// </summary>
/// <typeparam Name="T"></typeparam>
public class AUniqueHolder<T>: IEnumerable<T>
{
    class AElement
    {
        public AElement Prev = null;
        public AElement Next = null;

        public T Value;
    } // AElement

    struct SEnumerator: IEnumerator<T>
    {
        readonly AElement _firstElement;
        AElement _currentElement;

        public SEnumerator(AElement firstElement)
        {
            _firstElement = firstElement;
            _currentElement = null;
        }

        public T Current => _currentElement.Value;
        object IEnumerator.Current => Current;

        public void Dispose()
        {
            _currentElement = null;
        }
        public void Reset() => _currentElement = null;
        public bool MoveNext()
        {
            _currentElement = _currentElement == null ? _firstElement : _currentElement.Next;
            return _currentElement != null;
        }
    } // SEnumerator

    AElement _first = null;

    public IEnumerator<T> GetEnumerator() => new SEnumerator(_first);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    AElement Find(T o)
    {
        var e = _first;
        while (e != null)
        {
            if (e.Value.Equals(o))
                return e;
            e = e.Next;
        }
        return null;
    }

    public int Count { get; private set; } = 0;

    public bool Add(T o)
    {
        if (_first == null)
            _first = new AElement { Value = o };
        else
        {
            var e = Find(o);
            if (e != null)
                return false; // already have

            // --- before --- // F
            // --- after --- // E -> F
            // E.Next = F
            // F.Prev = E

            // Add to the beginning, so it'S faster
            e = new AElement { Value = o, Next = _first };
            _first.Prev = e;
            _first = e;
        }
        ++Count;
        return true;
    }
    public bool Remove(T o)
    {
        var e = Find(o);
        if (e == null)
            return false;
        Remove(e);
        --Count;
        return true;
    } // Remove

    void Remove(AElement B)
    {
        // --- before --- // A -> B -> C
        // A.Next = B
        // B.Next = C
        // B.Prev = A
        // C.Prev = B
        var A = B.Prev;
        var C = B.Next;

        // --- after --- // A -> C
        // A.Next = C
        // C.Prev = A

        if (A != null)
            A.Next = C;
        else  // B == _first - Remove first element
            _first = C;

        if (C != null)
            C.Prev = A;

        B.Prev = null;
        B.Next = null;
    } // Remove

    public void Close()
    {
        while (_first != null)
            Remove(_first);
    }
} // class AUniqueHolder

public class AUpdateCase
{
    readonly AUniqueHolder<IUpdateable> _update = new AUniqueHolder<IUpdateable>();
    readonly AUniqueHolder<IFixedUpdateAble> _fixed = new AUniqueHolder<IFixedUpdateAble>();

    ~AUpdateCase() => Close();
    public void Close()
    {
        _update.Close();
        _fixed.Close();
    }

    public void Add4Update(IUpdateable o) => _update.Add(o);
    public void Rem4Update(IUpdateable o) => _update.Remove(o);
    public void Add4Fixed(IFixedUpdateAble o) => _fixed.Add(o);
    public void Rem4Fixed(IFixedUpdateAble o) => _fixed.Remove(o);

    public void Update()
    {
        foreach (var u in _update)
            u.UpdateOverride();
    }
    public void FixedUpdate()
    {
        foreach (var u in _fixed)
            u.FixedUpdate();
    }
} // class AUpdateCase