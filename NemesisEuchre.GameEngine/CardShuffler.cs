namespace NemesisEuchre.GameEngine;

public class CardShuffler : ICardShuffler
{
    private readonly Random _random = new();

    public void Shuffle<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}
