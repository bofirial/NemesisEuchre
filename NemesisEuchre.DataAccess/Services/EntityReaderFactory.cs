using System.Data.Common;

namespace NemesisEuchre.DataAccess.Services;

public interface IEntityReaderFactory
{
    DbDataReader CreateReader<T>(IReadOnlyList<T> entities);
}

public sealed class EntityReaderFactory : IEntityReaderFactory
{
    private readonly Dictionary<Type, Func<object, DbDataReader>> _readerFactories = [];

    public void Register<T>(Func<IReadOnlyList<T>, DbDataReader> factory)
    {
        _readerFactories[typeof(T)] = items => factory((IReadOnlyList<T>)items);
    }

    public DbDataReader CreateReader<T>(IReadOnlyList<T> entities)
    {
        if (!_readerFactories.TryGetValue(typeof(T), out var factory))
        {
            throw new InvalidOperationException($"No reader registered for type {typeof(T).Name}");
        }

        return factory(entities);
    }
}
