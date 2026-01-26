using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Handlers;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;
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
    IPlayerActorResolver actorResolver) : ITrickPlayingOrchestrator
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

    private static RelativeCard[] GetValidCardsToPlay(
        RelativeCard[] relativeHand,
        Suit trump,
        Suit? leadSuit)
    {
        if (leadSuit == null)
        {
            return relativeHand;
        }

        var cardsMatchingLeadSuit = relativeHand
            .Where(rc => rc.Card.GetEffectiveSuit(trump) == leadSuit)
            .ToArray();

        return cardsMatchingLeadSuit.Length > 0
            ? cardsMatchingLeadSuit
            : relativeHand;
    }

    private static RelativeCard[] ConvertToRelativeCards(List<Card> cards, Suit trump)
    {
        return [.. cards.Select(c => c.ToRelative(trump))];
    }

    private static void SetLeadSuitIfFirstCard(Trick trick, RelativeCard chosenCard, Suit trump, bool isFirstCard)
    {
        if (isFirstCard)
        {
            trick.LeadSuit = chosenCard.Card.GetEffectiveSuit(trump);
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

        for (int i = 0; i < cardsToPlay; i++)
        {
            if (goingAloneHandler.ShouldPlayerSit(deal, currentPosition))
            {
                currentPosition = goingAloneHandler.GetNextActivePlayer(currentPosition, deal);
                continue;
            }

            var isFirstCard = i == 0;
            await PlaySingleCardAsync(deal, trick, currentPosition, isFirstCard);
            currentPosition = goingAloneHandler.GetNextActivePlayer(currentPosition, deal);
        }
    }

    private async Task PlaySingleCardAsync(Deal deal, Trick trick, PlayerPosition position, bool isFirstCard)
    {
        var player = deal.Players[position];
        var relativeHand = ConvertToRelativeCards(player.CurrentHand, deal.Trump!.Value);
        var validCards = GetValidCardsToPlay(relativeHand, deal.Trump!.Value, trick.LeadSuit);

        var chosenCard = await GetPlayerCardChoiceAsync(deal, position, relativeHand, validCards);
        validator.ValidateCardChoice(chosenCard, validCards);

        SetLeadSuitIfFirstCard(trick, chosenCard, deal.Trump!.Value, isFirstCard);
        RecordPlayedCard(trick, chosenCard.Card, position);
        UpdateHandAfterPlay(player, chosenCard.Card);
    }

    private Task<RelativeCard> GetPlayerCardChoiceAsync(
        Deal deal,
        PlayerPosition playerPosition,
        RelativeCard[] relativeHand,
        RelativeCard[] validCards)
    {
        var player = deal.Players[playerPosition];
        var playerActor = actorResolver.GetPlayerActor(player);
        var (teamScore, opponentScore) = contextBuilder.GetScores(deal, playerPosition);

        return playerActor.PlayCardAsync(
            relativeHand,
            deal.ToRelative(playerPosition),
            teamScore,
            opponentScore,
            validCards);
    }
}
