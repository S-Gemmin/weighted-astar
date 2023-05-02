using System;

public class Heap<T> where T : IHeapItem<T>
{
    private T[] items;
    private int currentItemCount;

    public int Count => currentItemCount;

    public Heap(int maxHeapSize)
    {
        items = new T[maxHeapSize];
    }

    // Adds an element to the heap.
    public void Add(T item)
    {
        item.HeapIndex = currentItemCount;
        items[currentItemCount] = item;
        SortUp(item);
        currentItemCount++;
    }

    // Removes and returns the root element of the heap.
    public T RemoveRoot()
    {
        T rootItem = items[0];
        currentItemCount--;
        items[0] = items[currentItemCount];
        items[0].HeapIndex = 0;
        SortDown(items[0]);
        return rootItem;
    }

    // Removes all elements from the heap.
    public void Clear() 
    {
        currentItemCount = 0;
    }

    // Updates the position of an item in the heap.
    public void UpdateItem(T item) 
    {
        SortUp(item);
    }

    // Determines whether the heap contains the specified element.
    public bool Contains(T item)
    {
        return item.HeapIndex < currentItemCount && Equals(items[item.HeapIndex], item);
    }

    // Sorts an element up the heap.
    private void SortUp(T item)
    {
        int parentIndex = (item.HeapIndex - 1) / 2;

        while (true)
        {
            T parentItem = items[parentIndex];

            if (item.CompareTo(parentItem) <= 0) 
                break;

            Swap(item, parentItem);
            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    // Sorts an element down the heap.
    private void SortDown(T item)
    {
        while (true)
        {
            int childIndexLeft = item.HeapIndex * 2 + 1;
            int childIndexRight = item.HeapIndex * 2 + 2;
            int swapIndex = 0;

            if (childIndexLeft >= currentItemCount)
                return;

            swapIndex = childIndexLeft;

            if (childIndexRight < currentItemCount 
            && items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
            {
                swapIndex = childIndexRight;
            }

            if (item.CompareTo(items[swapIndex]) >= 0)
                return;
                    
            Swap(item, items[swapIndex]);
        }
    }

    // Swaps teo elements in the heap. 
    private void Swap(T itemA, T itemB)
    {
        items[itemA.HeapIndex] = itemB;
        items[itemB.HeapIndex] = itemA;

        int itemAIndex = itemA.HeapIndex;
        
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = itemAIndex;
    }
}

public interface IHeapItem<T> : IComparable<T>
{
    int HeapIndex { get; set; }
}
