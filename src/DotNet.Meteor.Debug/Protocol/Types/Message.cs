using System.Text.Json.Serialization;

namespace DotNet.Meteor.Debug.Protocol.Types;

public class Message {
    /* Unique identifier for the message. */
    [JsonPropertyName("id")] public int Id { get; set; }

    /* A format string for the message. Embedded variables have the form '{name}'.
    * If variable name starts with an exclamation mark '!', the variable does not
    * contain user data (PII) and can be safely used for telemetry purposes. */
    [JsonPropertyName("format")] public string Format { get; set; }

    /* An object used as a dictionary for looking up the variables in the format string. */
    [JsonPropertyName("variables")] public object[] Variables { get; set; }

    /* If true send to telemetry. */
    [JsonPropertyName("sendTelemetry")] public bool SendTelemetry { get; set; }

    /* A url where additional information about this message can be found. */
    [JsonPropertyName("url")] public string Url { get; set; }

    /* A label that is presented to the user as the UI for opening the url. */
    [JsonPropertyName("urlLabel")] public string UrlLabel { get; set; }

    /* If true show user. */
    [JsonPropertyName("showUser")] public bool ShowUser { get; set; }

    public Message(string message, int id = 0) {
        this.Id = id;
        this.Format = message;
    }
}