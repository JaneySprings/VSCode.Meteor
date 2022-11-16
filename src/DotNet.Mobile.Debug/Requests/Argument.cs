using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace DotNet.Mobile.Debug.Protocol;

public class Argument {
    [JsonPropertyName("linesStartAt1")] public bool LinesStartAt1 { get; set; }
    [JsonPropertyName("pathFormat")] public string PathFormat { get; set; }
    [JsonPropertyName("expression")] public string Expression { get; set; }
    [JsonPropertyName("address")] public string Address { get; set; }
    [JsonPropertyName("threadId")] public int ThreadId { get; set; }
    [JsonPropertyName("frameId")] public int FrameId { get; set; }
    [JsonPropertyName("projectType")] public int ProjectType { get; set; }
    [JsonPropertyName("port")] public int Port { get; set; } = -1;
    [JsonPropertyName("debugPort")] public int DebugPort { get; set; }
    [JsonPropertyName("variablesReference")] public int VariablesReference { get; set; } = -1;
    [JsonPropertyName("levels")] public int Levels { get; set; } = 10;

    [JsonPropertyName("lines")] public List<int> Lines { get; set; }

    [JsonPropertyName("__exceptionOptions")] public List<ExceptionOption> ExceptionOptions { get; set; }
    [JsonPropertyName("source")] public Source Source { get; set; }
}