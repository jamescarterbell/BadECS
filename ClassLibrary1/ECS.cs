using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public interface IIndexed { }

public interface IIndexed<T> : IIndexed
{
    T this[int index] { get; set; }
    BitArray validBits { get; }
    int Count { get; }
    void Clear();
    bool Contains(T item);
    bool Valid(int index);
    int IndexOf(T item);
    void Insert(int index, T item);
    void Remove(int index);
}

/// <summary>
/// A Dictionary that can be used as a list, used for sparse storage
/// </summary>
public class HashList<T> : IIndexed<T>
    where T : new()
{
    private Dictionary<int, T> m_dict;
    private BitArray m_bitArray;

    /// <summary>
    /// Create a new HashList
    /// </summary>
    public HashList()
    {
        m_dict = new Dictionary<int, T>(0);
        m_bitArray = new BitArray(0);
    }

    /// <summary>
    /// Create a new HashList with the given capacity
    /// </summary>
    /// <param name="fromEnumerable">Initial capacity</param>
    public HashList(int capacity)
    {
        m_dict = new Dictionary<int, T>(capacity);
        m_bitArray = new BitArray(capacity);
    }

    /// <summary>
    /// Create a new HashList from the given enumerable
    /// </summary>
    /// <param name="list">Previous collection</param>
    public HashList(IEnumerable<T> list)
    {
        IEnumerator<T> iter = list.GetEnumerator();
        int i = 0;
        do
        {
            m_dict.Add(i, iter.Current);
        } while (iter.MoveNext());
        m_bitArray = new BitArray(i);
    }

    public T this[int index] { get { return m_dict[index]; } set { m_bitArray[index] = true; m_dict[index] = value; } }

    public BitArray validBits { get { return m_bitArray; } }

    /// <summary>
    /// Number of items currently in the HashList
    /// </summary>
    public int Count {
        get
        {
            return m_dict.Count;
        }
    }

    /// <summary>
    /// Remove all items from the list
    /// </summary>
    public void Clear()
    {
        m_dict = new Dictionary<int, T>(0);
        m_bitArray = new BitArray(0);
    }

    /// <summary>
    /// Find item in the list
    /// </summary>
    /// <param name="item"></param>
    /// <returns>True if item is in the collection</returns>
    public bool Contains(T item)
    {
        return m_dict.ContainsValue(item);
    }

    /// <summary>
    /// See if this item has been placed in the collection
    /// </summary>
    /// <param name="index"></param>
    /// <returns>True if index contains a valid item</returns>
    public bool Valid(int index)
    {
        return m_bitArray[index];
    }

    /// <summary>
    /// Find the index of an item, very slow
    /// </summary>
    /// <param name="item"></param>
    /// <returns>Index of the item if it's find</returns>
    /// <throws>KeyNotFoundException if the key hasn't been inserted</throws>
    public int IndexOf(T item)
    {
        foreach(KeyValuePair<int, T> kvp in m_dict)
        {
            if(kvp.Value.Equals(item))
            {
                return kvp.Key;
            }
        }
        throw new KeyNotFoundException();
    }

    /// <summary>
    /// Insert an item at the specified index, this replaces whatever was at that index.
    /// </summary>
    /// <param name="index">Where to insert the item</param>
    /// <param name="item">Item to insert</param>
    public void Insert(int index, T item)
    {
        m_dict.Add(index, item);
        while(m_bitArray.Length <= index)
        {
            m_bitArray.Length *= 2;
        }
        m_bitArray[index] = true;
    }

    /// <summary>
    /// Remove an item at the specified index, this replaces whatever was at that index.
    /// </summary>
    /// <param name="index">Where to insert the item</param>
    /// <param name="item">Item to insert</param>
    public void Remove(int index)
    {
        m_dict.Remove(index);
        m_bitArray[index] = false;
    }
}

