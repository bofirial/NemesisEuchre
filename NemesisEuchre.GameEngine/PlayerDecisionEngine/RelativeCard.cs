using System.Text.Json.Serialization;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.PlayerDecisionEngine;

public record RelativeCard(Rank Rank, RelativeSuit Suit)
{
    [JsonIgnore]
    public Card? Card { get; init; }
}
