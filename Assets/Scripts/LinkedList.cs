using System;
using System.Collections;

public class LinkedList<T> : IEnumerable
{
    private ListNode head;
    private ListNode current;
    private ListNode tail;
    //private ListNode _currentNode;

    private int count;
    public int Count
    {
        get
        {
            return count;
        }
    }

    private class ListNode
    {
        public ListNode(T nodeVal)
        {
            nodeData = nodeVal;
        }

        public ListNode(T nodeVal, ListNode listNode)
        {
            nodeData = nodeVal;
            _nextNode = listNode;
        }

        private T nodeData = default(T);
        public T NodeData
        {
            get { return nodeData; }
        }

        private ListNode _nextNode = null;
        public ListNode NextNode
        {
            get { return _nextNode; }
            set { _nextNode = value; }
        }
    }

    public LinkedList()
    {
        head = null;
        tail = null;
        count = 0;
    }

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= count)
            {
                throw new IndexOutOfRangeException("Invalid index: " + index);
            }

            var temp = GetNodeAtIndex(index);

            return temp != null ? temp.NodeData : default(T);
        }
        set
        {
            if (index < 0 || index >= count)
            {
                throw new IndexOutOfRangeException("Invalid index: " + index);
            }

            var prevIndex = index - 1;
            var previousNode = prevIndex < count && prevIndex > 0 ? GetNodeAtIndex(index - 1) : null;
            var temp = GetNodeAtIndex(index);

            if (temp != null)
            {
                temp = new ListNode(value, temp.NextNode);

                if (previousNode != null)
                    previousNode.NextNode = temp;
            }

        }
    }


    public IEnumerator GetEnumerator()
    {
        current = head;
        while (current != null)
        {
            yield return current.NodeData;
            current = current.NextNode;
        }
    }

    private ListNode GetNodeAtIndex(int index)
    {
        return GetNodeViaLoop(head, index, 0);
    }

    private ListNode GetNodeViaLoop(ListNode currentNode, int goalIndex, int currentIndex)
    {

        if (currentNode != null && goalIndex == currentIndex)
        { return currentNode; }
        else
        {
            currentIndex++;
            if (currentIndex <= goalIndex)
                return GetNodeViaLoop(currentNode.NextNode, goalIndex, currentIndex);
        }

        return null;
    }

    public void Add(T item)
    {
        if (count == 0)
        {
            head = new ListNode(item);
            tail = head;
        }
        else
        {
            var tempNode = new ListNode(item);
            tail.NextNode = tempNode;
            tail = tempNode;
        }
        count++;

    }

    public void Clear()
    {
        head = null;
        tail = null;
        count = 0;
    }

    public bool Contains(T item)
    {
        return IndexOf(item) != -1;
    }

    public void CopyTo(T[] target, int index)
    {
        if (count <= 0) { return; }

        int maxTargetItems = target.Length - index;
        var _items = new T[count];
        for (int i = 0; i < maxTargetItems; i++)
        {
            if (i >= count) { break; }
            _items[i] = GetNodeAtIndex(i).NodeData;
        }

        Array.Copy(_items, 0, target, index, Math.Min(_items.Length, maxTargetItems));
    }

    public int IndexOf(T item)
    {
        return GetIndexOfItem(item, head, 0);
    }

    private int GetIndexOfItem(T item, ListNode currentNode, int index)
    {
        if (currentNode != null && object.Equals(currentNode.NodeData, item))
        {
            return index;
        }
        else
        {
            index++;
            if (currentNode.NextNode == null) { return -1; }

            return GetIndexOfItem(item, currentNode.NextNode, index);
        }
    }

    public void Insert(int index, T item)
    {
        if (index < 0 || index > count)
        {
            throw new IndexOutOfRangeException("Invalid index: " + index);
        }

        var previousNode = GetNodeAtIndex(index - 1);
        var nodeToMove = GetNodeAtIndex(index);

        var newNode = new ListNode(item, nodeToMove);

        if (index == 0)
        {
            head = newNode;
            count++;
        }
        else if (previousNode != null)
        {
            previousNode.NextNode = newNode;
            if (index == count) { tail = newNode; }
            count++;
        }
    }

    public bool Remove(T item)
    {
        var retval = false;
        var index = IndexOf(item);
        if (index != -1)
        {
            var previousNode = GetNodeAtIndex(index - 1);
            var nextNode = GetNodeAtIndex(index + 1);
            if (index == 0)
            {
                head = nextNode;
                count--;
                retval = true;
            }
            else if (previousNode != null)
            {
                previousNode.NextNode = nextNode;
                count--;
                retval = true;
            }
        }
        return retval;
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= count)
        {
            throw new IndexOutOfRangeException("Invalid index: " + index);
        }

        var previousNode = GetNodeAtIndex(index - 1);
        var nextNode = GetNodeAtIndex(index + 1);

        if (index == 0)
        {
            head = nextNode;
            count--;
        }
        else if (previousNode != null)
        {
            previousNode.NextNode = nextNode;
            count--;
        }
    }
}

