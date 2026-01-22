namespace NemesisEuchre.GameEngine;

public interface ICardShuffler
{
    void Shuffle<T>(T[] array);
}
