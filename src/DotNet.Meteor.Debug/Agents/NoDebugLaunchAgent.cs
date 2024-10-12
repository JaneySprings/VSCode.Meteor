using DotNet.Meteor.Debug.Extensions;
using DotNet.Meteor.Common;
using Mono.Debugging.Soft;
using DotNet.Meteor.Common.Processes;
using DotNet.Meteor.Common.Apple;
using DotNet.Meteor.Common.Android;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;

namespace DotNet.Meteor.Debug;

public class NoDebugLaunchAgent : BaseLaunchAgent {
    public NoDebugLaunchAgent(LaunchConfiguration configuration) : base(configuration) { }
    public override void Launch(DebugSession debugSession) {
        if (Configuration.Device.IsAndroid)
            LaunchAndroid(debugSession);
        if (Configuration.Device.IsIPhone)
            LaunchAppleMobile(debugSession);
        if (Configuration.Device.IsMacCatalyst)
            LaunchMacCatalyst(debugSession);
        if (Configuration.Device.IsWindows)
            LaunchWindows(debugSession);
    }
    public override void Connect(SoftDebuggerSession session) {}

    private void LaunchAppleMobile(DebugSession debugSession) {
        if (RuntimeSystem.IsWindows) {
            var programPath = Path.ChangeExtension(Configuration.ProgramPath, ".ipa");
            // var forwardingProcess = IDeviceTool.Proxy(Configuration.Device.Serial, Configuration.ReloadHostPort, debugSession);
            // Disposables.Add(() => forwardingProcess.Terminate());

            IDeviceTool.Installer(Configuration.Device.Serial, programPath, debugSession);
            debugSession.OnImportantDataReceived("Application installed on device. Please tap on the app icon to run it.");
            return;
        }

        if (Configuration.Device.IsEmulator) {
            var appProcess = MonoLauncher.DebugSim(Configuration.Device.Serial, Configuration.ProgramPath, Configuration.DebugPort, debugSession);
            Disposables.Add(() => appProcess.Terminate());
        } else {
            var hotReloadPortForwarding = MonoLauncher.TcpTunnel(Configuration.Device.Serial, Configuration.ReloadHostPort, debugSession);
            MonoLauncher.InstallDev(Configuration.Device.Serial, Configuration.ProgramPath, debugSession);
            var appProcess = MonoLauncher.DebugDev(Configuration.Device.Serial, Configuration.ProgramPath, Configuration.DebugPort, debugSession);
            Disposables.Add(() => appProcess.Terminate());
            Disposables.Add(() => hotReloadPortForwarding.Terminate());
        }
    }
    private void LaunchMacCatalyst(IProcessLogger logger) {
        var tool = AppleSdkLocator.OpenTool();
        var processRunner = new ProcessRunner(tool, new ProcessArgumentBuilder().AppendQuoted(Configuration.ProgramPath));
        var result = processRunner.WaitForExit();

        if (!result.Success)
            throw ServerExtensions.GetProtocolException(string.Join(Environment.NewLine, result.StandardError));
    }
    private void LaunchWindows(IProcessLogger logger) {
        var program = new FileInfo(Configuration.ProgramPath);
        var process = new ProcessRunner(program, new ProcessArgumentBuilder(), logger).Start();
        Disposables.Add(() => process.Terminate());
    }
    private void LaunchAndroid(IProcessLogger logger) {
        var applicationId = Configuration.GetApplicationName();
        if (Configuration.Device.IsEmulator)
            Configuration.Device.Serial = AndroidEmulator.Run(Configuration.Device.Name).Serial;

        AndroidDebugBridge.Forward(Configuration.Device.Serial, Configuration.ReloadHostPort);
        Disposables.Add(() => AndroidDebugBridge.RemoveForward(Configuration.Device.Serial));

        if (Configuration.UninstallApp)
            AndroidDebugBridge.Uninstall(Configuration.Device.Serial, applicationId, logger);

        AndroidDebugBridge.Install(Configuration.Device.Serial, Configuration.ProgramPath, logger);
        AndroidDebugBridge.Launch(Configuration.Device.Serial, applicationId, logger);
        AndroidDebugBridge.Flush(Configuration.Device.Serial);

        var logcatProcess = AndroidDebugBridge.Logcat(Configuration.Device.Serial, logger);
        Disposables.Add(() => logcatProcess.Terminate());
    }
}