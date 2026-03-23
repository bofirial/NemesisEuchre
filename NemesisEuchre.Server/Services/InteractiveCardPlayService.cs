using Microsoft.Extensions.Options;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Handlers;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.Options;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Services;
using NemesisEuchre.GameEngine.Validation;

namespace NemesisEuchre.Server.Services;

public interface IInteractiveCardPlayService
{
    (PlayerPosition position, Card[] validCards)? GetCurrentCardPlayer(Deal deal);

    Task ProcessBotCardPlaysAsync(Deal deal, Game game, CancellationToken ct = default);

    Task ApplyHumanCardPlayAsync(Deal deal, Game game, PlayerPosition position, Card card, CancellationToken ct = default);
}

public class InteractiveCardPlayService(
    IGoingAloneHandler goingAloneHandler,
    ITrickWinnerCalculator trickWinnerCalculator,
    IDealResultCalculator dealResultCalculator,
    IPlayerActorResolver actorResolver,
    IPlayerContextBuilder contextBuilder,
    IDecisionRecorder decisionRecorder,
    IVoidDetector voidDetector,
    ICardAccountingService cardAccountingService,
    ITrickPlayingValidator validator,
    IDealFactory dealFactory,
    IOptions<GameOptions> gameOptions) : IInteractiveCardPlayService
{
    private const int TricksPerDeal = 5;

    public (PlayerPosition position, Card[] validCards)? GetCurrentCardPlayer(Deal deal)
    {
        if (deal.DealStatus != DealStatus.Playing)
        {
            return null;
        }

        EnsureCurrentTrick(deal);

        var trick = deal.CurrentTrick!;
        var cardsToPlay = goingAloneHandler.GetNumberOfCardsToPlay(deal);

        if (trick.CardsPlayed.Count >= cardsToPlay)
        {
            return null;
        }

        var position = GetNextPlayerPosition(deal, trick);
        var player = deal.Players[position];
        var hand = player.CurrentHand.ToArray();
        var validCards = GetValidCardsToPlay(hand, deal.Trump!.Value, trick.LeadSuit);

        return (position, validCards);
    }

    public async Task ProcessBotCardPlaysAsync(Deal deal, Game game, CancellationToken ct = default)
    {
        while (true)
        {
            ct.ThrowIfCancellationRequested();

            EnsureCurrentTrick(deal);
            var trick = deal.CurrentTrick!;
            var cardsToPlay = goingAloneHandler.GetNumberOfCardsToPlay(deal);

            if (trick.CardsPlayed.Count >= cardsToPlay)
            {
                CompleteTrick(deal);

                if (deal.CompletedTricks.Count >= TricksPerDeal)
                {
                    await FinalizeDealAsync(deal, game);
                    return;
                }

                continue;
            }

            var cardPlayer = GetCurrentCardPlayer(deal);
            if (cardPlayer is null)
            {
                return;
            }

            var dealPlayer = deal.Players[cardPlayer.Value.position];
            if (dealPlayer.Actor.ActorType == ActorType.User)
            {
                return;
            }

            await ExecuteBotCardPlayAsync(deal, trick, cardPlayer.Value.position, cardPlayer.Value.validCards);
        }
    }

    public Task ApplyHumanCardPlayAsync(
        Deal deal,
        Game game,
        PlayerPosition position,
        Card card,
        CancellationToken ct = default)
    {
        var cardPlayer = GetCurrentCardPlayer(deal);
        if (cardPlayer?.position != position)
        {
            throw new InvalidOperationException("Not your turn to play a card");
        }

        validator.ValidateCardChoice(card, cardPlayer.Value.validCards);

        var trick = deal.CurrentTrick!;
        var player = deal.Players[position];
        var hand = player.CurrentHand.ToArray();

        ApplyCardPlay(deal, trick, position, card, hand, cardPlayer.Value.validCards);

        return ProcessBotCardPlaysAsync(deal, game, ct);
    }

    private static Card[] GetValidCardsToPlay(Card[] hand, Suit trump, Suit? leadSuit)
    {
        if (leadSuit == null)
        {
            return hand;
        }

        var cardsMatchingLeadSuit = hand
            .Where(c => c.GetEffectiveSuit(trump) == leadSuit)
            .ToArray();

        return cardsMatchingLeadSuit.Length > 0
            ? cardsMatchingLeadSuit
            : hand;
    }

    private void EnsureCurrentTrick(Deal deal)
    {
        if (deal.CurrentTrick is not null)
        {
            return;
        }

        var leadPosition = deal.CompletedTricks.Count > 0
            ? deal.CompletedTricks[^1].WinningPosition!.Value
            : deal.DealerPosition!.Value.GetNextPosition();

        while (goingAloneHandler.ShouldPlayerSit(deal, leadPosition))
        {
            leadPosition = leadPosition.GetNextPosition();
        }

        deal.CurrentTrick = new Trick { LeadPosition = leadPosition };
    }

    private PlayerPosition GetNextPlayerPosition(Deal deal, Trick trick)
    {
        if (trick.CardsPlayed.Count == 0)
        {
            return trick.LeadPosition;
        }

        var lastPlayer = trick.CardsPlayed[^1].PlayerPosition;
        return goingAloneHandler.GetNextActivePlayer(lastPlayer, deal);
    }

    private async Task ExecuteBotCardPlayAsync(Deal deal, Trick trick, PlayerPosition position, Card[] validCards)
    {
        var player = deal.Players[position];
        var hand = player.CurrentHand.ToArray();
        var cardDecision = await GetBotCardChoiceAsync(deal, trick, position, hand, validCards);

        validator.ValidateCardChoice(cardDecision.ChosenCard, validCards);

        ApplyCardPlay(deal, trick, position, cardDecision.ChosenCard, hand, validCards, cardDecision);
    }

    private void ApplyCardPlay(
        Deal deal,
        Trick trick,
        PlayerPosition position,
        Card card,
        Card[] hand,
        Card[] validCards,
        CardDecisionContext? cardDecision = null)
    {
        var isFirstCard = trick.CardsPlayed.Count == 0;
        var player = deal.Players[position];

        cardDecision ??= new CardDecisionContext { ChosenCard = card };
        var recordingContext = new PlayCardRecordingContext(
            Deal: deal,
            Trick: trick,
            PlayerPosition: position,
            Hand: hand,
            ValidCards: validCards,
            CardDecisionContext: cardDecision,
            TrickWinnerCalculator: trickWinnerCalculator);

        if (validCards.Length > 1)
        {
            decisionRecorder.RecordPlayCardDecision(recordingContext);
        }

        if (voidDetector.TryDetectVoid(deal, card, trick.LeadSuit, deal.Trump!.Value, position, out var voidSuit))
        {
            deal.KnownPlayerSuitVoids.Add(new PlayerSuitVoid(position, voidSuit));
        }

        if (isFirstCard)
        {
            trick.LeadSuit = card.GetEffectiveSuit(deal.Trump!.Value);
        }

        trick.CardsPlayed.Add(new PlayedCard(card, position));
        player.CurrentHand.Remove(card);
    }

    private Task<CardDecisionContext> GetBotCardChoiceAsync(
        Deal deal,
        Trick trick,
        PlayerPosition position,
        Card[] hand,
        Card[] validCards)
    {
        var player = deal.Players[position];
        var playerActor = actorResolver.GetPlayerActor(player);
        var (teamScore, opponentScore) = contextBuilder.GetScores(deal, position);

        var playerTeam = position.GetTeam();
        var wonTricks = (short)deal.CompletedTricks.Count(t => t.WinningTeam == playerTeam);
        var opponentsWonTricks = (short)deal.CompletedTricks.Count(t => t.WinningTeam != null && t.WinningTeam != playerTeam);

        var playedCards = trick.CardsPlayed.ToDictionary(
            pc => pc.PlayerPosition,
            pc => pc.Card);

        PlayerPosition? winningTrickPlayer = null;
        if (trick.CardsPlayed.Count > 0 && trick.LeadSuit.HasValue)
        {
            winningTrickPlayer = trickWinnerCalculator.CalculateWinner(trick, deal.Trump!.Value);
        }

        var accountedForCards = cardAccountingService.GetAccountedForCards(deal, trick, position, hand);

        var context = new PlayCardContext
        {
            CardsInHand = [.. hand],
            ValidCardsToPlay = [.. validCards],
            PlayerPosition = position,
            TeamScore = teamScore,
            OpponentScore = opponentScore,
            WonTricks = wonTricks,
            OpponentsWonTricks = opponentsWonTricks,
            TrumpSuit = deal.Trump!.Value,
            CallingPlayer = deal.CallingPlayer!.Value,
            CallingPlayerIsGoingAlone = deal.CallingPlayerIsGoingAlone,
            Dealer = deal.DealerPosition!.Value,
            DealerPickedUpCard = deal.ChosenDecision is CallTrumpDecision.OrderItUp or CallTrumpDecision.OrderItUpAndGoAlone ? deal.UpCard : null,
            LeadPlayer = trick.LeadPosition,
            LeadSuit = trick.LeadSuit,
            TrickNumber = trick.TrickNumber,
            PlayedCardsInTrick = playedCards,
            CurrentlyWinningTrickPlayer = winningTrickPlayer,
            KnownPlayerSuitVoids = [.. deal.KnownPlayerSuitVoids],
            CardsAccountedFor = [.. accountedForCards],
        };

        return playerActor.PlayCardAsync(context);
    }

    private void CompleteTrick(Deal deal)
    {
        var trick = deal.CurrentTrick!;
        var winningPosition = trickWinnerCalculator.CalculateWinner(trick, deal.Trump!.Value);

        trick.TrickNumber = (short)(deal.CompletedTricks.Count + 1);
        trick.WinningPosition = winningPosition;
        trick.WinningTeam = winningPosition.GetTeam();

        deal.CompletedTricks.Add(trick);
        deal.CurrentTrick = null;
    }

    private async Task FinalizeDealAsync(Deal deal, Game game)
    {
        deal.DealStatus = DealStatus.Scoring;

        var (dealResult, winningTeam) = dealResultCalculator.CalculateDealResult(deal);
        deal.DealResult = dealResult;
        deal.WinningTeam = winningTeam;

        var scoreChange = dealResult switch
        {
            DealResult.WonStandardBid => (short)1,
            DealResult.WonGotAllTricks => (short)2,
            DealResult.OpponentsEuchred => (short)2,
            DealResult.WonAndWentAlone => (short)4,
            DealResult.ThrowIn => (short)0,
            _ => throw new ArgumentOutOfRangeException(nameof(deal), dealResult, "Unknown deal result"),
        };

        if (winningTeam == Team.Team1)
        {
            game.Team1Score += scoreChange;
        }
        else
        {
            game.Team2Score += scoreChange;
        }

        deal.Team1Score = game.Team1Score;
        deal.Team2Score = game.Team2Score;
        deal.DealStatus = DealStatus.Complete;

        game.CompletedDeals.Add(deal);

        if (game.Team1Score >= gameOptions.Value.WinningScore || game.Team2Score >= gameOptions.Value.WinningScore)
        {
            game.GameStatus = GameStatus.Complete;
            game.CurrentDeal = null;
            return;
        }

        var newDeal = await dealFactory.CreateDealAsync(game, deal);
        newDeal.DealStatus = DealStatus.SelectingTrumpPhase1;
        game.CurrentDeal = newDeal;
    }
}
