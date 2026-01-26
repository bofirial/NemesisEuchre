using FluentAssertions;

using NemesisEuchre.GameEngine.Constants;
using NemesisEuchre.GameEngine.Mappers;
using NemesisEuchre.GameEngine.PlayerDecisionEngine;

namespace NemesisEuchre.GameEngine.Tests.Mappers;

public class CallTrumpDecisionMapperTests
{
    private readonly CallTrumpDecisionMapper _mapper = new();

    [Fact]
    public void GetValidRound1Decisions_ReturnsPassOrderItUpAndOrderItUpAndGoAlone()
    {
        var decisions = _mapper.GetValidRound1Decisions();

        decisions.Should().HaveCount(3);
        decisions.Should().Contain(CallTrumpDecision.Pass);
        decisions.Should().Contain(CallTrumpDecision.OrderItUp);
        decisions.Should().Contain(CallTrumpDecision.OrderItUpAndGoAlone);
    }

    [Theory]
    [InlineData(Suit.Hearts, false, false, 7)]
    [InlineData(Suit.Hearts, true, false, 7)]
    [InlineData(Suit.Hearts, true, true, 6)]
    [InlineData(Suit.Clubs, false, false, 7)]
    public void GetValidRound2Decisions_ReturnsCorrectDecisions(Suit upcardSuit, bool isDealer, bool stickTheDealer, int expectedCount)
    {
        var decisions = _mapper.GetValidRound2Decisions(upcardSuit, isDealer, stickTheDealer);

        decisions.Should().HaveCount(expectedCount);
        decisions.Should().NotContain(d => d == CallTrumpDecision.OrderItUp || d == CallTrumpDecision.OrderItUpAndGoAlone);

        if (!isDealer || !stickTheDealer)
        {
            decisions.Should().Contain(CallTrumpDecision.Pass);
        }
        else
        {
            decisions.Should().NotContain(CallTrumpDecision.Pass);
        }
    }

    [Fact]
    public void GetValidRound2Decisions_ExcludesUpcardSuit()
    {
        var decisions = _mapper.GetValidRound2Decisions(Suit.Hearts, false, false);

        decisions.Should().NotContain(CallTrumpDecision.CallHearts);
        decisions.Should().NotContain(CallTrumpDecision.CallHeartsAndGoAlone);
        decisions.Should().Contain(CallTrumpDecision.CallClubs);
        decisions.Should().Contain(CallTrumpDecision.CallDiamonds);
        decisions.Should().Contain(CallTrumpDecision.CallSpades);
    }

    [Theory]
    [InlineData(CallTrumpDecision.CallClubs, Suit.Clubs)]
    [InlineData(CallTrumpDecision.CallClubsAndGoAlone, Suit.Clubs)]
    [InlineData(CallTrumpDecision.CallDiamonds, Suit.Diamonds)]
    [InlineData(CallTrumpDecision.CallDiamondsAndGoAlone, Suit.Diamonds)]
    [InlineData(CallTrumpDecision.CallHearts, Suit.Hearts)]
    [InlineData(CallTrumpDecision.CallHeartsAndGoAlone, Suit.Hearts)]
    [InlineData(CallTrumpDecision.CallSpades, Suit.Spades)]
    [InlineData(CallTrumpDecision.CallSpadesAndGoAlone, Suit.Spades)]
    public void ConvertDecisionToSuit_ReturnsCorrectSuit(CallTrumpDecision decision, Suit expectedSuit)
    {
        var suit = _mapper.ConvertDecisionToSuit(decision);

        suit.Should().Be(expectedSuit);
    }

    [Theory]
    [InlineData(CallTrumpDecision.Pass)]
    [InlineData(CallTrumpDecision.OrderItUp)]
    public void ConvertDecisionToSuit_WithNonSuitDecision_ThrowsArgumentOutOfRangeException(CallTrumpDecision decision)
    {
        var act = () => _mapper.ConvertDecisionToSuit(decision);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(CallTrumpDecision.OrderItUpAndGoAlone, true)]
    [InlineData(CallTrumpDecision.CallClubsAndGoAlone, true)]
    [InlineData(CallTrumpDecision.CallDiamondsAndGoAlone, true)]
    [InlineData(CallTrumpDecision.CallHeartsAndGoAlone, true)]
    [InlineData(CallTrumpDecision.CallSpadesAndGoAlone, true)]
    [InlineData(CallTrumpDecision.Pass, false)]
    [InlineData(CallTrumpDecision.OrderItUp, false)]
    [InlineData(CallTrumpDecision.CallClubs, false)]
    [InlineData(CallTrumpDecision.CallDiamonds, false)]
    [InlineData(CallTrumpDecision.CallHearts, false)]
    [InlineData(CallTrumpDecision.CallSpades, false)]
    public void IsGoingAloneDecision_ReturnsCorrectValue(CallTrumpDecision decision, bool expected)
    {
        var result = _mapper.IsGoingAloneDecision(decision);

        result.Should().Be(expected);
    }
}
