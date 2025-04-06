using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CommandLine;
using ContentBuildSystem.Interfaces;

namespace ContentBuildSystem.CLI;

internal static class Program
{
    private static int Main(string[] args)
    {
        ParserResult<object>? result = Parser.Default.ParseArguments<BuildOptions, CleanOptions, MonitorOptions>(args);
        return result.MapResult<BuildOptions, CleanOptions, MonitorOptions, int>(Build, Clean, Monitor, HandleErrors);
    }

    private static int HandleErrors(IEnumerable<Error> errors)
    {
        return -1;
    }

    private static int Build(BuildOptions options)
    {
        string projectPath = Path.GetFullPath(options.FileName!);
        ContentBuilderOptions builderOptions = ContentBuilderOptions.Build(projectPath, options.OutputPath, options.TempPath);

        IReport report = options.Verbose || Console.IsOutputRedirected ? new VerboseConsoleReport() : new ConsoleReport();
        report.Info("BUILD");
        ProjectBuilder builder = new(report);

        builder.LoadProject(projectPath);
        builder.LoadPlugins(builderOptions.ProjectPath);

        bool result = true;
        if (options.Clean)
        {
            result = builder.Clean(builderOptions);
        }

        if (result)
        {
            result = builder.Build(builderOptions, options.Configuration);
        }

        return HandleResult(result);
    }

    private static int Clean(CleanOptions options)
    {
        IReport report = options.Verbose || Console.IsOutputRedirected ? new VerboseConsoleReport() : new ConsoleReport();
        report.Info("CLEAN");
        ProjectBuilder builder = new(report);

        string projectPath = Path.GetFullPath(options.FileName!);
        ContentBuilderOptions builderOptions = ContentBuilderOptions.Build(projectPath, options.OutputPath, options.TempPath);

        bool result = builder.Clean(builderOptions);

        return HandleResult(result);
    }

    private static int Monitor(MonitorOptions options)
    {
        bool running = true;
        Console.CancelKeyPress += (sender, args) =>
        {
            args.Cancel = true;
            running = false;
        };

        // using Stream input = Console.OpenStandardInput();
        // byte[] buff = new byte[16];

        IReport report = options.Verbose || Console.IsOutputRedirected ? new VerboseConsoleReport() : new ConsoleReport();
        ProjectBuilder builder = new(report);

        string projectPath = Path.GetFullPath(options.FileName!);
        ContentBuilderOptions builderOptions = ContentBuilderOptions.Build(projectPath, options.OutputPath, options.TempPath);

        builder.LoadProject(projectPath);
        builder.LoadPlugins(builderOptions.ProjectPath);

        using ProjectMonitor monitor = new ProjectMonitor(builder, builderOptions, options.Configuration ?? "default");

        while (running)
        {
            Thread.Sleep(100);

            monitor.Update();

            // if (input.CanRead)
            // {
            //     int read = input.Read(buff, 0, 10);
            //     Console.WriteLine($"READ {read} bytes");
            // }
        }

        return 0;
    }

    private static int HandleResult(bool result)
    {
        if (result)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("SUCCESS");
            Console.ResetColor();
            return 0;
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("FAILURE");
        Console.ResetColor();
        return -1;
    }
}