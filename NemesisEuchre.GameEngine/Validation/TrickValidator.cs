using NemesisEuchre.GameEngine.Models;

namespace NemesisEuchre.GameEngine.Validation;

public interface ITrickValidator
{
    void ValidateTrick(Trick trick);
}

public class TrickValidator : ITrickValidator
{
    public void ValidateTrick(Trick trick)
    {
        ArgumentNullException.ThrowIfNull(trick);
    }
}
