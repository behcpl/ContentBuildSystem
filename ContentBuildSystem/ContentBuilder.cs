﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using ContentBuildSystem.Interfaces;
using ContentBuildSystem.Project;
using ContentBuildSystem.Rules;

namespace ContentBuildSystem;

public class ContentBuilderOptions
{
    public string ProjectPath = string.Empty;
    public string TempPath = string.Empty;
    public string OutputPath = string.Empty;

    public bool IgnoreEmptyFileNames; // i.e. .gitignore, etc
    public bool IgnoreEmptyFolderNames; // i.e. .git, etc
    public bool IgnoreHiddenAttribute;

    public static ContentBuilderOptions Default(string projectFilePath)
    {
        string dir = Path.GetDirectoryName(projectFilePath)!;

        return new ContentBuilderOptions
        {
            ProjectPath = Path.GetFullPath(dir),
            TempPath = Path.GetFullPath("../Temp", dir),
            OutputPath = Path.GetFullPath("../Output", dir),

            IgnoreEmptyFileNames = true,
            IgnoreEmptyFolderNames = true,
            IgnoreHiddenAttribute = true
        };
    }
}

public class ContentBuilder
{
    private readonly ProjectDescription _project;
    private readonly RuleProvider _ruleProvider;
    private readonly BuildItemManifestSerializer _itemManifestSerializer;

    private readonly List<BuildGroup> _groups;

    private readonly string _projectPath;
    private readonly string _tempPath;
    private readonly string _outputPath;

    private readonly bool _ignoreEmptyFileNames;
    private readonly bool _ignoreEmptyFolderNames;
    private readonly bool _ignoreHiddenAttribute;

    private readonly ConfigurationHandler _configurationHandler;

    private readonly HashSet<string> _consumedFiles;

    public ContentBuilder(ContentBuilderOptions options, ProjectDescription project, RuleProvider ruleProvider, BuildItemManifestSerializer itemManifestSerializer)
    {
        _project = project;
        _ruleProvider = ruleProvider;
        _itemManifestSerializer = itemManifestSerializer;

        _groups = new List<BuildGroup>();
        _consumedFiles = new HashSet<string>();

        _configurationHandler = new ConfigurationHandler();

        _projectPath = options.ProjectPath;
        _tempPath = options.TempPath;
        _outputPath = options.OutputPath;

        _ignoreEmptyFileNames = options.IgnoreEmptyFileNames;
        _ignoreEmptyFolderNames = options.IgnoreEmptyFolderNames;
        _ignoreHiddenAttribute = options.IgnoreHiddenAttribute;
    }

    public bool PrepareConfiguration(string? activeConfiguration, IReport? report)
    {
        return _configurationHandler.Prepare(_project.Configuration, activeConfiguration, report);
    }

    public bool BuildGroups(IReport? report)
    {
        if (_project.ItemGroups == null)
        {
            report?.Error("No 'ItemGroups' defined!");
            return false;
        }

        bool success = true;
        foreach (GroupDescription groupDesc in _project.ItemGroups)
        {
            if (groupDesc.Path == null)
            {
                report?.Error("No 'ItemGroups' defined!");
                success = false;
                continue;
            }

            if (groupDesc.Ruleset == null || groupDesc.Ruleset.Length == 0)
            {
                report?.Error("Missing 'Ruleset'!");
                success = false;
                continue;
            }

            success = BuildGroup(groupDesc, report) && success;
        }

        return success;
    }

