using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Handlers;
using NemesisEuchre.GameEngine.Models;
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
    ITrickWinnerCalculator trickWinnerCalculator) : ITrickPlayingOrchestrator
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
        var hand = player.CurrentHand.ToArray();
        var validCards = GetValidCardsToPlay(hand, deal.Trump!.Value, trick.LeadSuit);

        var chosenCard = await GetPlayerCardChoiceAsync(deal, trick, position, hand, validCards);
        CapturePlayCardDecision(deal, position, hand, validCards, trick, chosenCard);
        validator.ValidateCardChoice(chosenCard, validCards);

        SetLeadSuitIfFirstCard(trick, chosenCard, deal.Trump!.Value, isFirstCard);
        RecordPlayedCard(trick, chosenCard, position);
        UpdateHandAfterPlay(player, chosenCard);
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

        return playerActor.PlayCardAsync(
            [.. hand],
            playerPosition,
            teamScore,
            opponentScore,
            deal.Trump!.Value,
            trick.LeadPosition,
            trick.LeadSuit,
            playedCards,
            winningTrickPlayer,
            [.. validCards]);
    }

    private void CapturePlayCardDecision(
        Deal deal,
        PlayerPosition playerPosition,
        Card[] hand,
        Card[] validCards,
        Trick trick,
        Card chosenCard)
    {
        var (teamScore, opponentScore) = contextBuilder.GetScores(deal, playerPosition);

        var trickSnapshot = new Trick
        {
            LeadPosition = trick.LeadPosition,
            LeadSuit = trick.LeadSuit,
        };

        trickSnapshot.CardsPlayed.AddRange(trick.CardsPlayed);

        var record = new PlayCardDecisionRecord
        {
            Hand = [.. hand],
            DecidingPlayerPosition = playerPosition,
            CurrentTrick = trickSnapshot,
            TeamScore = teamScore,
            OpponentScore = opponentScore,
            ValidCardsToPlay = [.. validCards],
            ChosenCard = chosenCard,
            LeadPosition = trick.LeadPosition,
        };

        deal.PlayCardDecisions.Add(record);
    }
}
