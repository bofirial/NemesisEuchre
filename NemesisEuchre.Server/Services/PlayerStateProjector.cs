using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.Server.Models;
using NemesisEuchre.Server.Models.Events;

namespace NemesisEuchre.Server.Services;

public interface IPlayerStateProjector
{
    PlayerGameState Project(GameContext context, ActiveSessionMember player);
}

public class PlayerStateProjector(
    IInteractiveTrumpService interactiveTrumpService,
    IInteractiveCardPlayService interactiveCardPlayService) : IPlayerStateProjector
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
            Team1Score = context.ActiveGame?.Team1Score ?? 0,
            Team2Score = context.ActiveGame?.Team2Score ?? 0,
            ConnectedUsers = [.. context.Members
                .Select(m => new ConnectedUserInfo
                {
                    GitHubLogin = m.Membership.User!.GitHubLogin,
                    IsSessionLeader = m.Membership.IsSessionLeader,
                })],
            Seats = seats,
            CurrentDeal = currentDeal is not null ? MapDealState(currentDeal, myPosition) : null,
            Events = context.ActiveGame is not null
                ? ProjectEvents(context.ActiveGame.GameEvents, myPosition)
                : [],
        };
    }

    private static IReadOnlyList<PlayerGameEvent> ProjectEvents(
        List<GameEvent> gameEvents,
        PlayerPosition myPosition)
    {
        return [.. gameEvents.Select(e => ProjectEvent(e, myPosition))];
    }

    private static PlayerGameEvent ProjectEvent(GameEvent e, PlayerPosition myPosition)
    {
        return e switch
        {
            NewDealStartedEvent ev => new PlayerNewDealStartedEvent(
                ev.EventIndex,
                ev.DealNumber,
                ev.DealerPosition,
                ev.UpCard,
                ev.Hands.TryGetValue(myPosition, out var hand) ? hand : []),
            TrumpDecisionMadeEvent ev => new PlayerTrumpDecisionMadeEvent(
                ev.EventIndex, ev.Position, ev.Decision),
            UpCardFlippedEvent ev => new PlayerUpCardFlippedEvent(ev.EventIndex),
            UpCardPickedUpEvent ev => new PlayerUpCardPickedUpEvent(ev.EventIndex, ev.DealerPosition),
            DealerDiscardedEvent ev => new PlayerDealerDiscardedEvent(
                ev.EventIndex,
                ev.Position,
                ev.Position == myPosition ? ev.Card : null),
            CardPlayedEvent ev => new PlayerCardPlayedEvent(ev.EventIndex, ev.Position, ev.Card),
            TrickCompletedEvent ev => new PlayerTrickCompletedEvent(
                ev.EventIndex, ev.TrickNumber, ev.WinnerPosition, ev.WinningTeam),
            DealCompletedEvent ev => new PlayerDealCompletedEvent(
                ev.EventIndex, ev.DealNumber, ev.Result, ev.WinningTeam, ev.PointsAwarded, ev.Team1Score, ev.Team2Score, ev.CallingPlayer, ev.WinningTeamTrickCount),
            GameCompletedEvent ev => new PlayerGameCompletedEvent(
                ev.EventIndex, ev.WinningTeam, ev.Team1Score, ev.Team2Score),
            WaitingForDecisionEvent ev => new PlayerWaitingForDecisionEvent(ev.EventIndex, ev.Position),
            _ => throw new ArgumentOutOfRangeException(nameof(e), e.GetType().Name, "Unknown game event type"),
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
        var cardPlayer = interactiveCardPlayService.GetCurrentCardPlayer(deal);

        var currentDeciderPosition = trumpDecider?.position ?? discardDecider?.position ?? cardPlayer?.position;

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
            CurrentTrickCards = [.. deal.CurrentTrick?.CardsPlayed ?? []],
            CurrentDeciderPosition = currentDeciderPosition,
            ValidTrumpDecisions = trumpDecider?.position == myPosition
                ? trumpDecider.Value.validDecisions
                : null,
            ValidDiscardCards = discardDecider?.position == myPosition
                ? discardDecider.Value.validCards
                : null,
            ValidCardsToPlay = cardPlayer?.position == myPosition
                ? cardPlayer.Value.validCards
                : null,
            TrumpDecisions = [.. deal.CallTrumpDecisions.Select(d => new TrumpDecisionInfo
            {
                Position = d.PlayerPosition,
                Decision = d.ChosenDecision,
            })],
            DealResult = deal.DealResult,
            WinningTeam = deal.WinningTeam,
        };
    }
}
