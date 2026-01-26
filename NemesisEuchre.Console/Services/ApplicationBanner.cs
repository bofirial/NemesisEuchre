using Spectre.Console;

namespace NemesisEuchre.Console.Services;

public interface IApplicationBanner
{
    void Display();
}

public class ApplicationBanner(IAnsiConsole ansiConsole) : IApplicationBanner
{
    public void Display()
    {
        ansiConsole.Write(
            new FigletText("Nemesis Euchre")
                .Centered()
                .Color(Color.Blue));
        ansiConsole.WriteLine();
    }
}
