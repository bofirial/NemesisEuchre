using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine;

public interface IDealFactory
{
    Task<Deal> CreateDealAsync(Game game, Deal? previousDeal = null);
}
