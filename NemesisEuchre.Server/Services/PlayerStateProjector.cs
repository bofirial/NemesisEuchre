using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.Server.Models;

namespace NemesisEuchre.Server.Services;

public interface IPlayerStateProjector
{
    PlayerGameState Project(GameContext context, ActiveSessionMember player);
}

public class PlayerStateProjector(IInteractiveTrumpService interactiveTrumpService) : IPlayerStateProjector
{
    public PlayerGameState Project(GameContext context, ActiveSessionMember player)
    {
        var myLogin = player.Membership.User?.GitHubLogin;

        var seats = context.Seats.ToDictionary(s => s.Position, s => s);

        var mySeat = context.Seats.FirstOrDefault(s => s.GitHubLogin == myLogin);
        var myPosition = mySeat?.Position ?? PlayerPosition.South;

        var currentDeal = context.ActiveGame?.CurrentDeal;

        return new PlayerGameState
        {
            SessionName = context.SessionName,
            GameStatus = context.Status,
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
            CurrentDeal = currentDeal is not null ? MapDealState(currentDeal, myPosition) : null,
        };
    }

    private DealState MapDealState(Deal deal, PlayerPosition myPosition)
    {
        var myHand = deal.Players.TryGetValue(myPosition, out var dealPlayer)
            ? (IReadOnlyList<Card>)dealPlayer.CurrentHand
            : [];

        var otherHandCounts = deal.Players
            .Where(p => p.Key != myPosition)
            .ToDictionary(p => p.Key, p => p.Value.CurrentHand.Count);

        var trumpDecider = interactiveTrumpService.GetCurrentTrumpDecider(deal);
        var discardDecider = interactiveTrumpService.GetCurrentDiscardDecider(deal);

        var currentDeciderPosition = trumpDecider?.position ?? discardDecider?.position;

        return new DealState
        {
            DealStatus = deal.DealStatus,
            DealerPosition = deal.DealerPosition,
            UpCard = deal.UpCard,
            Trump = deal.Trump,
            CallingPlayer = deal.CallingPlayer,
            CallingPlayerIsGoingAlone = deal.CallingPlayerIsGoingAlone,
            MyHand = myHand,
            OtherHandCounts = otherHandCounts,
            CompletedTricks = [.. deal.CompletedTricks.Select(t => new CompletedTrickInfo
            {
                TrickNumber = t.TrickNumber,
                LeadPosition = t.LeadPosition,
                CardsPlayed = [.. t.CardsPlayed],
                WinningPosition = t.WinningPosition!.Value,
                WinningTeam = t.WinningTeam!.Value,
            })],
            CurrentDeciderPosition = currentDeciderPosition,
            ValidTrumpDecisions = trumpDecider?.position == myPosition
                ? trumpDecider.Value.validDecisions
                : null,
            ValidDiscardCards = discardDecider?.position == myPosition
                ? discardDecider.Value.validCards
                : null,
        };
    }
}
