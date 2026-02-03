namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public interface IFeatureEngineer<in TEntity, out TDto>
    where TEntity : class
    where TDto : class, new()
{
    TDto Transform(TEntity entity);
}
