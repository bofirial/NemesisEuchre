using NemesisEuchre.DataAccess.Entities;

namespace NemesisEuchre.DataAccess.Services;

public class LeafCollectionCache
{
    private readonly List<(GameEntity parent, GamePlayer leaf)> _gamePlayers = [];
    private readonly List<(DealEntity parent, DealDeckCard leaf)> _dealDeckCards = [];
    private readonly List<(DealEntity parent, DealKnownPlayerSuitVoid leaf)> _dealKnownPlayerSuitVoids = [];
    private readonly List<(DealPlayerEntity parent, DealPlayerStartingHandCard leaf)> _dealPlayerStartingHandCards = [];
    private readonly List<(TrickEntity parent, TrickCardPlayed leaf)> _trickCardsPlayed = [];
    private readonly List<(CallTrumpDecisionEntity parent, CallTrumpDecisionCardsInHand leaf)> _callTrumpCardsInHand = [];
    private readonly List<(CallTrumpDecisionEntity parent, CallTrumpDecisionValidDecision leaf)> _callTrumpValidDecisions = [];
    private readonly List<(CallTrumpDecisionEntity parent, CallTrumpDecisionPredictedPoints leaf)> _callTrumpPredictedPoints = [];
    private readonly List<(DiscardCardDecisionEntity parent, DiscardCardDecisionCardsInHand leaf)> _discardCardsInHand = [];
    private readonly List<(DiscardCardDecisionEntity parent, DiscardCardDecisionPredictedPoints leaf)> _discardPredictedPoints = [];
    private readonly List<(PlayCardDecisionEntity parent, PlayCardDecisionCardsInHand leaf)> _playCardCardsInHand = [];
    private readonly List<(PlayCardDecisionEntity parent, PlayCardDecisionPlayedCard leaf)> _playCardPlayedCards = [];
    private readonly List<(PlayCardDecisionEntity parent, PlayCardDecisionValidCard leaf)> _playCardValidCards = [];
    private readonly List<(PlayCardDecisionEntity parent, PlayCardDecisionKnownVoid leaf)> _playCardKnownVoids = [];
    private readonly List<(PlayCardDecisionEntity parent, PlayCardDecisionAccountedForCard leaf)> _playCardAccountedForCards = [];
    private readonly List<(PlayCardDecisionEntity parent, PlayCardDecisionPredictedPoints leaf)> _playCardPredictedPoints = [];

    public LeafCollectionCache(IReadOnlyList<GameEntity> entities)
    {
        foreach (var game in entities)
        {
            ExtractAndClear(game, game.GamePlayers, _gamePlayers);

            foreach (var deal in game.Deals)
            {
                ExtractAndClear(deal, deal.DealDeckCards, _dealDeckCards);
                ExtractAndClear(deal, deal.DealKnownPlayerSuitVoids, _dealKnownPlayerSuitVoids);

                foreach (var dealPlayer in deal.DealPlayers)
                {
                    ExtractAndClear(dealPlayer, dealPlayer.StartingHandCards, _dealPlayerStartingHandCards);
                }

                foreach (var trick in deal.Tricks)
                {
                    ExtractAndClear(trick, trick.TrickCardsPlayed, _trickCardsPlayed);

                    foreach (var playCardDecision in trick.PlayCardDecisions)
                    {
                        ExtractPlayCardDecisionLeaves(playCardDecision);
                    }
                }

                foreach (var callTrumpDecision in deal.CallTrumpDecisions)
                {
                    ExtractAndClear(callTrumpDecision, callTrumpDecision.CardsInHand, _callTrumpCardsInHand);
                    ExtractAndClear(callTrumpDecision, callTrumpDecision.ValidDecisions, _callTrumpValidDecisions);
                    ExtractAndClear(callTrumpDecision, callTrumpDecision.PredictedPoints, _callTrumpPredictedPoints);
                }

                foreach (var discardDecision in deal.DiscardCardDecisions)
                {
                    ExtractAndClear(discardDecision, discardDecision.CardsInHand, _discardCardsInHand);
                    ExtractAndClear(discardDecision, discardDecision.PredictedPoints, _discardPredictedPoints);
                }
            }
        }
    }

    public int LeafCount =>
        _gamePlayers.Count +
        _dealDeckCards.Count +
        _dealKnownPlayerSuitVoids.Count +
        _dealPlayerStartingHandCards.Count +
        _trickCardsPlayed.Count +
        _callTrumpCardsInHand.Count +
        _callTrumpValidDecisions.Count +
        _callTrumpPredictedPoints.Count +
        _discardCardsInHand.Count +
        _discardPredictedPoints.Count +
        _playCardCardsInHand.Count +
        _playCardPlayedCards.Count +
        _playCardValidCards.Count +
        _playCardKnownVoids.Count +
        _playCardAccountedForCards.Count +
        _playCardPredictedPoints.Count;

    public IReadOnlyList<GamePlayer> GamePlayers => _gamePlayers.ConvertAll(x => x.leaf);

    public IReadOnlyList<DealDeckCard> DealDeckCards => _dealDeckCards.ConvertAll(x => x.leaf);

    public IReadOnlyList<DealKnownPlayerSuitVoid> DealKnownPlayerSuitVoids => _dealKnownPlayerSuitVoids.ConvertAll(x => x.leaf);

    public IReadOnlyList<DealPlayerStartingHandCard> DealPlayerStartingHandCards => _dealPlayerStartingHandCards.ConvertAll(x => x.leaf);

