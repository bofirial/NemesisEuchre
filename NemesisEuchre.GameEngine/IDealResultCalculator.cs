using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine;

public interface IDealResultCalculator
{
    (DealResult DealResult, Team WinningTeam) CalculateDealResult(Deal deal);
}
