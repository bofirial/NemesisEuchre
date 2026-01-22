using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine;

public interface IDealOrchestrator
{
    Task OrchestrateDealAsync(Deal deal, Dictionary<PlayerPosition, Player> players);
}
