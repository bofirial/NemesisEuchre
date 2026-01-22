using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine;

public interface ITrickWinnerCalculator
{
    PlayerPosition CalculateWinner(Trick trick, Suit trump);
}
