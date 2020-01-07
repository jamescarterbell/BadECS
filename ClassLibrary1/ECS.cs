using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

#region Storages

public interface IIndexed { }

public interface IIndexed<T> : IIndexed
{
    T this[int index] { get; set; }
    BitArray validBits { get; }
    Density density { get; }
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
    Density IIndexed<T>.density => Density.sparse;

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
    Density IIndexed<T>.density => Density.dense;

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
    Density IIndexed<T>.density => Density.non;

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

public enum Density
{
    dense = 2, sparse = 1, non = 0
}

#endregion

/// <summary>
/// The tag that all structs need in order to be recognized by the ECS system.
/// Can be used on classes as well, but this will incur a large performance hit.
/// </summary>
public interface IComponent{};

#region Entities
/// <summary>
/// Contains the array position for all components of the entity, as well as it's generation number.
/// </summary>
public struct Entity : IEquatable<Entity>
{
    public int id;
    public int generation;

    internal Entity(int _gen, int _id)
    {
        generation = _gen;
        id = _id;
    }

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

public class EntityManager
{
    private IndexList<Entity> entities;
    private Queue<Entity> deletedEntities;
    private World world;
    private int currentEntity;

    internal EntityManager(World _world)
    {
        world = _world;
        entities = new IndexList<Entity>();
        deletedEntities = new Queue<Entity>();
        currentEntity = 0;
    }

    public EntityConstructor NewEntity(Dictionary<Type, IIndexed<IComponent>> compWrite, Dictionary<Type, IIndexed<IComponent>> compAddable)
    {
        Entity newEntity;
        if(deletedEntities.Count > 0)
        {
            newEntity = deletedEntities.Dequeue();
            newEntity.generation++;
        }
        else
        {
            newEntity = new Entity(0, ++currentEntity);
        }
        entities.Insert(currentEntity, newEntity);
        return new EntityConstructor(currentEntity, compWrite, compAddable);
    }

    public EntityConstructor AddToEntity(int i, Dictionary<Type, IIndexed<IComponent>> compWrite, Dictionary<Type, IIndexed<IComponent>> compAddable)
    {
        return new EntityConstructor(i, compWrite, compAddable);
    }

    public void DestroyEntity(int i)
    {
        foreach(IIndexed<IComponent> indexed in world.componentLUT.Values)
        {
            indexed.Remove(i);
        }
        deletedEntities.Enqueue(entities[i]);
        entities.Remove(i);
    }

    public void DestroyEntity(Entity e)
    {
        int index = e.id;
        foreach (IIndexed<IComponent> indexed in world.componentLUT.Values)
        {
            indexed.Remove(index);
        }
        deletedEntities.Enqueue(entities[index]);
        entities.Remove(index);
    }
}

public class EntityConstructor
{
    private int i;
    private Dictionary<Type, IIndexed<IComponent>> compWrite;
    private Dictionary<Type, IIndexed<IComponent>> compAddable;

    internal EntityConstructor(int _i, Dictionary<Type, IIndexed<IComponent>> _compWrite, Dictionary<Type, IIndexed<IComponent>> _compAddable)
    {
        compAddable = _compAddable;
        compWrite = _compWrite;
        i = _i;
    }

    public EntityConstructor With<T>(T component)
    {
        if(compAddable.ContainsKey(typeof(T)))
            compAddable[typeof(T)].Insert(i, (IComponent)component);
        else
            compWrite[typeof(T)].Insert(i, (IComponent)component);
        return this;
    }

    public EntityConstructor Remove<T>(T component)
    {
        compWrite[typeof(T)].Remove(i);
        return this;
    }
}

#endregion

#region Systems

public interface ISystem
{
    Type[] compRead { get; }
    Type[] compWrite { get; }
    Type[] compAddable { get; }
    Type[] compExclude { get; }
    Type[] rescRead { get; }
    Type[] rescWrite { get; }
    bool entityAccess { get; }
    bool entityDeleteAccess { get; }

    void Create();
    void Destroy();
    void Update(SystemDataFilter filter);
    void Start();
    void Stop();
}

public class SystemDataFilter: IEnumerator
{
    private Dictionary<Type, IIndexed<IComponent>> compRead;
    private Dictionary<Type, IIndexed<IComponent>> compWrite;
    private Dictionary<Type, IIndexed<IComponent>> compAddable;
    private Dictionary<Type, IComponent> rescRead;
    private Dictionary<Type, IComponent> rescWrite;
    private BitArray validEntities;
    private bool entityAccess;
    private bool entityDeleteAccess;
    private EntityManager entityManager;

    private int i;
    public object Current => GetCurrentEntity();

