using System;
using ContentBuildSystem.Interfaces;

namespace ContentBuildSystem;

public class ConsoleReport : IReport
{
    private string _group;
    private string _item;
    private bool _headerShown;

    public ConsoleReport()
    {
        _group = string.Empty;
        _item = string.Empty;
    }

    public void BeginGroup(string groupName, int expectedCount)
    {
        _group = groupName;
        _headerShown = false;
    }

    public void GroupItem(string itemName)
    {
        _item = itemName;
        _headerShown = false;
    }

    public void EndGroup()
    {
        _headerShown = false;
    }


    public void Info(string message) { }

    public void Warning(string message)
    {
        PrintHeader();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public void Error(string message)
    {
        PrintHeader();

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public void Exception(Exception e)
    {
        PrintHeader();
        
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(e.Message);
        Console.ResetColor();
    }

    private void PrintHeader()
    {
        if(_headerShown)
            return;

        _headerShown = true;
        Console.WriteLine($"[{_group}] {_item}");
    }
}