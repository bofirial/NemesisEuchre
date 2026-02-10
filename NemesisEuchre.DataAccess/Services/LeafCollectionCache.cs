using NemesisEuchre.DataAccess.Entities;

namespace NemesisEuchre.DataAccess.Services;

public class LeafCollectionCache
{
    private readonly List<GameEntity> _gamePlayerParents = [];
    private readonly List<GamePlayer> _gamePlayerLeaves = [];
    private readonly List<DealEntity> _dealDeckCardParents = [];
    private readonly List<DealDeckCard> _dealDeckCardLeaves = [];
    private readonly List<DealEntity> _dealKnownPlayerSuitVoidParents = [];
    private readonly List<DealKnownPlayerSuitVoid> _dealKnownPlayerSuitVoidLeaves = [];
    private readonly List<DealPlayerEntity> _dealPlayerStartingHandCardParents = [];
    private readonly List<DealPlayerStartingHandCard> _dealPlayerStartingHandCardLeaves = [];
    private readonly List<TrickEntity> _trickCardsPlayedParents = [];
    private readonly List<TrickCardPlayed> _trickCardsPlayedLeaves = [];
    private readonly List<CallTrumpDecisionEntity> _callTrumpCardsInHandParents = [];
    private readonly List<CallTrumpDecisionCardsInHand> _callTrumpCardsInHandLeaves = [];
    private readonly List<CallTrumpDecisionEntity> _callTrumpValidDecisionParents = [];
    private readonly List<CallTrumpDecisionValidDecision> _callTrumpValidDecisionLeaves = [];
    private readonly List<CallTrumpDecisionEntity> _callTrumpPredictedPointsParents = [];
    private readonly List<CallTrumpDecisionPredictedPoints> _callTrumpPredictedPointsLeaves = [];
    private readonly List<DiscardCardDecisionEntity> _discardCardsInHandParents = [];
    private readonly List<DiscardCardDecisionCardsInHand> _discardCardsInHandLeaves = [];
    private readonly List<DiscardCardDecisionEntity> _discardPredictedPointsParents = [];
    private readonly List<DiscardCardDecisionPredictedPoints> _discardPredictedPointsLeaves = [];
    private readonly List<PlayCardDecisionEntity> _playCardCardsInHandParents = [];
    private readonly List<PlayCardDecisionCardsInHand> _playCardCardsInHandLeaves = [];
    private readonly List<PlayCardDecisionEntity> _playCardPlayedCardParents = [];
    private readonly List<PlayCardDecisionPlayedCard> _playCardPlayedCardLeaves = [];
    private readonly List<PlayCardDecisionEntity> _playCardValidCardParents = [];
    private readonly List<PlayCardDecisionValidCard> _playCardValidCardLeaves = [];
    private readonly List<PlayCardDecisionEntity> _playCardKnownVoidParents = [];
    private readonly List<PlayCardDecisionKnownVoid> _playCardKnownVoidLeaves = [];
    private readonly List<PlayCardDecisionEntity> _playCardAccountedForCardParents = [];
    private readonly List<PlayCardDecisionAccountedForCard> _playCardAccountedForCardLeaves = [];
    private readonly List<PlayCardDecisionEntity> _playCardPredictedPointsParents = [];
    private readonly List<PlayCardDecisionPredictedPoints> _playCardPredictedPointsLeaves = [];

    public LeafCollectionCache(IReadOnlyList<GameEntity> entities)
    {
        foreach (var game in entities)
        {
            ExtractAndClear(game, game.GamePlayers, _gamePlayerParents, _gamePlayerLeaves);

            foreach (var deal in game.Deals)
            {
                ExtractAndClear(deal, deal.DealDeckCards, _dealDeckCardParents, _dealDeckCardLeaves);
                ExtractAndClear(deal, deal.DealKnownPlayerSuitVoids, _dealKnownPlayerSuitVoidParents, _dealKnownPlayerSuitVoidLeaves);

                foreach (var dealPlayer in deal.DealPlayers)
                {
                    ExtractAndClear(dealPlayer, dealPlayer.StartingHandCards, _dealPlayerStartingHandCardParents, _dealPlayerStartingHandCardLeaves);
                }

                foreach (var trick in deal.Tricks)
                {
                    ExtractAndClear(trick, trick.TrickCardsPlayed, _trickCardsPlayedParents, _trickCardsPlayedLeaves);

                    foreach (var playCardDecision in trick.PlayCardDecisions)
                    {
                        ExtractPlayCardDecisionLeaves(playCardDecision);
                    }
                }

                foreach (var callTrumpDecision in deal.CallTrumpDecisions)
                {
                    ExtractAndClear(callTrumpDecision, callTrumpDecision.CardsInHand, _callTrumpCardsInHandParents, _callTrumpCardsInHandLeaves);
                    ExtractAndClear(callTrumpDecision, callTrumpDecision.ValidDecisions, _callTrumpValidDecisionParents, _callTrumpValidDecisionLeaves);
                    ExtractAndClear(callTrumpDecision, callTrumpDecision.PredictedPoints, _callTrumpPredictedPointsParents, _callTrumpPredictedPointsLeaves);
                }

                foreach (var discardDecision in deal.DiscardCardDecisions)
                {
                    ExtractAndClear(discardDecision, discardDecision.CardsInHand, _discardCardsInHandParents, _discardCardsInHandLeaves);
                    ExtractAndClear(discardDecision, discardDecision.PredictedPoints, _discardPredictedPointsParents, _discardPredictedPointsLeaves);
                }
            }
        }
    }

