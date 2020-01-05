using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#region Storages

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

#endregion

/// <summary>
/// The tag that all structs need in order to be recognized by the ECS system.
/// Can be used on classes as well, but this will incur a large performance hit.
/// </summary>
public interface IComponent{};

/// <summary>
/// Contains the array position for all components of the entity, as well as it's generation number.
/// </summary>
public struct Entity : IEquatable<Entity>, IComponent
{
    public int id;
    public int generation;

    public override bool  Equals(Object o)
    {
        Entity e = (Entity)o;
        return e.generation == this.generation && e.id == this.id;
    }

    public bool Equals(Entity other)
    {
        return id == other.id &&
               generation == other.generation;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(id, generation);
    }

    static public bool operator ==(Entity e1, Entity e2)
    {
        return e1.Equals(e2);
    }

    static public bool operator !=(Entity e1, Entity e2)
    {
        return e1.Equals(e2);
    }
}

public interface ISystem
{
    Type[] compRead { get; }
    Type[] compWrite { get; }
    Type[] compExclude { get; }
    Type[] rescRead { get; }
    Type[] rescWrite { get; }

    void OnCreate();
    void OnDestroy();
    void OnUpdate();
    void OnStart();
    void OnStop();
}

public class SystemDataFilter: IEnumerable
{
    private Dictionary<Type, IIndexed<IComponent>> compRead;
    private Dictionary<Type, IIndexed<IComponent>> compWrite;
    private Dictionary<Type, IComponent> rescRead;
    private Dictionary<Type, IComponent> rescWrite;

    public IIndexed<T> GetReadableCompStorage<T>()
    {
        return (IIndexed<T>)compRead[typeof(T)];
    }

    public IIndexed<T> GetWritableStorage<T>()
    {
        return (IIndexed<T>)compWrite[typeof(T)];
    }

    public T GetReadableResc<T>()
    {
        return (T)rescRead[typeof(T)];
    }

    public T GetWritableResc<T>()
    {
        return (T)rescWrite[typeof(T)];
    }

    public IEnumerator GetEnumerator()
    {
        throw new NotImplementedException();
    }
}

public class SystemDataFilterEnumerator : IEnumerator
{
    private int i;
    public object Current => i;

    public bool MoveNext()
    {
        throw new NotImplementedException();
    }

    public void Reset()
    {
        i = 0;
    }
}

public class World
{

    private Dictionary<Type, IIndexed<IComponent>> componentLUT;
    private Dictionary<Type, IComponent> resourceLUT;
    
    public Queue<Entity> deletedEntities;

    public List<List<ISystem>> systems;

    public World()
    {
        componentLUT = new Dictionary<Type, IIndexed<IComponent>>();
        resourceLUT = new Dictionary<Type, IComponent>();

        IIndexed<Entity> test = new IndexList<Entity>();
        componentLUT.Add(typeof(Entity), test);
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