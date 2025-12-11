using System.Collections.Generic;

namespace LF;

public class ListPool<T> : CollectionPool<List<T>, T>;
public class DictionaryPool<TKey,TValue>:CollectionPool<Dictionary<TKey,TValue>,KeyValuePair<TKey,TValue>>;
public class HashSetPool<T>:CollectionPool<HashSet<T>,T>;

public class CollectionPool<TCollection, TItem> where TCollection : class, ICollection<TItem>, new()
{
    private static readonly BasePool<TCollection> Pool = new(Create, actionOnRelease: OnRelease);

    public static TCollection Get() => Pool.Get();

    public static CollectionPoolItem<TCollection> GetItem()
    {
        return new(Pool);
    }

    private static TCollection Create() => new ();

    private static void OnRelease(TCollection collection)
    {
        collection.Clear();
    }

    public static void Release(TCollection toRelease)
    {
        Pool.Release(toRelease);
    }
}