    public IReadOnlyList<TrickCardPlayed> TrickCardsPlayed => _trickCardsPlayed.ConvertAll(x => x.leaf);

    public IReadOnlyList<CallTrumpDecisionCardsInHand> CallTrumpCardsInHand => _callTrumpCardsInHand.ConvertAll(x => x.leaf);

    public IReadOnlyList<CallTrumpDecisionValidDecision> CallTrumpValidDecisions => _callTrumpValidDecisions.ConvertAll(x => x.leaf);

    public IReadOnlyList<CallTrumpDecisionPredictedPoints> CallTrumpPredictedPoints => _callTrumpPredictedPoints.ConvertAll(x => x.leaf);

    public IReadOnlyList<DiscardCardDecisionCardsInHand> DiscardCardsInHand => _discardCardsInHand.ConvertAll(x => x.leaf);

    public IReadOnlyList<DiscardCardDecisionPredictedPoints> DiscardPredictedPoints => _discardPredictedPoints.ConvertAll(x => x.leaf);

    public IReadOnlyList<PlayCardDecisionCardsInHand> PlayCardCardsInHand => _playCardCardsInHand.ConvertAll(x => x.leaf);

    public IReadOnlyList<PlayCardDecisionPlayedCard> PlayCardPlayedCards => _playCardPlayedCards.ConvertAll(x => x.leaf);

    public IReadOnlyList<PlayCardDecisionValidCard> PlayCardValidCards => _playCardValidCards.ConvertAll(x => x.leaf);

    public IReadOnlyList<PlayCardDecisionKnownVoid> PlayCardKnownVoids => _playCardKnownVoids.ConvertAll(x => x.leaf);

    public IReadOnlyList<PlayCardDecisionAccountedForCard> PlayCardAccountedForCards => _playCardAccountedForCards.ConvertAll(x => x.leaf);

    public IReadOnlyList<PlayCardDecisionPredictedPoints> PlayCardPredictedPoints => _playCardPredictedPoints.ConvertAll(x => x.leaf);

    public void PopulateForeignKeys()
    {
        foreach (var (parent, leaf) in _gamePlayers)
        {
            leaf.GameId = parent.GameId;
        }

        foreach (var (parent, leaf) in _dealDeckCards)
        {
            leaf.DealId = parent.DealId;
        }

        foreach (var (parent, leaf) in _dealKnownPlayerSuitVoids)
        {
            leaf.DealId = parent.DealId;
        }

        foreach (var (parent, leaf) in _dealPlayerStartingHandCards)
        {
            leaf.DealPlayerId = parent.DealPlayerId;
        }

        foreach (var (parent, leaf) in _trickCardsPlayed)
        {
            leaf.TrickId = parent.TrickId;
        }

        foreach (var (parent, leaf) in _callTrumpCardsInHand)
        {
            leaf.CallTrumpDecisionId = parent.CallTrumpDecisionId;
        }

        foreach (var (parent, leaf) in _callTrumpValidDecisions)
        {
            leaf.CallTrumpDecisionId = parent.CallTrumpDecisionId;
        }

        foreach (var (parent, leaf) in _callTrumpPredictedPoints)
        {
            leaf.CallTrumpDecisionId = parent.CallTrumpDecisionId;
        }

        foreach (var (parent, leaf) in _discardCardsInHand)
        {
            leaf.DiscardCardDecisionId = parent.DiscardCardDecisionId;
        }

        foreach (var (parent, leaf) in _discardPredictedPoints)
        {
            leaf.DiscardCardDecisionId = parent.DiscardCardDecisionId;
        }

        foreach (var (parent, leaf) in _playCardCardsInHand)
        {
            leaf.PlayCardDecisionId = parent.PlayCardDecisionId;
        }

        foreach (var (parent, leaf) in _playCardPlayedCards)
        {
            leaf.PlayCardDecisionId = parent.PlayCardDecisionId;
        }

        foreach (var (parent, leaf) in _playCardValidCards)
        {
            leaf.PlayCardDecisionId = parent.PlayCardDecisionId;
        }

        foreach (var (parent, leaf) in _playCardKnownVoids)
        {
            leaf.PlayCardDecisionId = parent.PlayCardDecisionId;
        }

        foreach (var (parent, leaf) in _playCardAccountedForCards)
        {
            leaf.PlayCardDecisionId = parent.PlayCardDecisionId;
        }

        foreach (var (parent, leaf) in _playCardPredictedPoints)
        {
            leaf.PlayCardDecisionId = parent.PlayCardDecisionId;
        }
    }

    private static void ExtractAndClear<TParent, TLeaf>(
        TParent parent,
        ICollection<TLeaf> collection,
        List<(TParent parent, TLeaf leaf)> cache)
    {
        foreach (var leaf in collection)
        {
            cache.Add((parent, leaf));
        }

        collection.Clear();
    }

    private void ExtractPlayCardDecisionLeaves(PlayCardDecisionEntity decision)
    {
        ExtractAndClear(decision, decision.CardsInHand, _playCardCardsInHand);
        ExtractAndClear(decision, decision.PlayedCards, _playCardPlayedCards);
        ExtractAndClear(decision, decision.ValidCards, _playCardValidCards);
        ExtractAndClear(decision, decision.KnownVoids, _playCardKnownVoids);
        ExtractAndClear(decision, decision.CardsAccountedFor, _playCardAccountedForCards);
        ExtractAndClear(decision, decision.PredictedPoints, _playCardPredictedPoints);
    }
}
