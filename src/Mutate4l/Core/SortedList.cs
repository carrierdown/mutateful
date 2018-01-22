using System;
using System.Collections;
using System.Collections.Generic;
using Mutate4l.Dto;

namespace Mutate4l.Core
{
    public class SortedList<T> : IEnumerable<T>
    {
        private readonly List<T> _list;
        private readonly IComparer<T> _comparer;

        public int Count => _list.Count;

        public bool IsReadOnly => ((IList<T>)_list).IsReadOnly;

        public T this[int index] { get => _list[index]; }

        public SortedList(IComparer<T> comparer = null)
        {
            _comparer = comparer ?? Comparer<T>.Default;
            _list = new List<T>();
        }

        public SortedList(List<T> items) : this(comparer: null)
        {
            AddRange(items);
        }

        public void Add(T item)
        {
            if (_list.Contains(item)) return;
            var index = _list.BinarySearch(item);
            _list.Insert(index < 0 ? ~index : index, item);
        }

        public void AddRange(List<T> items)
        {
            items.ForEach(x => Add(x));
        }

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return _list.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IList<T>)_list).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)_list).GetEnumerator();
        }


    }
}
