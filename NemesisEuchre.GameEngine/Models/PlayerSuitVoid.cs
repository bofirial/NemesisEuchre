using NemesisEuchre.Foundation.Constants;

namespace NemesisEuchre.GameEngine.Models;

[System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Records")]
public record PlayerSuitVoid(PlayerPosition PlayerPosition, Suit Suit);
