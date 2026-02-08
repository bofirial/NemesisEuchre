namespace NemesisEuchre.GameEngine.Utilities;

public interface IRandomNumberGenerator
{
    int NextInt(int maxValue);

    int NextInt(int minValue, int maxValue);

    double NextDouble();
}

public class RandomNumberGenerator : IRandomNumberGenerator
{
    private readonly Random _random = new();

    public int NextInt(int maxValue)
    {
        return _random.Next(maxValue);
    }

    public int NextInt(int minValue, int maxValue)
    {
        return _random.Next(minValue, maxValue);
    }

    public double NextDouble()
    {
        return _random.NextDouble();
    }
}
