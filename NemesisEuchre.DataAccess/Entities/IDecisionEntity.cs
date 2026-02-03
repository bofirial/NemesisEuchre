using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.DataAccess.Entities;

public interface IDecisionEntity
{
    ActorType? ActorType { get; set; }

    bool? DidTeamWinGame { get; set; }
}