/// <summary>
/// A List that can be used as a storage, includes valid bit array.
/// </summary>
public class IndexList<T> : IIndexed<T>
    where T : new()
{
    private List<T> m_list;
    private BitArray m_bitArray;

    /// <summary>
    /// Create a new IndexList
    /// </summary>
    public IndexList()
    {
        m_list = new List<T>(0);
        m_bitArray = new BitArray(0);
    }

    /// <summary>
    /// Create a new IndexList with the given capacity
    /// </summary>
    /// <param name="fromEnumerable">Initial capacity</param>
    public IndexList(int capacity)
    {
        m_list = new List<T>(capacity);
        m_bitArray = new BitArray(capacity);
    }

    /// <summary>
    /// Create a new IndexList from the given enumerable
    /// </summary>
    /// <param name="list">Previous collection</param>
    public IndexList(IEnumerable<T> list)
    {
        m_list = new List<T>(list);
        m_bitArray = new BitArray(m_list.Capacity);
    }

    public T this[int index] { get { return m_list[index]; } set { m_bitArray[index] = true; m_list[index] = value; } }
    public BitArray validBits { get { return m_bitArray; } }

    /// <summary>
    /// Number of items currently in the HashList
    /// </summary>
    public int Count
    {
        get
        {
            int i = 0;
            foreach(bool b in m_bitArray)
            {
                if (b) i++;
            }
            return i;
        }
    }

    /// <summary>
    /// Remove all items from the list
    /// </summary>
    public void Clear()
    {
        m_list = new List<T>(0);
        m_bitArray = new BitArray(0);
    }

    /// <summary>
    /// See if the list contains the item
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Contains(T item)
    {
        return m_list.Contains(item);
    }

    /// <summary>
    /// See if the index has a valid object
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Valid(int index)
    {
        return m_bitArray[index];
    }

    /// <summary>
    /// Find the index of an item, very slow
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public int IndexOf(T item)
    {
        return m_list.IndexOf(item);
    }

    /// <summary>
    /// add an item at the specified index
    /// </summary>
    /// <param name="index"></param>
    /// <param name="item"></param>
    public void Insert(int index, T item)
    {
        while(m_list.Capacity <= index)
        {
            if (m_list.Capacity == 0)
            {
                m_list.Capacity = 1;
                continue;
            }
            m_list.Capacity *= 2;
            m_bitArray.Length = m_list.Capacity;
        }
        while(m_list.Count <= index)
        {
            m_list.Add(new T());
        }
        m_list[index] = item;
        m_bitArray[index] = true;
    }
    
    /// <summary>
    /// Remove an item at the specified index, this replaces whatever was at that index.
    /// </summary>
    /// <param name="index">Where to insert the item</param>
    /// <param name="item">Item to insert</param>
    public void Remove(int index)
    {
        m_bitArray[index] = false;
    }
}

/// <summary>
/// A storage for empty structs that are used as tags, doesn't actually store structs, just a bit array wrapper.  Accessing returns true or false.
/// </summary>
/// <typeparam name="T"></typeparam>
public class NullList<T> : IIndexed<T>
    where T: new()
{
    private BitArray m_bitArray;
    private int count;

    public NullList()
    {
        m_bitArray = new BitArray(0);
    }

    public T this[int index] { get { return new T(); } set{ m_bitArray[index] = true; count++ ; } }

    public BitArray validBits { get { return m_bitArray; } }

    public int Count { get { return count; } }

    public void Clear()
    {
        m_bitArray = new BitArray(0);
        count = 0;
    }

    public bool Contains(T item)
    {
        return count > 0;
    }

    public int IndexOf(T item)
    {
        if (count == 0) return -1;
        for(int i = 0; i < m_bitArray.Length; i++)
        {
            if (m_bitArray[i]) return i;
        }
        return -1;
    }

    public void Insert(int index, T item)
    {
        while(m_bitArray.Length <= index)
        {
            if(m_bitArray.Length == 0)
            {
                m_bitArray.Length = 1;
                continue;
            }
            m_bitArray.Length *= 2;
        }
        m_bitArray[index] = true;
    }

    public void Remove(int index)
    {
        m_bitArray[index] = true;
    }

    public bool Valid(int index)
    {
        return m_bitArray[index];
    }
}

public interface IComponent{};

public struct Entity
{
    public int id;
    public int generation;
}

public class World
{

    public Dictionary<Type, IIndexed<IComponent>> componentLUT;
    public Dictionary<Type, IComponent> resourceLUT;

    public List<Entity> entities;
    public Queue<Entity> deletedEntities;

    public World()
    {
        componentLUT = new Dictionary<Type, IIndexed<IComponent>>();
        resourceLUT = new Dictionary<Type, IComponent>();

        entities = new List<Entity>();
        deletedEntities = new Queue<Entity>();
    }

    public void RegisterComponent(Type type, Type storageType)
    {
        if (!type.GetInterfaces().Contains(typeof(IComponent)))
            throw new NotComponentException();
        if (!storageType.GetInterfaces().Contains(typeof(IIndexed)))
            throw new NotIIndexedException();
        componentLUT.Add(type, Activator.CreateInstance(storageType) as IIndexed<IComponent>);
    }
}

public class NotComponentException: Exception
{

}

public class NotIIndexedException: Exception
{

}