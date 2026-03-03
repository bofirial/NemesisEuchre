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
        var myLogin = player.Membership.User?.GitHubLogin;

        var seats = context.Seats.ToDictionary(s => s.Position, s => s);

        var mySeat = context.Seats.FirstOrDefault(s => s.GitHubLogin == myLogin);
        var myPosition = mySeat?.Position ?? PlayerPosition.South;

        return new PlayerGameState
        {
            SessionName = context.SessionName,
            GameStatus = GameStatusViewModel.Lobby,
            MyPosition = myPosition,
            Players = new Dictionary<PlayerPosition, PlayerInfo>(),
            Team1Score = 0,
            Team2Score = 0,
            ConnectedUsers = [.. context.Members
                .Select(m => new ConnectedUserInfo
                {
                    GitHubLogin = m.Membership.User!.GitHubLogin,
                    IsSessionLeader = m.Membership.IsSessionLeader,
                })],
            Seats = seats,
        };
    }
}
