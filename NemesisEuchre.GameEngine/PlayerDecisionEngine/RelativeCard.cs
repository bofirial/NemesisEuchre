using System.Text.Json.Serialization;

using NemesisEuchre.Foundation.Constants;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.PlayerDecisionEngine;

public class RelativeCard
{
    public Rank Rank { get; set; }

    public RelativeSuit Suit { get; set; }

    [JsonIgnore]
    public Card Card { get; set; } = null!;
}
