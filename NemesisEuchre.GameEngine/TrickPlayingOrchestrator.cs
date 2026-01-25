using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine;

public class TrickPlayingOrchestrator(IEnumerable<IPlayerActor> playerActors) : ITrickPlayingOrchestrator
{
    private const int PlayersPerTrick = 4;
    private readonly Dictionary<ActorType, IPlayerActor> _playerActors = playerActors.ToDictionary(x => x.ActorType, x => x);

    public async Task<Trick> PlayTrickAsync(Deal deal, PlayerPosition leadPosition)
    {
        ValidatePreconditions(deal);

        var trick = InitializeTrick(leadPosition);

        await PlayAllCardsAsync(deal, trick);

        return trick;
    }

    private static void ValidatePreconditions(Deal deal)
    {
        ArgumentNullException.ThrowIfNull(deal);

        if (deal.DealStatus != DealStatus.Playing)
        {
            throw new InvalidOperationException($"Deal must be in Playing status, but was {deal.DealStatus}");
        }

        if (deal.Trump == null)
        {
            throw new InvalidOperationException("Trump must be set");
        }

        if (deal.CallingPlayer == null)
        {
            throw new InvalidOperationException("CallingPlayer must be set");
        }

        if (deal.Players.Count != PlayersPerTrick)
        {
            throw new InvalidOperationException($"Deal must have exactly {PlayersPerTrick} players, but had {deal.Players.Count}");
        }
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

    private static void ValidateCardChoice(RelativeCard chosenCard, RelativeCard[] validCards)
    {
        if (!validCards.Contains(chosenCard))
        {
            throw new InvalidOperationException("ChosenCard was not included in ValidCards");
        }
    }

    private static (short TeamScore, short OpponentScore) GetScores(Deal deal, PlayerPosition playerPosition)
    {
        var isTeam1 = playerPosition.GetTeam() == Team.Team1;
        return isTeam1 ? (deal.Team1Score, deal.Team2Score) : (deal.Team2Score, deal.Team1Score);
    }

    private static bool ShouldPlayerSit(Deal deal, PlayerPosition position)
    {
        if (!deal.CallingPlayerIsGoingAlone)
        {
            return false;
        }

        var partnerPosition = deal.CallingPlayer!.Value.GetPartnerPosition();
        return position == partnerPosition;
    }

    private static PlayerPosition GetNextActivePlayer(PlayerPosition current, Deal deal)
    {
        var next = current.GetNextPosition();
        while (ShouldPlayerSit(deal, next))
        {
            next = next.GetNextPosition();
        }

        return next;
    }

    private async Task PlayAllCardsAsync(Deal deal, Trick trick)
    {
        var currentPosition = trick.LeadPosition;
        var cardsToPlay = deal.CallingPlayerIsGoingAlone ? 3 : 4;

        for (int i = 0; i < cardsToPlay; i++)
        {
            if (ShouldPlayerSit(deal, currentPosition))
            {
                currentPosition = GetNextActivePlayer(currentPosition, deal);
                continue;
            }

            var player = deal.Players[currentPosition];
            var relativeHand = ConvertToRelativeCards(player.CurrentHand, deal.Trump!.Value);
            var validCards = GetValidCardsToPlay(
                relativeHand,
                deal.Trump!.Value,
                trick.LeadSuit);

            var chosenCard = await GetPlayerCardChoiceAsync(
                deal, currentPosition, relativeHand, validCards);
            ValidateCardChoice(chosenCard, validCards);

            if (i == 0)
            {
                trick.LeadSuit = chosenCard.Card.GetEffectiveSuit(deal.Trump.Value);
            }

            trick.CardsPlayed.Add(new PlayedCard
            {
                Card = chosenCard.Card,
                PlayerPosition = currentPosition,
            });

            player.CurrentHand.Remove(chosenCard.Card);
            currentPosition = GetNextActivePlayer(currentPosition, deal);
        }
    }

    private Task<RelativeCard> GetPlayerCardChoiceAsync(
        Deal deal,
        PlayerPosition playerPosition,
        RelativeCard[] relativeHand,
        RelativeCard[] validCards)
    {
        var player = deal.Players[playerPosition];
        var playerActor = GetPlayerActor(player);
        var (teamScore, opponentScore) = GetScores(deal, playerPosition);

        return playerActor.PlayCardAsync(
            relativeHand,
            deal.ToRelative(playerPosition),
            teamScore,
            opponentScore,
            validCards);
    }

    private IPlayerActor GetPlayerActor(DealPlayer player)
    {
        return _playerActors[player.ActorType!.Value];
    }
}
