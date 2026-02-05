using System.Buffers;

using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Pooling;

public static class GameEnginePoolManager
{
    private static readonly ArrayPool<Card> CardPool = ArrayPool<Card>.Shared;
    private static readonly ArrayPool<float> FloatPool = ArrayPool<float>.Shared;
    private static readonly ArrayPool<RelativeCard> RelativeCardPool = ArrayPool<RelativeCard>.Shared;
    private static readonly ArrayPool<(Foundation.Constants.PlayerPosition, Foundation.Constants.Suit)> PlayerVoidPool = ArrayPool<(Foundation.Constants.PlayerPosition, Foundation.Constants.Suit)>.Shared;

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

    public static float[] RentFloatArray(int minimumLength)
    {
        return FloatPool.Rent(minimumLength);
    }

    public static void ReturnFloatArray(float[] array, bool clearArray = true)
    {
        if (array != null)
        {
            FloatPool.Return(array, clearArray);
        }
    }

    public static RelativeCard[] RentRelativeCardArray(int minimumLength)
    {
        return RelativeCardPool.Rent(minimumLength);
    }

    public static void ReturnRelativeCardArray(RelativeCard[] array, bool clearArray = true)
    {
        if (array != null)
        {
            RelativeCardPool.Return(array, clearArray);
        }
    }

    public static (Foundation.Constants.PlayerPosition, Foundation.Constants.Suit)[] RentPlayerVoidArray(int minimumLength)
    {
        return PlayerVoidPool.Rent(minimumLength);
    }

    public static void ReturnPlayerVoidArray((Foundation.Constants.PlayerPosition, Foundation.Constants.Suit)[] array, bool clearArray = true)
    {
        if (array != null)
        {
            PlayerVoidPool.Return(array, clearArray);
        }
    }
}
