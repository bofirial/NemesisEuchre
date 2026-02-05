using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Handlers;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
using NemesisEuchre.GameEngine.Pooling;
using NemesisEuchre.GameEngine.Services;
using NemesisEuchre.GameEngine.Validation;

namespace NemesisEuchre.GameEngine;

public interface ITrickPlayingOrchestrator
{
    Task<Trick> PlayTrickAsync(Deal deal, PlayerPosition leadPosition);
}

public class TrickPlayingOrchestrator(
    ITrickPlayingValidator validator,
    IGoingAloneHandler goingAloneHandler,
    IPlayerContextBuilder contextBuilder,
    IPlayerActorResolver actorResolver,
    ITrickWinnerCalculator trickWinnerCalculator,
    IDecisionRecorder decisionRecorder,
    IVoidDetector voidDetector,
    ICardAccountingService cardAccountingService) : ITrickPlayingOrchestrator
{
    public async Task<Trick> PlayTrickAsync(Deal deal, PlayerPosition leadPosition)
    {
        validator.ValidatePreconditions(deal);

        var trick = InitializeTrick(leadPosition);

        await ExecuteTrickCardPlaysAsync(deal, trick);

        return trick;
    }

    private static Trick InitializeTrick(PlayerPosition leadPosition)
    {
        return new Trick
        {
            LeadPosition = leadPosition,
        };
    }

    private static Card[] GetValidCardsToPlay(
        Card[] hand,
        Suit trump,
        Suit? leadSuit)
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

    private static void SetLeadSuitIfFirstCard(Trick trick, Card chosenCard, Suit trump, bool isFirstCard)
    {
        if (isFirstCard)
        {
            trick.LeadSuit = chosenCard.GetEffectiveSuit(trump);
        }
    }

    private static void RecordPlayedCard(Trick trick, Card card, PlayerPosition position)
    {
        trick.CardsPlayed.Add(new PlayedCard
        {
            Card = card,
            PlayerPosition = position,
        });
    }

    private static void UpdateHandAfterPlay(DealPlayer player, Card card)
    {
        player.CurrentHand.Remove(card);
    }

    private async Task ExecuteTrickCardPlaysAsync(Deal deal, Trick trick)
    {
        var currentPosition = trick.LeadPosition;
        var cardsToPlay = goingAloneHandler.GetNumberOfCardsToPlay(deal);

        while (trick.CardsPlayed.Count < cardsToPlay)
        {
            if (goingAloneHandler.ShouldPlayerSit(deal, currentPosition))
            {
                currentPosition = goingAloneHandler.GetNextActivePlayer(currentPosition, deal);
                continue;
            }

            var isFirstCard = trick.CardsPlayed.Count == 0;
            await PlaySingleCardAsync(deal, trick, currentPosition, isFirstCard);
            currentPosition = goingAloneHandler.GetNextActivePlayer(currentPosition, deal);
        }
    }

    private async Task PlaySingleCardAsync(Deal deal, Trick trick, PlayerPosition position, bool isFirstCard)
    {
        var player = deal.Players[position];
        var handCount = player.CurrentHand.Count;
        var hand = GameEnginePoolManager.RentCardArray(handCount);

        try
        {
            player.CurrentHand.CopyTo(hand, 0);
            var handArray = new Card[handCount];
            Array.Copy(hand, handArray, handCount);
            var validCards = GetValidCardsToPlay(handArray, deal.Trump!.Value, trick.LeadSuit);

            var chosenCard = await GetPlayerCardChoiceAsync(deal, trick, position, handArray, validCards).ConfigureAwait(false);
            decisionRecorder.RecordPlayCardDecision(deal, trick, position, handArray, validCards, chosenCard, trickWinnerCalculator);
            validator.ValidateCardChoice(chosenCard, validCards);

            if (voidDetector.TryDetectVoid(deal, chosenCard, trick.LeadSuit, deal.Trump!.Value, position, out var voidSuit))
            {
                deal.KnownPlayerSuitVoids.Add((position, voidSuit));
            }

            SetLeadSuitIfFirstCard(trick, chosenCard, deal.Trump!.Value, isFirstCard);
            RecordPlayedCard(trick, chosenCard, position);
            UpdateHandAfterPlay(player, chosenCard);
        }
        finally
        {
            GameEnginePoolManager.ReturnCardArray(hand);
        }
    }

    private Task<Card> GetPlayerCardChoiceAsync(
        Deal deal,
        Trick trick,
        PlayerPosition playerPosition,
        Card[] hand,
        Card[] validCards)
    {
        var player = deal.Players[playerPosition];
        var playerActor = actorResolver.GetPlayerActor(player);
        var (teamScore, opponentScore) = contextBuilder.GetScores(deal, playerPosition);

        var playedCards = trick.CardsPlayed.ToDictionary(
            pc => pc.PlayerPosition,
            pc => pc.Card);

        PlayerPosition? winningTrickPlayer = null;
        if (trick.CardsPlayed.Count > 0 && trick.LeadSuit.HasValue)
        {
            winningTrickPlayer = trickWinnerCalculator.CalculateWinner(trick, deal.Trump!.Value);
        }

        var accountedForCards = cardAccountingService.GetAccountedForCards(
            deal,
            trick,
            playerPosition,
            hand);

        var context = new PlayCardContext
        {
            CardsInHand = [.. hand],
            ValidCardsToPlay = [.. validCards],
            PlayerPosition = playerPosition,
            TeamScore = teamScore,
            OpponentScore = opponentScore,
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
}
