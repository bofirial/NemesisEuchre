using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.PlayerBots;

public interface IPlayerBot : IPlayerActor
{
    BotType BotType { get; }
}
