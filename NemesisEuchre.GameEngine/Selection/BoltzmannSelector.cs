using NemesisEuchre.GameEngine.Utilities;

namespace NemesisEuchre.GameEngine.Selection;

public static class BoltzmannSelector
{
    public static T SelectWeighted<T>(
        IReadOnlyList<T> options,
        IReadOnlyList<float> scores,
        float temperature,
        IRandomNumberGenerator random)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(scores);
        ArgumentNullException.ThrowIfNull(random);

        if (options.Count == 0)
        {
            throw new ArgumentException("Options list cannot be empty.", nameof(options));
        }

        if (options.Count != scores.Count)
        {
            throw new ArgumentException("Options and scores must have the same count.", nameof(scores));
        }

        if (temperature <= 0)
        {
            throw new ArgumentException("Temperature must be greater than zero.", nameof(temperature));
        }

        if (options.Count == 1)
        {
            return options[0];
        }

        var weights = new double[options.Count];
        var sumWeights = 0.0;

        for (int i = 0; i < options.Count; i++)
        {
            if (float.IsNaN(scores[i]) || float.IsInfinity(scores[i]))
            {
                throw new ArgumentException($"Score at index {i} is NaN or Infinity.", nameof(scores));
            }

            weights[i] = Math.Exp(scores[i] / temperature);
            sumWeights += weights[i];
        }

        var randomValue = random.NextDouble() * sumWeights;

        var cumulative = 0.0;
        for (int i = 0; i < options.Count; i++)
        {
            cumulative += weights[i];
            if (randomValue < cumulative)
            {
                return options[i];
            }
        }

        return options[^1];
    }
}
