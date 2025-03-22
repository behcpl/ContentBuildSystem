﻿using CommandLine;

namespace ContentBuildSystem.CLI;

[Verb("build", HelpText = "Build asset project.")]
public class BuildOptions
{
    [Option('n', "clean", Default = false, HelpText = "Removes any existing output data forcing to do full rebuild.")]
    public bool Clean { get; set; }

    [Option('v', "verbose", Default = false, HelpText = "Prints all messages to standard output.")]
    public bool Verbose { get; set; }
    
    [Option('c', "configuration", Default = null, HelpText = "Configuration name to specify rules' parameters.")]
    public string? Configuration { get; set; }

    [Value(0, MetaName = "input file", HelpText = "Input project file to be processed.", Required = true)]
    public string? FileName { get; set; }
}

[Verb("clean", HelpText = "Removes any existing output data.")]
public class CleanOptions
{
    [Option('v', "verbose", Default = false, HelpText = "Prints all messages to standard output.")]
    public bool Verbose { get; set; }

    [Value(0, MetaName = "input file", HelpText = "Input project file to be processed.", Required = true)]
    public string? FileName { get; set; }
}

[Verb("monitor", HelpText = "Starts monitoring source folders and triggers auto build until closed")]
public class MonitorOptions
{
    [Option('v', "verbose", Default = false, HelpText = "Prints all messages to standard output.")]
    public bool Verbose { get; set; }
   
    [Option('c', "configuration", Default = null, HelpText = "Configuration name to specify rules' parameters.")]
    public string? Configuration { get; set; }

    [Value(0, MetaName = "input file", HelpText = "Input project file to be processed.", Required = true)]
    public string? FileName { get; set; }
}