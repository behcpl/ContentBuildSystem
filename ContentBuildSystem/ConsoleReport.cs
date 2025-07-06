using System;
using System.Collections.Generic;
using ContentBuildSystem.Interfaces;

namespace ContentBuildSystem;

public class ConsoleReport : IReport
{
    private readonly object _lock;
    private readonly List<GroupDesc> _activeGroups;
    private readonly char[] _empty;

    private const int _BAR_POS = 30;
    private const int _BAR_SIZE = 40;
    private readonly char[] _barBuffer;

    private int _interactiveRowStart;
    private int _interactiveRowCount;

    private class GroupDesc : IReport
    {
        private readonly ConsoleReport _owner;

        public int Errors;
        public int Warnings;

        public int Total;
        public int Processed;

        public string Name;

        public GroupDesc(ConsoleReport owner, string name, int total)
        {
            _owner = owner;
            Name = name;
            Total = total;
        }

        public IReport CreateGroup(string groupName, int expectedCount)
        {
            return _owner.CreateGroup(groupName, expectedCount);
        }

        public void Update(string groupName, int expectedCount)
        {
            Name = groupName;
            Total = expectedCount;

            // TODO: redraw?
        }

        public void Advance()
        {
            _owner.AdvanceGroup(this);
        }

        public void Finish()
        {
            _owner.FinishGroup(this);
        }

        public void Info(string message)
        {
            _owner.Info(message);
        }

        public void Warning(string message)
        {
            Warnings++;
            _owner.Warning(message);
        }

        public void Error(string message)
        {
            Errors++;
            _owner.Error(message);
        }

        public void Exception(Exception e)
        {
            Errors++;
            _owner.Exception(e);
        }
    }

    public ConsoleReport()
    {
        _lock = new object();
        _activeGroups = new List<GroupDesc>();
        _empty = new char[1024];
        _barBuffer = new char[_BAR_SIZE + 2];

        _interactiveRowStart = Console.CursorTop;
        _interactiveRowCount = 0;
        
        Array.Fill(_empty, ' ');
    }

    public IReport CreateGroup(string groupName, int expectedCount)
    {
        lock (_lock)
        {
            GroupDesc group = new(this, groupName, expectedCount);
            _activeGroups.Add(group);
            return group;
        }
    }

    public void Update(string groupName, int expectedCount) { }
    public void Advance() { }
    public void Finish() { }

    private void AdvanceGroup(GroupDesc group)
    {
        lock (_lock)
        {
            group.Processed = Math.Min(group.Processed + 1, group.Total);

            ClearInteractiveSection();
            PrintInteractiveSection();
        }
    }

    private void FinishGroup(GroupDesc group)
    {
        lock (_lock)
        {
            _activeGroups.Remove(group);

            ClearInteractiveSection();
            PrintGroupDone(group);
            PrintInteractiveSection();
        }
    }

    public void Info(string message)
    {
        // NOOP
    }

    public void Warning(string message)
    {
        lock (_lock)
        {
            ClearInteractiveSection();
            PrintText(message, ConsoleColor.Yellow);
            PrintInteractiveSection();
        }
    }

    public void Error(string message)
    {
        lock (_lock)
        {
            ClearInteractiveSection();
            PrintText(message, ConsoleColor.Red);
            PrintInteractiveSection();
        }
    }

    public void Exception(Exception e)
    {
        lock (_lock)
        {
            ClearInteractiveSection();
            PrintText(e.Message, ConsoleColor.Red);
            PrintInteractiveSection();
        }
    }

    private void PrintText(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    private void ClearInteractiveSection()
    {
        int width = Console.BufferWidth;

        for (int i = 0; i < _interactiveRowCount; i++)
        {
            Console.SetCursorPosition(0, _interactiveRowStart + i);
            Console.Write(_empty, 0, width);
        }

        Console.SetCursorPosition(0, _interactiveRowStart);
    }

    private void PrintInteractiveSection()
    {
        _interactiveRowStart = Console.CursorTop;
        foreach (GroupDesc group in _activeGroups)
        {
            PrintGroup(group);
            Console.WriteLine();
        }

        _interactiveRowCount = Console.CursorTop - _interactiveRowStart;
        Console.SetCursorPosition(0, _interactiveRowStart);
    }

    private void PrintGroup(GroupDesc group)
    {
        Console.Write(group.Name);
        Console.CursorLeft = _BAR_POS;
        DrawBar(group.Processed, group.Total);
    }

    private void PrintGroupDone(GroupDesc group)
    {
        Console.Write(group.Name);
        Console.CursorLeft = _BAR_POS;
        if (group.Errors > 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"{group.Errors} Error(s), {group.Warnings} Warning(s)");
        }
        else if (group.Warnings > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"DONE! {group.Warnings} Warning(s)");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("DONE!");
        }

        Console.ResetColor();
        Console.WriteLine();
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
}
