using System.Diagnostics.CodeAnalysis;

using FluentAssertions;

using NemesisEuchre.Console.Services;

using Spectre.Console.Testing;

namespace NemesisEuchre.Console.Tests.Services;

public class ApplicationBannerTests
{
    [SuppressMessage(
        "Roslynator",
        "RCS1192:Unnecessary usage of verbatim string literal",
        Justification = "Verbatim string is not necessary for each line but removing it ruins the ascii art and makes the source code less readable")]
    [Fact]
    public void DisplayShouldOutputNemesisEuchreLogo()
    {
        var testConsole = new TestConsole();
        var banner = new ApplicationBanner(testConsole);
        banner.Display();

        testConsole.Output.Should().Contain(@" _   _                                   _       ");
        testConsole.Output.Should().Contain(@"| \ | |   ___   _ __ ___     ___   ___  (_)  ___ ");
        testConsole.Output.Should().Contain(@"|  \| |  / _ \ | '_ ` _ \   / _ \ / __| | | / __|");
        testConsole.Output.Should().Contain(@"| |\  | |  __/ | | | | | | |  __/ \__ \ | | \__ \");
        testConsole.Output.Should().Contain(@"|_| \_|  \___| |_| |_| |_|  \___| |___/ |_| |___/");

        testConsole.Output.Should().Contain(@" _____                  _");
        testConsole.Output.Should().Contain(@"| ____|  _   _    ___  | |__    _ __    ___");
        testConsole.Output.Should().Contain(@"|  _|   | | | |  / __| | '_ \  | '__|  / _ \");
        testConsole.Output.Should().Contain(@"| |___  | |_| | | (__  | | | | | |    |  __/");
        testConsole.Output.Should().Contain(@"|_____|  \__,_|  \___| |_| |_| |_|     \___|");
    }
}
