namespace NemesisEuchre.MachineLearning.FeatureEngineering;

public abstract class FeatureEngineerBase<TEntity, TTrainingData>(FeatureBuilderBase<TEntity, TTrainingData> builder) : IFeatureEngineer<TEntity, TTrainingData>
    where TEntity : class
    where TTrainingData : class, new()
{
    public TTrainingData Transform(TEntity entity)
    {
        return builder.BuildFeatures(entity);
    }
}
