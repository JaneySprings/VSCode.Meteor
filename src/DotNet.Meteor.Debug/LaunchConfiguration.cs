﻿using DotNet.Meteor.Common;
using DotNet.Meteor.Debug.Extensions;
using Newtonsoft.Json.Linq;
using Mono.Debugging.Client;

namespace DotNet.Meteor.Debug;

public class LaunchConfiguration {
    public Project Project { get; init; }
    public DeviceData Device { get; init; }
    public string ProgramPath { get; init; }

    public bool UninstallApp { get; init; }
    public int DebugPort { get; init; }
    public int ReloadHostPort { get; init; }
    public int ProfilerPort { get; init; }
    public DebuggerSessionOptions DebuggerSessionOptions { get; init; }

    private ProfilerMode Profiler { get; init; }
    private bool SkipDebug { get; init; }

    public LaunchConfiguration(Dictionary<string, JToken> configurationProperties) {
        Project = configurationProperties["project"].ToClass<Project>()!;
        Device = configurationProperties["device"].ToClass<DeviceData>()!;
        
        UninstallApp = configurationProperties.TryGetValue("uninstallApp").ToValue<bool>();
        SkipDebug = configurationProperties.TryGetValue("skipDebug").ToValue<bool>();
        DebugPort = configurationProperties.TryGetValue("debuggingPort").ToValue<int>();
        ReloadHostPort = configurationProperties.TryGetValue("reloadHost").ToValue<int>();
        ProfilerPort = configurationProperties.TryGetValue("profilerPort").ToValue<int>();
        Profiler = configurationProperties.TryGetValue("profilerMode").ToValue<ProfilerMode>();
        DebuggerSessionOptions = configurationProperties.TryGetValue("debuggerOptions")?.ToClass<DebuggerSessionOptions>() 
            ?? ServerExtensions.DefaultDebuggerOptions;

        DebugPort = DebugPort == 0 ? ServerExtensions.FindFreePort() : DebugPort;
        ReloadHostPort = ReloadHostPort == 0 ? ServerExtensions.FindFreePort() : ReloadHostPort;
        ProfilerPort = ProfilerPort == 0 ? ServerExtensions.FindFreePort() : ProfilerPort;

        ProgramPath = Project.GetRelativePath(configurationProperties.TryGetValue("program").ToClass<string>());
        if (!File.Exists(ProgramPath) && !Directory.Exists(ProgramPath))
            throw ServerExtensions.GetProtocolException($"Incorrect path to program: '{ProgramPath}'");
    }

    public string GetApplicationName() {
        if (!Device.IsAndroid)
            return Path.GetFileNameWithoutExtension(ProgramPath);

        var assemblyName = Path.GetFileNameWithoutExtension(ProgramPath);
        return assemblyName.Replace("-Signed", "");
    }
    public string GetAssemblyPath() {
        if (Device.IsMacCatalyst)
            return Path.Combine(ProgramPath, "Contents", "MonoBundle");
        if (Device.IsIPhone)
            return ProgramPath;
        if (Device.IsAndroid)
            return ServerExtensions.ExtractAndroidAssemblies(ProgramPath);

        return Path.GetDirectoryName(ProgramPath)!;
    }
    public BaseLaunchAgent GetLaunchAgent() {
        if (Profiler == ProfilerMode.Trace)
            return new TraceLaunchAgent(this);
        if (Profiler == ProfilerMode.GCDump)
            return new GCDumpLaunchAgent(this);
        if (!SkipDebug)
            return new DebugLaunchAgent(this);

        return new NoDebugLaunchAgent(this);
    }

    private enum ProfilerMode { None, Trace, GCDump }
}