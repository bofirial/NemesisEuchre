using System.Text.Json;

using NemesisEuchre.DataAccess.Entities;
using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.DataAccess.Mappers;

public interface ITrickToEntityMapper
{
    TrickEntity Map(Trick trick, int trickNumber);
}

public class TrickToEntityMapper : ITrickToEntityMapper
{
    public TrickEntity Map(Trick trick, int trickNumber)
    {
        return new TrickEntity
        {
            TrickNumber = trickNumber,
            LeadPosition = trick.LeadPosition,
            CardsPlayedJson = JsonSerializer.Serialize(trick.CardsPlayed),
            LeadSuit = trick.LeadSuit,
            WinningPosition = trick.WinningPosition,
            WinningTeam = trick.WinningTeam,
        };
    }
}
