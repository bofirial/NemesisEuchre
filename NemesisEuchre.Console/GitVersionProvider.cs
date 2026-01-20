namespace NemesisEuchre.Console;

public sealed class GitVersionProvider : IVersionProvider
{
    public string AssemblyInformationalVersion => ThisAssembly.AssemblyInformationalVersion;

    public string AssemblyFileVersion => ThisAssembly.AssemblyFileVersion;

    public string GitCommitId => ThisAssembly.GitCommitId;

    public DateTime GitCommitDate => ThisAssembly.GitCommitDate;

    public string AssemblyConfiguration => ThisAssembly.AssemblyConfiguration;

    public bool IsPrerelease => ThisAssembly.IsPrerelease;
}