    private bool BuildGroup(GroupDescription groupDesc, IReport? report)
    {
        BuildGroup buildGroup = new BuildGroup();
        AddItems(buildGroup, Path.GetFullPath(groupDesc.Path!, _projectPath), groupDesc.Recursive, groupDesc.Ruleset!, report);
        _groups.Add(buildGroup);

        report?.BeginGroup(groupDesc.Description ?? "path", buildGroup.Items.Count);

        Context context = new()
        {
            OutputPath = _outputPath,
            ProjectPath = _projectPath,
            TempPath = _tempPath,
            GroupPath = groupDesc.Path!,
        };
        
        bool success = true;
        foreach (BuildGroup.ItemType item in buildGroup.Items)
        {
            string ext = Path.GetExtension(item.Path).TrimStart('.');
            string name = Path.GetFileNameWithoutExtension(item.Path);
            string? dir = Path.GetDirectoryName(item.Path);
 
            context.ItemName = name;
            context.ItemExtension = ext;
            context.ItemPath = item.Path;
            context.ItemRelativePath = dir != null ? Path.GetRelativePath(context.ProjectPath, dir) : string.Empty;

            report?.GroupItem(name);
            success = ProcessItem(context, item.Rule, report) && success;
        }

        report?.EndGroup();

        return success;
    }

    private void AddItems(BuildGroup buildGroup, string groupPath, bool recursive, RulesetDescription[] ruleset, IReport? report)
    {
        foreach (string itemPath in Directory.EnumerateFiles(groupPath))
        {
            TryAddItem(buildGroup, itemPath, ruleset, report);
        }

        if (!recursive)
            return;

        foreach (string subGroupPath in Directory.EnumerateDirectories(groupPath))
        {
            DirectoryInfo dirInfo = new DirectoryInfo(subGroupPath);

            if (_ignoreEmptyFolderNames && dirInfo.Name.StartsWith('.'))
                continue;

            AddItems(buildGroup, subGroupPath, recursive, ruleset, report);
        }
    }

