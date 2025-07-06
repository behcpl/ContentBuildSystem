using System;

namespace ContentBuildSystem.Interfaces;

public interface IReport
{
    IReport CreateGroup(string groupName, int expectedCount);
    void Update(string groupName, int expectedCount);
    void Advance();
    void Finish();

    void Info(string message);
    void Warning(string message);
    void Error(string message);
    void Exception(Exception e);
}