    public int LeafCount =>
        _gamePlayerLeaves.Count +
        _dealDeckCardLeaves.Count +
        _dealKnownPlayerSuitVoidLeaves.Count +
        _dealPlayerStartingHandCardLeaves.Count +
        _trickCardsPlayedLeaves.Count +
        _callTrumpCardsInHandLeaves.Count +
        _callTrumpValidDecisionLeaves.Count +
        _callTrumpPredictedPointsLeaves.Count +
        _discardCardsInHandLeaves.Count +
        _discardPredictedPointsLeaves.Count +
        _playCardCardsInHandLeaves.Count +
        _playCardPlayedCardLeaves.Count +
        _playCardValidCardLeaves.Count +
        _playCardKnownVoidLeaves.Count +
        _playCardAccountedForCardLeaves.Count +
        _playCardPredictedPointsLeaves.Count;

    public IReadOnlyList<GamePlayer> GamePlayers => _gamePlayerLeaves;

    public IReadOnlyList<DealDeckCard> DealDeckCards => _dealDeckCardLeaves;

    public IReadOnlyList<DealKnownPlayerSuitVoid> DealKnownPlayerSuitVoids => _dealKnownPlayerSuitVoidLeaves;

    public IReadOnlyList<DealPlayerStartingHandCard> DealPlayerStartingHandCards => _dealPlayerStartingHandCardLeaves;

    public IReadOnlyList<TrickCardPlayed> TrickCardsPlayed => _trickCardsPlayedLeaves;

    public IReadOnlyList<CallTrumpDecisionCardsInHand> CallTrumpCardsInHand => _callTrumpCardsInHandLeaves;

    public IReadOnlyList<CallTrumpDecisionValidDecision> CallTrumpValidDecisions => _callTrumpValidDecisionLeaves;

    public IReadOnlyList<CallTrumpDecisionPredictedPoints> CallTrumpPredictedPoints => _callTrumpPredictedPointsLeaves;

    public IReadOnlyList<DiscardCardDecisionCardsInHand> DiscardCardsInHand => _discardCardsInHandLeaves;

    public IReadOnlyList<DiscardCardDecisionPredictedPoints> DiscardPredictedPoints => _discardPredictedPointsLeaves;

    public IReadOnlyList<PlayCardDecisionCardsInHand> PlayCardCardsInHand => _playCardCardsInHandLeaves;

    public IReadOnlyList<PlayCardDecisionPlayedCard> PlayCardPlayedCards => _playCardPlayedCardLeaves;

    public IReadOnlyList<PlayCardDecisionValidCard> PlayCardValidCards => _playCardValidCardLeaves;

    public IReadOnlyList<PlayCardDecisionKnownVoid> PlayCardKnownVoids => _playCardKnownVoidLeaves;

    public IReadOnlyList<PlayCardDecisionAccountedForCard> PlayCardAccountedForCards => _playCardAccountedForCardLeaves;

    public IReadOnlyList<PlayCardDecisionPredictedPoints> PlayCardPredictedPoints => _playCardPredictedPointsLeaves;

