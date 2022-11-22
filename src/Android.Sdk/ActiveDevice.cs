using System;
using System.Linq;
using DotNet.Mobile.Shared;

namespace Android.Sdk {
    public class ActiveDevice {
        public string Serial { get; set; }
        public string State { get; set; }
    }

    public static class PhysicalDeviceExtensions {
        public static string GetAVDName(this ActiveDevice device) {
            var adb = PathUtils.GetADBTool();
            ProcessResult result = ProcessRunner.Run(adb, new ProcessArgumentBuilder()
                .Append("-s", device.Serial)
                .Append("emu", "avd", "name")
            );

            if (result.ExitCode != 0)
                throw new Exception(string.Join(Environment.NewLine, result.StandardError));

            return result.StandardOutput.FirstOrDefault();
        }

        public static DeviceData ToDeviceData(this ActiveDevice device) {
            bool isEmulator = device.Serial.Contains("emulator");
            var result = new DeviceData {
                Serial = device.Serial,
                Platform = Platform.Android,
                IsRunning = true,
                IsEmulator = isEmulator
            };

            if (isEmulator) {
                result.Name = device.GetAVDName();
                result.Details = "Emulator";
            } else {
                result.Name = device.Serial;
                result.Details = "Physical Device";
            }

            return result;
        }
    }
}