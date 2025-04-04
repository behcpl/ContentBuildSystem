using System;
using System.IO;
using ContentBuildSystem.Interfaces;
using ContentBuildSystem.Json;
using ContentBuildSystem.Project;
using ContentBuildSystem.Rules;

namespace ContentBuildSystem.CLI;

public class ProjectBuilder
{
    private readonly IReport _report;

    private readonly ProjectSerializer _projectSerializer;
    private readonly RuleProvider _ruleProvider;

    private PluginManager? _pluginManager;

    private ProjectDescription? _projectDescription;

    public ProjectBuilder(IReport report)
    {
        _report = report;
        _projectSerializer = new ProjectSerializer();
        _ruleProvider = new RuleProvider(new RuleSerializer());

        // TODO: where to search plugins? project first then some default folder?
    }

    public void LoadProject(string projectPath)
    {
        _projectDescription = _projectSerializer.Deserialize(projectPath, _report);
    }

    public void LoadPlugins(string pluginPath)
    {
        if (_projectDescription?.Plugins == null)
            return;
      
        _pluginManager = new PluginManager(pluginPath);

        foreach (PluginDescription pluginDescription in _projectDescription.Plugins)
        {
            IPlugin plugin = _pluginManager.LoadPlugin(pluginDescription.Path!, pluginDescription.Options);

            _ruleProvider.AddPlugin(plugin, pluginDescription.Namespace);
        }
    }

    public bool Build(ContentBuilderOptions builderOptions, string? configuration)
    {
        ContentBuilder contentBuilder = new ContentBuilder(builderOptions, _projectDescription!, _ruleProvider, new BuildItemManifestSerializer());

        if (!contentBuilder.PrepareConfiguration(configuration, _report))
            return false;
        
        return contentBuilder.BuildGroups(_report);
    }

    public bool Clean(ContentBuilderOptions builderOptions)
    {
        try
        {
            if (Directory.Exists(builderOptions.TempPath))
            {
                _report.Info($"DELETING '{builderOptions.TempPath}'");
                Directory.Delete(builderOptions.TempPath, true);
            }

            if (Directory.Exists(builderOptions.OutputPath))
            {
                _report.Info($"DELETING '{builderOptions.OutputPath}'");
                Directory.Delete(builderOptions.OutputPath, true);
            }
        }
        catch (Exception e)
        {
            _report.Exception(e);
            return false;
        }

        return true;
    }
}