    public void PopulateForeignKeys()
    {
        for (int i = 0; i < _gamePlayerParents.Count; i++)
        {
            _gamePlayerLeaves[i].GameId = _gamePlayerParents[i].GameId;
        }

        for (int i = 0; i < _dealDeckCardParents.Count; i++)
        {
            _dealDeckCardLeaves[i].DealId = _dealDeckCardParents[i].DealId;
        }

        for (int i = 0; i < _dealKnownPlayerSuitVoidParents.Count; i++)
        {
            _dealKnownPlayerSuitVoidLeaves[i].DealId = _dealKnownPlayerSuitVoidParents[i].DealId;
        }

        for (int i = 0; i < _dealPlayerStartingHandCardParents.Count; i++)
        {
            _dealPlayerStartingHandCardLeaves[i].DealPlayerId = _dealPlayerStartingHandCardParents[i].DealPlayerId;
        }

        for (int i = 0; i < _trickCardsPlayedParents.Count; i++)
        {
            _trickCardsPlayedLeaves[i].TrickId = _trickCardsPlayedParents[i].TrickId;
        }

        for (int i = 0; i < _callTrumpCardsInHandParents.Count; i++)
        {
            _callTrumpCardsInHandLeaves[i].CallTrumpDecisionId = _callTrumpCardsInHandParents[i].CallTrumpDecisionId;
        }

        for (int i = 0; i < _callTrumpValidDecisionParents.Count; i++)
        {
            _callTrumpValidDecisionLeaves[i].CallTrumpDecisionId = _callTrumpValidDecisionParents[i].CallTrumpDecisionId;
        }

        for (int i = 0; i < _callTrumpPredictedPointsParents.Count; i++)
        {
            _callTrumpPredictedPointsLeaves[i].CallTrumpDecisionId = _callTrumpPredictedPointsParents[i].CallTrumpDecisionId;
        }

        for (int i = 0; i < _discardCardsInHandParents.Count; i++)
        {
            _discardCardsInHandLeaves[i].DiscardCardDecisionId = _discardCardsInHandParents[i].DiscardCardDecisionId;
        }

        for (int i = 0; i < _discardPredictedPointsParents.Count; i++)
        {
            _discardPredictedPointsLeaves[i].DiscardCardDecisionId = _discardPredictedPointsParents[i].DiscardCardDecisionId;
        }

        for (int i = 0; i < _playCardCardsInHandParents.Count; i++)
        {
            _playCardCardsInHandLeaves[i].PlayCardDecisionId = _playCardCardsInHandParents[i].PlayCardDecisionId;
        }

        for (int i = 0; i < _playCardPlayedCardParents.Count; i++)
        {
            _playCardPlayedCardLeaves[i].PlayCardDecisionId = _playCardPlayedCardParents[i].PlayCardDecisionId;
        }

        for (int i = 0; i < _playCardValidCardParents.Count; i++)
        {
            _playCardValidCardLeaves[i].PlayCardDecisionId = _playCardValidCardParents[i].PlayCardDecisionId;
        }

        for (int i = 0; i < _playCardKnownVoidParents.Count; i++)
        {
            _playCardKnownVoidLeaves[i].PlayCardDecisionId = _playCardKnownVoidParents[i].PlayCardDecisionId;
        }

        for (int i = 0; i < _playCardAccountedForCardParents.Count; i++)
        {
            _playCardAccountedForCardLeaves[i].PlayCardDecisionId = _playCardAccountedForCardParents[i].PlayCardDecisionId;
        }

        for (int i = 0; i < _playCardPredictedPointsParents.Count; i++)
        {
            _playCardPredictedPointsLeaves[i].PlayCardDecisionId = _playCardPredictedPointsParents[i].PlayCardDecisionId;
        }
    }

    private static void ExtractAndClear<TParent, TLeaf>(
        TParent parent,
        ICollection<TLeaf> collection,
        List<TParent> parentCache,
        List<TLeaf> leafCache)
    {
        foreach (var leaf in collection)
        {
            parentCache.Add(parent);
            leafCache.Add(leaf);
        }

        collection.Clear();
    }

    private void ExtractPlayCardDecisionLeaves(PlayCardDecisionEntity decision)
    {
        ExtractAndClear(decision, decision.CardsInHand, _playCardCardsInHandParents, _playCardCardsInHandLeaves);
        ExtractAndClear(decision, decision.PlayedCards, _playCardPlayedCardParents, _playCardPlayedCardLeaves);
        ExtractAndClear(decision, decision.ValidCards, _playCardValidCardParents, _playCardValidCardLeaves);
        ExtractAndClear(decision, decision.KnownVoids, _playCardKnownVoidParents, _playCardKnownVoidLeaves);
        ExtractAndClear(decision, decision.CardsAccountedFor, _playCardAccountedForCardParents, _playCardAccountedForCardLeaves);
        ExtractAndClear(decision, decision.PredictedPoints, _playCardPredictedPointsParents, _playCardPredictedPointsLeaves);
    }
}
