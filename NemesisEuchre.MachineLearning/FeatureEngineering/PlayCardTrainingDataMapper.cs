using System.Collections.Concurrent;
using System.Reflection;

using NemesisEuchre.MachineLearning.Models;

namespace NemesisEuchre.MachineLearning.FeatureEngineering;

internal static class PlayCardTrainingDataMapper
{
    private static readonly ConcurrentDictionary<Type, (PropertyInfo source, PropertyInfo target)[]> PropertyPairCache = new();

    public static T MapFrom<T>(AllPlayCardTrainingData source)
        where T : class, new()
    {
        var target = new T();
        var pairs = PropertyPairCache.GetOrAdd(typeof(T), BuildPropertyPairs);

        foreach (var (src, tgt) in pairs)
        {
            tgt.SetValue(target, src.GetValue(source));
        }

        return target;
    }

    private static (PropertyInfo source, PropertyInfo target)[] BuildPropertyPairs(Type targetType)
    {
        var sourceProps = typeof(AllPlayCardTrainingData)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToDictionary(p => p.Name);

        return [.. targetType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite && sourceProps.ContainsKey(p.Name))
            .Select(p => (source: sourceProps[p.Name], target: p))];
    }
}
