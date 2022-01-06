namespace Mutateful.Core;

public class SortedList<T> : IEnumerable<T>, IEquatable<T>
{
    private readonly List<T> ListField;
    private readonly IComparer<T> ComparerField;

    public int Count => ListField.Count;

    public bool IsReadOnly => ((IList<T>)ListField).IsReadOnly;

    public T this[int index] => ListField[index];

    public static readonly SortedList<T> Empty = new();

    public SortedList(IComparer<T> comparer = null)
    {
        ComparerField = comparer ?? Comparer<T>.Default;
        ListField = new List<T>();
    }

    public SortedList(IEnumerable<T> items) : this(comparer: null)
    {
        AddRange(items);
    }

    public void Add(T item)
    {
        if (ListField.Contains(item)) return;
        var index = ListField.BinarySearch(item);
        ListField.Insert(index < 0 ? ~index : index, item);
    }

    public void AddRange(List<T> items)
    {
        items.ForEach(x => Add(x));
    }

    public void AddRange(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            Add(item);
        }
    }

    public int IndexOf(T item)
    {
        return ListField.IndexOf(item);
    }

    public void RemoveAt(int index)
    {
        ListField.RemoveAt(index);
    }

    public void Clear()
    {
        ListField.Clear();
    }

    public bool Contains(T item)
    {
        return ListField.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        ListField.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
        return ListField.Remove(item);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return ((IList<T>)ListField).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<T>)ListField).GetEnumerator();
    }

    protected bool Equals(SortedList<T> other)
    {
        if (ListField.Count != other.Count) return false;
        for (var i = 0; i < ListField.Count; i++)
        {
            if (!ListField[i].Equals(other[i]))
            {
                return false;
            }
        }
        return true;
    }

    public bool Equals(T other)
    {
        throw new NotImplementedException();
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SortedList<T>)obj);
    }

    public override int GetHashCode()
    {
        return (ListField != null ? ListField.GetHashCode() : 0);
    }
}