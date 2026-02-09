namespace NemesisEuchre.DataAccess.Entities;

public interface IDecisionEntity
{
    int? ActorTypeId { get; set; }

    bool? DidTeamWinGame { get; set; }
}
