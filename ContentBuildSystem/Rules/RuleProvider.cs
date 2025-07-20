using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ContentBuildSystem.Interfaces;
using ContentBuildSystem.Json;

namespace ContentBuildSystem.Rules;

public class RuleProvider
{
    private readonly RuleSerializer _serializer;
    private readonly Dictionary<string, Rule> _rules;
    private readonly Dictionary<string, ProcessorDesc> _processors;
    private readonly HashSet<string> _missing;

    public RuleProvider(RuleSerializer serializer)
    {
        _serializer = serializer;
        _rules = new Dictionary<string, Rule>();
        _processors = new Dictionary<string, ProcessorDesc>();
        _missing = new HashSet<string>();
    }

    public void AddProcessor(string name, IItemProcessorFactory factory, Type settingsType)
    {
        _processors.Add(name, new ProcessorDesc(factory, settingsType));
    }

    public void AddPlugin(IPlugin plugin, string? ns)
    {
        foreach (PluginDescriptor descriptor in plugin.Descriptors)
        {
            AddProcessor(ns != null ? $"{ns}.{descriptor.Name}" : descriptor.Name, descriptor.Factory, descriptor.SettingsType);
        }
    }

    private void ApplyBase(object obj, object bs)
    {
        Type type = obj.GetType();
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (FieldInfo fieldInfo in fields)
        {
            object? objValue = fieldInfo.GetValue(obj);
            object? baseValue = fieldInfo.GetValue(bs);

            if (objValue == null)
                fieldInfo.SetValue(obj, baseValue);
        }
    }

    public bool GetRule(string path, out Rule rule)
    {
        if (_rules.TryGetValue(path, out rule))
        {
            return true;
        }

        if (_missing.Contains(path))
        {
            rule = default;
            return false;
        }

        DateTime lastWriteTime = File.GetLastWriteTimeUtc(path);
        DateTime createTime = File.GetCreationTimeUtc(path);
        DateTime mostRecent = lastWriteTime > createTime ? lastWriteTime : createTime;

        string text = File.ReadAllText(path);
        RuleHeader header = _serializer.GetHeader(text)!;
        if (header.BasePath != null)
        {
            string dirPath = Path.GetDirectoryName(path)!;
            string basePath = Path.GetFullPath(header.BasePath, dirPath);

            if (!GetRule(basePath, out Rule baseRule))
            {
                _missing.Add(path);
                rule = default;
                return false;
            }

            header.Handler ??= baseRule.Header.Handler;
            header.FileTypes ??= baseRule.Header.FileTypes;
            header.FileNamePattern ??= baseRule.Header.FileNamePattern;
            header.FolderPattern ??= baseRule.Header.FolderPattern;
        }
        
        // TODO: resolve extensions using IParameterResolver<string[]>

        // TODO: apply base settings from base rule
        _processors.TryGetValue(header.Handler!, out ProcessorDesc desc);

        object? confSettings = _serializer.GetRawSettings(text, desc.SettingsType);

        // defSettings ??= Activator.CreateInstance(desc.SettingsType);
        // if (defSettings != null && confSettings != null)
        // {
        // ApplyBase(confSettings, defSettings);
        // }

        rule = new Rule(header, mostRecent, desc.Factory, confSettings);
        _rules.Add(path, rule);
        return true;
    }

    private readonly struct ProcessorDesc
    {
        public readonly IItemProcessorFactory Factory;
        public readonly Type SettingsType;

        public ProcessorDesc(IItemProcessorFactory factory, Type settingsType)
        {
            Factory = factory;
            SettingsType = settingsType;
        }
    }
}
