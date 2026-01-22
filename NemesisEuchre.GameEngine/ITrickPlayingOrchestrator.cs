using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine;

public interface ITrickPlayingOrchestrator
{
    Task<Trick> PlayTrickAsync(Deal deal, PlayerPosition leadPosition);
}
