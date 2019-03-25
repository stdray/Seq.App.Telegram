using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Seq.Apps;
using Seq.Apps.LogEvents;
using Serilog;

namespace Seq.App.Telegram
{
    public class MessageFormatter
    {
        static readonly Regex PlaceholdersRegex = new Regex("(\\[(?<key>[^\\[\\]]+?)(\\:(?<format>[^\\[\\]]+?))?\\])", RegexOptions.CultureInvariant | RegexOptions.Compiled);

        public MessageFormatter(ILogger log, string baseUrl, string messageTemplate)
        {
            Log = log;
            MessageTemplate = messageTemplate ?? "[RenderedMessage]";
            BaseUrl = baseUrl;
        }

        public ILogger Log { get; }
        public string MessageTemplate { get; }
        public string BaseUrl { get; }

        public string GenerateMessageText(Event<LogEventData> evt)
        {
            return $"{SubstitutePlaceholders(MessageTemplate, evt)} [link]({BaseUrl}/#/events?filter=@Id%3D%3D'{evt.Id}'&show=expanded)";
        }

        string SubstitutePlaceholders(string messageTemplateToUse, Event<LogEventData> evt)
        {
            var data = evt.Data;
            var eventType = evt.EventType;
            var level = data.Level;

            var placeholders = data.Properties?.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase)
                ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            AddValueIfKeyDoesntExist(placeholders, "Level", level);
            AddValueIfKeyDoesntExist(placeholders, "EventType", eventType);
            AddValueIfKeyDoesntExist(placeholders, "RenderedMessage", data.RenderedMessage);

            return PlaceholdersRegex.Replace(messageTemplateToUse, m =>
            {
                var key = m.Groups["key"].Value.ToLower();
                var format = m.Groups["format"].Value;
                return placeholders.ContainsKey(key) ? FormatValue(placeholders[key], format) : m.Value;
            });
        }

        string FormatValue(object value, string format)
        {
            var rawValue = value?.ToString() ?? "(Null)";

            if (string.IsNullOrWhiteSpace(format))
                return rawValue;
            try
            {
                return string.Format(format, rawValue);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not format message: {value} {format}", value, format);
            }

            return rawValue;
        }

        static void AddValueIfKeyDoesntExist(IDictionary<string, object> placeholders, string key, object value)
        {
            if (!placeholders.ContainsKey(key))
                placeholders.Add(key, value);
        }
    }
}
