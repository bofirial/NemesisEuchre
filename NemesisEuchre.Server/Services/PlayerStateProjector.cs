using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.Server.Models;

namespace NemesisEuchre.Server.Services;

public interface IPlayerStateProjector
{
    PlayerGameState Project(GameContext context, ActiveSessionMember player);
}

public class PlayerStateProjector : IPlayerStateProjector
{
    public PlayerGameState Project(GameContext context, ActiveSessionMember player)
    {
        return new PlayerGameState
        {
            SessionName = context.SessionName,
            GameStatus = GameStatusViewModel.Lobby,
            MyPosition = PlayerPosition.South,
            Players = new Dictionary<PlayerPosition, PlayerInfo>(),
            Team1Score = 0,
            Team2Score = 0,
            ConnectedUsers = [.. context.Members
                .Select(m => new ConnectedUserInfo
                {
                    GitHubLogin = m.Membership.User!.GitHubLogin,
                    IsSessionLeader = m.Membership.IsSessionLeader,
                })],
        };
    }
}
