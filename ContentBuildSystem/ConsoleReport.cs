using System;
using System.Collections.Generic;
using ContentBuildSystem.Interfaces;

namespace ContentBuildSystem;

public class ConsoleReport : IReport
{
    private string _group;
    private string _item;
    private bool _headerShown;

    private const int _BAR_SIZE = 20;
    private readonly char[] _barBuffer;

    private class GroupBar
    {
        // public string Name;
        public int Row;
        public int Count;
        public int Total;
    }

    private readonly List<GroupBar> _activeGroups;

    public ConsoleReport()
    {
        _group = string.Empty;
        _item = string.Empty;

        _barBuffer = new char[_BAR_SIZE + 2];

        _activeGroups = new List<GroupBar>();
    }

    public void BeginGroup(string groupName, int expectedCount)
    {
        _group = groupName;
        _headerShown = false;

        GroupBar gb = new();
        gb.Row = Console.CursorTop;
        gb.Total = expectedCount;

        if (groupName.Length < 30)
            Console.Write(groupName);

        for (int i = groupName.Length; i < 30; i++)
        {
            Console.Write(" ");
        }

        DrawBar(0, expectedCount);
        Console.WriteLine();

        _activeGroups.Add(gb);
    }

    private void DrawBar(int count, int total)
    {
        _barBuffer[0] = '[';
        _barBuffer[^1] = ']';

        int fill = total > 0 ? (count * _BAR_SIZE + total / 2) / total : _BAR_SIZE;
        for (int i = 0; i < _BAR_SIZE; i++)
        {
            _barBuffer[i + 1] = i < fill ? '\u25a0' : ' ';
        }

        Console.Write(_barBuffer);
    }


    public void GroupItem(string itemName)
    {
        (int left, int top) = Console.GetCursorPosition();
        GroupBar group = _activeGroups[^1];
        Console.SetCursorPosition(30, group.Row);
        group.Count++;
        DrawBar(group.Count, group.Total);

        Console.SetCursorPosition(left, top);

        _item = itemName;
        _headerShown = false;
    }

    public void EndGroup()
    {
        _headerShown = false;
        _activeGroups.RemoveAt(_activeGroups.Count - 1);
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
        if (_headerShown)
            return;

        _headerShown = true;
        Console.WriteLine($"[{_group}] {_item}");
    }
}
