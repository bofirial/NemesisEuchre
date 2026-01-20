namespace NemesisEuchre.Console;

public interface IVersionProvider
{
    string AssemblyInformationalVersion { get; }

    string AssemblyFileVersion { get; }

    string GitCommitId { get; }

    DateTime GitCommitDate { get; }

    string AssemblyConfiguration { get; }

    bool IsPrerelease { get; }
}
