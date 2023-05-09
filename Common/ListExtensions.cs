namespace Common;

public static class ListExtensions
{
    public static T Choose<T>(this IList<T> list, Random random)
    {
        return list[random.Next(list.Count)];
    }
}