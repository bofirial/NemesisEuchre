using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Extensions;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.DataAccess.Mappers;

public interface IEntityToTrickMapper
{
    Trick Map(TrickEntity entity, Suit trump, PlayerPosition dealer, bool includeDecisions);
}

public class EntityToTrickMapper : IEntityToTrickMapper
{
    public Trick Map(TrickEntity entity, Suit trump, PlayerPosition dealer, bool includeDecisions)
    {
        var trick = new Trick
        {
            TrickNumber = (short)entity.TrickNumber,
            LeadPosition = (PlayerPosition)entity.LeadPlayerPositionId,
            LeadSuit = entity.LeadSuitId.HasValue ? (Suit)entity.LeadSuitId.Value : null,
            WinningPosition = entity.WinningPlayerPositionId.HasValue ? (PlayerPosition)entity.WinningPlayerPositionId.Value : null,
            WinningTeam = entity.WinningTeamId.HasValue ? (Team)entity.WinningTeamId.Value : null,
        };

        foreach (var cardPlayed in entity.TrickCardsPlayed.OrderBy(c => c.PlayOrder))
        {
            trick.CardsPlayed.Add(new PlayedCard(
                CardIdHelper.ToCard(cardPlayed.CardId),
                (PlayerPosition)cardPlayed.PlayerPositionId));
        }

        if (includeDecisions)
        {
            trick.PlayCardDecisions = [.. entity.PlayCardDecisions.Select(d => MapPlayCardDecision(d, trump, dealer))];
        }

        return trick;
    }

    private static PlayCardDecisionRecord MapPlayCardDecision(PlayCardDecisionEntity entity, Suit trump, PlayerPosition dealer)
    {
        var selfPosition = PlayerPositionExtensions.DeriveAbsolutePosition(
            dealer,
            (RelativePlayerPosition)entity.DealerRelativePlayerPositionId);

        return new PlayCardDecisionRecord
        {
            PlayerPosition = selfPosition,
            TrumpSuit = trump,
            TeamScore = entity.TeamScore,
            OpponentScore = entity.OpponentScore,
            TrickNumber = entity.TrickNumber,
            CallingPlayerGoingAlone = entity.CallingPlayerGoingAlone,
            LeadPlayer = ((RelativePlayerPosition)entity.LeadRelativePlayerPositionId).ToAbsolutePosition(selfPosition),
            LeadSuit = entity.LeadRelativeSuitId.HasValue
                ? ((RelativeSuit)entity.LeadRelativeSuitId.Value).ToAbsoluteSuit(trump)
                : null,
            WinningTrickPlayer = entity.WinningTrickRelativePlayerPositionId.HasValue
                ? ((RelativePlayerPosition)entity.WinningTrickRelativePlayerPositionId.Value).ToAbsolutePosition(selfPosition)
                : null,
            CallingPlayer = ((RelativePlayerPosition)entity.CallingRelativePlayerPositionId).ToAbsolutePosition(selfPosition),
            Dealer = ((RelativePlayerPosition)entity.DealerRelativePlayerPositionId).ToAbsolutePosition(selfPosition),
            DealerPickedUpCard = entity.DealerPickedUpRelativeCardId.HasValue
                ? CardIdHelper.ToRelativeCard(entity.DealerPickedUpRelativeCardId.Value).ToAbsolute(trump)
                : null,
            ChosenCard = CardIdHelper.ToRelativeCard(entity.ChosenRelativeCardId).ToAbsolute(trump),
            CardsInHand = [.. entity.CardsInHand
                .OrderBy(c => c.SortOrder)
                .Select(c => CardIdHelper.ToRelativeCard(c.RelativeCardId).ToAbsolute(trump))],
            PlayedCards = entity.PlayedCards.ToDictionary(
                p => ((RelativePlayerPosition)p.RelativePlayerPositionId).ToAbsolutePosition(selfPosition),
                p => CardIdHelper.ToRelativeCard(p.RelativeCardId).ToAbsolute(trump)),
            ValidCardsToPlay = [.. entity.ValidCards
                .Select(c => CardIdHelper.ToRelativeCard(c.RelativeCardId).ToAbsolute(trump))],
            KnownPlayerSuitVoids = [.. entity.KnownVoids.Select(v => new PlayerSuitVoid(
                ((RelativePlayerPosition)v.RelativePlayerPositionId).ToAbsolutePosition(selfPosition),
                ((RelativeSuit)v.RelativeSuitId).ToAbsoluteSuit(trump)))],
            CardsAccountedFor = [.. entity.CardsAccountedFor
                .Select(c => CardIdHelper.ToRelativeCard(c.RelativeCardId).ToAbsolute(trump))],
            DecisionPredictedPoints = entity.PredictedPoints.ToDictionary(
                p => CardIdHelper.ToRelativeCard(p.RelativeCardId).ToAbsolute(trump),
                p => p.PredictedPoints),
        };
    }
}