    private void TryAddItem(BuildGroup buildGroup, string itemPath, RulesetDescription[] ruleset, IReport? report)
    {
        if (_consumedFiles.Contains(itemPath))
        {
            report?.Info($"SKIP {itemPath}");
            return;
        }

        string ext = Path.GetExtension(itemPath).TrimStart('.');
        string name = Path.GetFileNameWithoutExtension(itemPath);
        string? dir = Path.GetDirectoryName(itemPath);
        string parentDir = string.Empty;
        if (dir != null)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dir);
            parentDir = dirInfo.Name;
        }

        FileInfo fileInfo = new FileInfo(itemPath);
        if (_ignoreHiddenAttribute && (fileInfo.Attributes & FileAttributes.Hidden) != 0)
        {
            report?.Info($"IGNORE: {itemPath}");
            return;
        }

        if (_ignoreEmptyFileNames && string.IsNullOrEmpty(name))
        {
            report?.Info($"IGNORE: {itemPath}");
            return;
        }

        foreach (RulesetDescription ruleDesc in ruleset)
        {
            string fullRulePath = Path.GetFullPath(ruleDesc.Path ?? "./", _projectPath);
            _ruleProvider.GetRule(fullRulePath, out Rule rule);

            if (!_configurationHandler.IsApplicable(ruleDesc.Configuration))
                continue;

            if (ValidRule(name, ext, parentDir, rule))
            {
                buildGroup.Items.Add(new BuildGroup.ItemType(itemPath, rule));
                return;
            }
        }

        report?.Info($"IGNORE: {itemPath}");
    }

    private bool ProcessItem(Context context, in Rule rule, IReport? report)
    {
        IItemProcessor processor = rule.ProcessorFactory.Create(context, rule.Settings);
        bool canSkip = rule.ProcessorFactory.SimpleProcessor ? CanSkipSimple(context, processor, rule) : CanSkipComplex(context, processor, rule);
        bool result = true;
        if (canSkip)
        {
            report?.Info($"SKIP: {context.ItemPath}");
        }
        else
        {
            report?.Info($"PROCESS: {context.ItemPath}");
            result = processor.Process(report);
            if (result)
                BuildItemManifest(context, processor);
        }

        // ReSharper disable once SuspiciousTypeConversion.Global
        if (processor is IDisposable disposable)
            disposable.Dispose();

        return result;
    }

    private bool CanSkipSimple(Context context, IItemProcessor processor, in Rule rule)
    {
        DateTime createTime = File.GetCreationTimeUtc(context.ItemPath);
        DateTime lastWriteTime = File.GetLastWriteTimeUtc(context.ItemPath);
        DateTime mostRecent = lastWriteTime > createTime ? lastWriteTime : createTime;

        string[] outputPaths = processor.GetOutputPaths();
        foreach (string outputPath in outputPaths)
        {
            DateTime outCreateTime = File.GetCreationTimeUtc(outputPath);
            DateTime outLastWriteTime = File.GetLastWriteTimeUtc(outputPath);
            DateTime outMostRecent = outLastWriteTime > outCreateTime ? outLastWriteTime : outCreateTime;

            if (mostRecent > outMostRecent || rule.LastUpdateTime > outMostRecent)
                return false;
        }

        return true;
    }

    private bool CanSkipComplex(Context context, IItemProcessor processor, in Rule rule)
    {
        string manifestPath = Path.Combine(context.TempPath, context.ItemRelativePath, ItemManifestName(context.ItemName));
        if (!File.Exists(manifestPath))
            return false;

        DateTime createTime = File.GetCreationTimeUtc(context.ItemPath);
        DateTime lastWriteTime = File.GetLastWriteTimeUtc(context.ItemPath);
        DateTime mostRecent = lastWriteTime > createTime ? lastWriteTime : createTime;

        DateTime manifestTime = File.GetLastWriteTimeUtc(manifestPath);
        if (mostRecent > manifestTime)
            return false;

        if (!_itemManifestSerializer.Deserialize(manifestPath, out BuildItemManifest manifest))
            return false;

        if (manifest.Output != null)
        {
            // NOTE: 
            foreach (string outFilePath in manifest.Output)
            {
                if (!File.Exists(outFilePath))
                    return false;
            }
        }

        if (manifest.Dependencies != null)
        {
            foreach (string depFilePath in manifest.Dependencies)
            {
                if (!File.Exists(depFilePath))
                    return false;

                DateTime depCreateTime = File.GetCreationTimeUtc(depFilePath);
                DateTime depLastWriteTime = File.GetLastWriteTimeUtc(depFilePath);
                DateTime depMostRecent = depLastWriteTime > depCreateTime ? depLastWriteTime : depCreateTime;

                if (depMostRecent > manifestTime)
                    return false;
            }
        }

        // TODO: what about directory dependencies?
        return true;
    }

    private void BuildItemManifest(Context context, IItemProcessor processor)
    {
        string dirPath = Path.Combine(context.TempPath, context.ItemRelativePath);
        Directory.CreateDirectory(dirPath);

        string manifestPath = Path.Combine(dirPath, ItemManifestName(context.ItemName));

        BuildItemManifest manifest = new()
        {
            Output = processor.GetOutputPaths(),
            Dependencies = processor.GetDependencies()
        };
        _itemManifestSerializer.Serialize(manifestPath, manifest);
    }

    private string ItemManifestName(string itemName) => $"_{itemName}.txt";

    private bool ValidRule(string fileName, string fileExt, string folderName, in Rule rule)
    {
        if (rule.Header.FileTypes == null)
            return false;

        if (!rule.Header.FileTypes.Any(ext => string.Equals(ext, fileExt, StringComparison.OrdinalIgnoreCase)))
            return false;

        if (rule.Header.FileNamePattern != null)
        {
            Regex regex = new Regex(rule.Header.FileNamePattern, RegexOptions.Singleline | RegexOptions.NonBacktracking);
            if (!regex.Match(fileName).Success)
                return false;
        }

        if (rule.Header.FolderPattern != null)
        {
            Regex regex = new Regex(rule.Header.FolderPattern, RegexOptions.Singleline | RegexOptions.NonBacktracking);
            if (!regex.Match(folderName).Success)
                return false;
        }

        return true;
    }

    private class Context : IProcessorContext
    {
        public string ItemPath { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string ItemExtension { get; set; } = string.Empty;
        public string ItemRelativePath { get; set; } = string.Empty;
        public string GroupPath { get; set; } = string.Empty;
        public string ProjectPath { get; set; } = string.Empty;
        public string OutputPath { get; set; } = string.Empty;
        public string TempPath { get; set; } = string.Empty;
    }
}