namespace LF;

public static class StaticPool<T> where T : class, IPoolable,new()
{
    private static readonly Pool<T> Pool = new(CreateItem);
    
    public static PoolItem<T> Get()
    {
        return Pool.Get();
    }
    
    public static void Release(T item)
    {
        Pool.Release(item);
    }

    private static T CreateItem()
    {
        return new T();
    }
}