    internal SystemDataFilter(World _world, Type[] _compRead, Type[] _compWrite, Type[] _compAddable, Type[] _compExclude, Type[] _rescRead, Type[] _rescWrite, bool _entityAccess, bool _entityDeleteAccess)
    {
        compRead = new Dictionary<Type, IIndexed<IComponent>>();
        compWrite = new Dictionary<Type, IIndexed<IComponent>>();
        compAddable = new Dictionary<Type, IIndexed<IComponent>>();
        rescRead = new Dictionary<Type, IComponent>();
        rescWrite = new Dictionary<Type, IComponent>();

        entityAccess = _entityAccess;
        entityDeleteAccess = _entityDeleteAccess;
        if(entityAccess || entityDeleteAccess)
        {
            entityManager = _world.entityManager;
        }
        
        foreach(Type t in _compRead)
        {
            compRead.Add(t, _world.componentLUT[t]);
            validEntities = (validEntities ?? compRead[t].validBits).And(compRead[t].validBits);
        }
        foreach (Type t in _compWrite)
        {
            compWrite.Add(t, _world.componentLUT[t]);
            validEntities = (validEntities ?? compWrite[t].validBits).And(compWrite[t].validBits);
        }
        foreach (Type t in _compAddable)
        {
            compAddable.Add(t, _world.componentLUT[t]);
        }
        foreach (Type t in _compExclude)
        {
            validEntities = (validEntities ?? _world.componentLUT[t].validBits.Not()).And(_world.componentLUT[t].validBits.Not());
        }
        foreach (Type t in _rescRead)
        {
            rescRead.Add(t, _world.resourceLUT[t]);
        }
        foreach (Type t in _rescWrite)
        {
            rescWrite.Add(t, _world.resourceLUT[t]);
        }

        Reset();
    }

    public T GetReadableComp<T>()
    {
        return (T)compRead[typeof(T)][i];
    }

    public T GetWritableComp<T>()
    {
        return (T)compWrite[typeof(T)][i];
    }

    public void SetWritableComp<T>(T set)
    {
        compWrite[typeof(T)][i] = (IComponent)set;
    }

    public T GetReadableResc<T>()
    {
        return (T)rescRead[typeof(T)];
    }

    public T GetWritableResc<T>()
    {
        return (T)rescWrite[typeof(T)];
    }

    public void SetWritableResc<T>(T set)
    {
        rescWrite[typeof(T)] = (IComponent)set;
    }

    public EntityConstructor GetCurrentEntity()
    {
        if (entityAccess) return entityManager.AddToEntity(i, compWrite, compAddable);
        throw new NoEntityAccessError();
    }

    public EntityConstructor CreateNewEntity()
    {
        if (entityAccess) return entityManager.NewEntity(compWrite, compAddable);
        throw new NoEntityAccessError();
    }

    public void DestroyCurrentEntity()
    {
        if(entityDeleteAccess) entityManager.DestroyEntity(i);
        throw new NoEntityDeleteAccessError();
    }

    public void DestroyEntity(Entity e)
    {
        if (entityDeleteAccess) entityManager.DestroyEntity(e);
        throw new NoEntityDeleteAccessError();
    }

    public bool MoveNext()
    {
        do
        {
            i++;
        }while (!validEntities[i] && i < validEntities.Count) ;
        if(i < validEntities.Count)
            return true;
        return false;
    }

    public void Reset()
    {
        i = -1;
        MoveNext();
    }
}

public class SystemCollection
{
    internal List<ISystem> systems;

    internal Dictionary<Type, IIndexed<IComponent>> components;
    internal Dictionary<Type, IComponent> resources;

    public void RegisterComponent<T, U>()
        where T : IIndexed<U>, new()
        where U : IComponent
    {
        components.Add(typeof(U), new T() as IIndexed<IComponent>);
    }
}

public class NoEntityAccessError : Exception{};
public class NoEntityDeleteAccessError : Exception { };

#endregion

public class World
{
    internal Dictionary<Type, IIndexed<IComponent>> componentLUT;
    internal Dictionary<Type, IComponent> resourceLUT;

    internal EntityManager entityManager;
    

    public World()
    {
        componentLUT = new Dictionary<Type, IIndexed<IComponent>>();
        resourceLUT = new Dictionary<Type, IComponent>();

        entityManager = new EntityManager(this);
    }

    public void RegisterComponent<T, U>()
        where T: IIndexed<U>, new()
        where U: IComponent
    {
        T newStorage = new T();
        if (componentLUT.ContainsKey(typeof(U)))
        {
            componentLUT[typeof(U)] = (newStorage.density > componentLUT[typeof(U)].density) ? newStorage as IIndexed<IComponent> : componentLUT[typeof(U)];
        }
        else
        {
            componentLUT.Add(typeof(U), new T() as IIndexed<IComponent>);
        }
    }

    public void RegisterResource<T>()
        where T: IComponent, new()
    {
        if(!resourceLUT.ContainsKey(typeof(T)))
            resourceLUT.Add(typeof(T), new T());
    }

    internal void RunUpdateSystems(ISystem[] systems)
    {
        List<Thread> threads = new List<Thread>();
        foreach(ISystem s in systems)
        {
            SystemDataFilter sFilter = new SystemDataFilter(this, s.compRead, s.compWrite, s.compAddable, s.compExclude, s.rescRead, s.rescWrite, s.entityAccess, s.entityDeleteAccess);
            Thread sThread = new Thread(delegate ()
            {
                s.Update(sFilter);
            });
            sThread.Start();
            threads.Add(sThread);
        }
        foreach(Thread t in threads)
        {
            t.Join();
        }
    }
}
