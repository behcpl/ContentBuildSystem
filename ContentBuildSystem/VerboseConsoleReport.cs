using System;
using ContentBuildSystem.Interfaces;

namespace ContentBuildSystem;

public class VerboseConsoleReport : IReport
{
    public IReport CreateGroup(string groupName, int expectedCount)
    {
        return this;
    }

    public void Update(string groupName, int expectedCount) { }
    public void Advance() { }
    public void Finish() { }

    public void Info(string message)
    {
        Console.WriteLine(message);
    }

    public void Warning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public void Error(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public void Exception(Exception e)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(e.Message);
        Console.ResetColor();
    }
}
