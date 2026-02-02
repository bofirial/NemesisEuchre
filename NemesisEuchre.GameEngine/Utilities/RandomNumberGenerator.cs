namespace NemesisEuchre.GameEngine.Utilities;

public interface IRandomNumberGenerator
{
    int NextInt(int maxValue);

    int NextInt(int minValue, int maxValue);
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
}
