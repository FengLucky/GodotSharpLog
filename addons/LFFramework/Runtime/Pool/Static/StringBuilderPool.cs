using System.Text;

namespace LF;

public static class StringBuilderPool
{
    private static readonly BasePool<StringBuilder> Pool = new(Create);

    public static StringBuilder Get()
    {
        return Pool.Get();
    }

    public static void Release(StringBuilder builder)
    {
        builder.Clear();
        Pool.Release(builder);
    }
    
    private static StringBuilder Create()
    {
        return new StringBuilder();
    }
}