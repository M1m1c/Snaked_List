using System;
using System.Collections.Generic;

namespace m1m1c_3DAstarPathfinding
{
    public class Heap<T> where T : IHeapItem<T>
    {
        T[] items;
        int currentItemCount;
        public int Count { get { return currentItemCount; } }

        private bool isSorting = false;

        public Heap(int maxHeapSize)
        {
            items = new T[maxHeapSize];
        }

        public void Add(T item)
        {
            item.HeapIndex = currentItemCount;
            items[currentItemCount] = item;
            SortForwards(item);
            currentItemCount++;
        }

        public T GetItemWithKey(int key)
        {
            T retval = default;
            foreach (var item in items)
            {
                if (item.Key == key)
                {
                    retval = item;
                    break;
                }
            }
            return retval;
        }

        public T RemoveFirst()
        {
            T firstItem = items[0];
            currentItemCount--;
            items[0] = items[currentItemCount];
            items[0].HeapIndex = 0;
            SortBackwards(items[0]);
            return firstItem;
        }

        public void UpdateItem(T item)
        {
            SortForwards(item);
        }

        public void Clear()
        {
            currentItemCount = 0;
        }

        public bool Contains(T item)
        {
            if (item.HeapIndex < currentItemCount)
            {
                return Equals(items[item.HeapIndex], item);
            }
            else
            {
                return false;
            }
        }


        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < currentItemCount; i++)
            {
                yield return items[i];
            }
        }

        /// <summary>
        /// sorts the heap using SelectionSort 
        /// </summary>
        public void Sort()
        {
            if (currentItemCount <= 1) { return; }

            if (isSorting) { return; }
            isSorting = true;

            for (int i = 0; i < currentItemCount - 1; i++)
            {
                var minIndex = i;

                for (int q = i + 1; q < currentItemCount; q++)
                {
                    if (items[minIndex].CompareTo(items[q]) < 0)
                    {

                        minIndex = q;
                    }
                }
                Swap(items[minIndex], items[i]);
            }
            isSorting = false;
        }

        /// <summary>
        /// sorts the item to a higher index (closer to the end of the array),
        /// based on if its priority is lower than the child indexes after it.
        /// </summary>
        private void SortBackwards(T item)
        {
            while (true)
            {
                var childIndexLeft = item.HeapIndex * 2 + 1;
                var childIndexRight = item.HeapIndex * 2 + 2;
                var swapIndex = 0;

                if (!(childIndexLeft < currentItemCount)) { return; }

                swapIndex = childIndexLeft;
                if (childIndexRight < currentItemCount &&
                    items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
                {
                    swapIndex = childIndexRight;
                }

                if (!(item.CompareTo(items[swapIndex]) < 0)) { return; }
                Swap(item, items[swapIndex]);
            }
        }

        /// <summary>
        /// sorts the item to a lower index (closer to the start of the array), 
        /// based on if its priority is higher than the parent index before it.
        /// </summary>
        private void SortForwards(T item)
        {
            var parentIndex = (item.HeapIndex - 1) / 2;

            if (parentIndex < 0) { return; }

            while (true)
            {
                T parentItem = items[parentIndex];
                if (parentItem == null) { break; }

                if (item.CompareTo(parentItem) > 0)
                {
                    Swap(item, parentItem);
                }
                else { break; }
                parentIndex = (item.HeapIndex - 1) / 2;
            }
        }

        private void Swap(T itemA, T itemB)
        {
            items[itemA.HeapIndex] = itemB;
            items[itemB.HeapIndex] = itemA;

            int itemAHeapindex = itemA.HeapIndex;
            itemA.HeapIndex = itemB.HeapIndex;
            itemB.HeapIndex = itemAHeapindex;
        }

    }

    public interface IHeapItem<T> : IComparable<T>
    {
        int HeapIndex { get; set; }
        int Key { get; set; }
    }
}