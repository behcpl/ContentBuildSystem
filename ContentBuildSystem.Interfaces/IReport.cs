using System;

namespace ContentBuildSystem.Interfaces;

public interface IReport
{
    void BeginGroup(string groupName, int expectedCount);
    void GroupItem(string itemName);
    void EndGroup();

    void Info(string message);
    void Warning(string message);
    void Error(string message);
    void Exception(Exception e);
}