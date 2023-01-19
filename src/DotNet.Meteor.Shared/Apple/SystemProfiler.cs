using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotNet.Meteor.Processes;
using DotNet.Meteor.Shared;
using System;

namespace DotNet.Meteor.Apple {
    internal static class SystemProfiler {
        public static List<DeviceData> PhysicalDevices() {
            var profiler = PathUtils.SystemProfilerTool();
            var devices = new List<DeviceData>();
            var regex = new Regex(@"(iPhone:)[^,]*?Version:\s+(?<ver>\d+.\d+)[^,]*?Serial\sNumber:\s+(?<id>\S+)");

            ProcessResult result = new ProcessRunner(profiler, new ProcessArgumentBuilder()
                .Append("SPUSBDataType"))
                .WaitForExit();

            if (result.ExitCode != 0)
                throw new Exception(string.Join(Environment.NewLine, result.StandardError));

            var output = string.Join(Environment.NewLine, result.StandardOutput);

            foreach (Match match in regex.Matches(output)) {
                var version = match.Groups["ver"].Value;
                var serial = match.Groups["id"].Value;
                //For modern iOS devices, the serial number is 24 characters long
                if (serial.Length == 24)
                    serial = serial.Insert(8, "-");

                devices.Add(new DeviceData {
                    IsEmulator = false,
                    IsRunning = true,
                    IsMobile = true,
                    RuntimeId = Runtimes.iOSArm64,
                    Name = $"iPhone {version}",
                    Details = Details.iOSDevice,
                    Platform = Platforms.iOS,
                    Serial = serial
                });
            }
            return devices;
        }

        public static bool IsArch64() {
            var profiler = PathUtils.SystemProfilerTool();
            ProcessResult result = new ProcessRunner(profiler, new ProcessArgumentBuilder()
                .Append("SPHardwareDataType"))
                .WaitForExit();

            if (result.ExitCode != 0)
                throw new Exception(string.Join(Environment.NewLine, result.StandardError));

            var output = string.Join(Environment.NewLine, result.StandardOutput);
            var appleSilicon = new Regex(@"Chip: *(?<name>.+)").Match(output);

            return appleSilicon.Success;
        }
    }
}