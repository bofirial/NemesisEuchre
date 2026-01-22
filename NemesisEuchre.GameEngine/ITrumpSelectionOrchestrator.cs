using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine;

public interface ITrumpSelectionOrchestrator
{
    Task SelectTrumpAsync(Deal deal);
}
