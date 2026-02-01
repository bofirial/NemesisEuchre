using System.Collections.Concurrent;
using System.Reflection;

using Microsoft.ML.Data;

namespace NemesisEuchre.MachineLearning.Utilities;

public static class FeatureColumnProvider
{
    private static readonly ConcurrentDictionary<Type, string[]> Cache = new();

    public static string[] GetFeatureColumns<TData>(Func<string, bool>? columnFilter = null)
        where TData : class, new()
    {
        var columns = Cache.GetOrAdd(typeof(TData), DiscoverFeatureColumns);

        if (columnFilter != null)
        {
            columns = [.. columns.Where(columnFilter)];
        }

        return columns;
    }

    private static string[] DiscoverFeatureColumns(Type dataType)
    {
        var properties = dataType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        var featureColumns = properties
            .Select(p => new
            {
                Property = p,
                LoadColumn = p.GetCustomAttribute<LoadColumnAttribute>(),
            })
            .Where(x => x.LoadColumn != null && x.Property.Name != "ExpectedDealPoints")
            .OrderBy(x => GetColumnIndex(x.LoadColumn!))
            .Select(x => x.Property.Name)
            .ToArray();

        if (featureColumns.Length == 0)
        {
            throw new InvalidOperationException(
                $"No feature columns found on type {dataType.Name}. " +
                "Ensure properties have [LoadColumn] attributes.");
        }

        return featureColumns;
    }

    private static int GetColumnIndex(LoadColumnAttribute attribute)
    {
        var type = attribute.GetType();
        var indexProperty = type.GetProperty("Start");
        return indexProperty != null ? (int)indexProperty.GetValue(attribute)! : 0;
    }
}
