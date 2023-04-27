using System.Numerics;
using Vector = Common.Protobuf.Vector;

namespace Gary.Extentions;

public static class UtilsExtensions
{
    public static V GetOrDefault<K,V>(this Dictionary<K,V> self, K key, V defaultValue)
    {
        if (self.TryGetValue(key, out var v))
        {
            return v;
        }

        return defaultValue;
    }
}