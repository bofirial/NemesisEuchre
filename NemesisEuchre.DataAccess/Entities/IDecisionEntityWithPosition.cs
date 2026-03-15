using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.DataAccess.Entities;

public interface IDecisionEntityWithPosition
{
    PlayerPosition PlayerPosition { get; }

    short? RelativeDealPoints { get; }
}
