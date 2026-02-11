namespace NemesisEuchre.Console.Models;

public record GamePersistenceOptions(bool PersistToSql, string? IdvGenerationName, bool AllowOverwrite = false);
