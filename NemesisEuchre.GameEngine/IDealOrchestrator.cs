using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine;

public interface IDealOrchestrator
{
    Task OrchestrateDealAsync(Deal deal, Player[] players);
}
