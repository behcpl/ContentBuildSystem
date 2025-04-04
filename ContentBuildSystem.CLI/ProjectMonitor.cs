using System;
using System.Collections.Generic;
using System.IO;

namespace ContentBuildSystem.CLI;

public class ProjectMonitor : IDisposable
{
    private readonly ProjectBuilder _builder;
    private readonly ContentBuilderOptions _options;
    private readonly string _configuration;
    private object _lock;
    private bool _rebuild;
    private DateTime _rebuildTime;
    private TimeSpan _rebuildDelay;

    private readonly List<FileSystemWatcher> _watchers;

    public ProjectMonitor(ProjectBuilder builder, ContentBuilderOptions options, string configuration)
    {
        _builder = builder;
        _options = options;
        _configuration = configuration;

        _lock = new object();
        _watchers = new List<FileSystemWatcher>();

        _rebuildDelay = TimeSpan.FromMilliseconds(1500);

        WatchPath(_options.ProjectPath);
    }

    private void WatchPath(string path)
    {
        FileSystemWatcher watcher = new FileSystemWatcher(path);
        watcher.NotifyFilter = NotifyFilters.Attributes
                               | NotifyFilters.CreationTime
                               // | NotifyFilters.DirectoryName
                               | NotifyFilters.FileName
                               // | NotifyFilters.LastAccess
                               | NotifyFilters.LastWrite
                               // | NotifyFilters.Security
                               | NotifyFilters.Size;

        watcher.Changed += OnChanged;
        watcher.Created += OnChanged;
        watcher.Deleted += OnChanged;
        watcher.Renamed += OnChanged;
        watcher.Error += OnError;

        // watcher.Filter = "*.txt";
        watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;

        _watchers.Add(watcher);
    }

    public void Update()
    {
        lock (_lock)
        {
            if (!_rebuild || DateTime.UtcNow < _rebuildTime)
                return;

            _rebuild = false;
        }

        Console.WriteLine("REBUILDING....");
        _builder.Build(_options, _configuration);
    }

    private void OnError(object sender, ErrorEventArgs e)
    {
        Console.WriteLine(e.GetException());
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"CHANGE {e.GetType().Name} | {e.FullPath} - {e.ChangeType.ToString()}");
            Console.ResetColor();
            _rebuild = true;
            _rebuildTime = DateTime.UtcNow + _rebuildDelay;
        }
    }

    public void Dispose()
    {
        foreach (FileSystemWatcher watcher in _watchers)
        {
            watcher.Dispose();
        }

        _watchers.Clear();
    }
}