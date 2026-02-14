namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public abstract class FeatureBuilderBase<TEntity, TTrainingData>
    where TEntity : class
    where TTrainingData : class, new()
{
    public TTrainingData BuildFeatures(TEntity entity)
    {
        ValidateEntity(entity);
        return BuildFeaturesCore(entity);
    }

    protected abstract TTrainingData BuildFeaturesCore(TEntity entity);

    protected virtual void ValidateEntity(TEntity entity)
    {
    }
}
