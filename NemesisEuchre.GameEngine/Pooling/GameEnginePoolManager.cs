using System.Buffers;

using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Pooling;

public static class GameEnginePoolManager
{
    private static readonly ArrayPool<Card> CardPool = ArrayPool<Card>.Shared;

    public static Card[] RentCardArray(int minimumLength)
    {
        return CardPool.Rent(minimumLength);
    }

    public static void ReturnCardArray(Card[] array, bool clearArray = true)
    {
        if (array != null)
        {
            CardPool.Return(array, clearArray);
        }
    }
}
