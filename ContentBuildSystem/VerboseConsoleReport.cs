using System;
using ContentBuildSystem.Interfaces;

namespace ContentBuildSystem;

public class VerboseConsoleReport : IReport
{
    public void BeginGroup(string groupName, int expectedCount)
    {
        Console.WriteLine($"GROUP: {groupName}[{expectedCount}]");
    }

    public void GroupItem(string itemName)
    {
        Console.WriteLine($"ITEM: {itemName}");
    }

    public void EndGroup()
    {
        Console.WriteLine("END-GROUP");
    }

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