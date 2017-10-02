using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using Seq.Apps;
using Seq.Apps.LogEvents;
using Telegram.Bot;

namespace Seq.App.Telegram
{
    [SeqApp("Telegram notifier", Description = "Sends messages matching a view to Telegram.")]
    public class TelegramReactor : Reactor, ISubscribeTo<LogEventData>
    {
        [SeqAppSetting(
            DisplayName = "Bot authentication token",
            HelpText = "https://core.telegram.org/bots/api#authorizing-your-bot")]
        public string BotToken { get; set; }

        [SeqAppSetting(
            DisplayName = "Group chat identifier",
            HelpText = "Unique identifier for your group chat")]
        public int ChatId { get; set; }

        [SeqAppSetting(
            DisplayName = "Seq Base URL",
            HelpText = "Used for generating perma links to events in Telegram messages.",
            IsOptional = true)]
        public string BaseUrl { get; set; }

        [SeqAppSetting(
            HelpText = "The message template to use when writing the message to Telegram. Refer to https://tlgrm.ru/docs/bots/api#formatting-options for formatting options. Event property values can be added in the format [PropertyKey]. The default is \"[RenderedMessage]\"",
            IsOptional = true)]
        public string MessageTemplate { get; set; }

        [SeqAppSetting(
            DisplayName = "Suppression time (minutes)",
            IsOptional = true,
            HelpText = "Once an event type has been sent to Telegram, the time to wait before sending again. The default is zero.")]
        public int SuppressionMinutes { get; set; } = 0;

        readonly HttpClientHandler _httpClientHandler = new HttpClientHandler();
        readonly ConcurrentDictionary<uint, DateTime> _lastSeen = new ConcurrentDictionary<uint, DateTime>();

        public void On(Event<LogEventData> evt)
        {
            if (!CanSend(evt))
                return;
            using (var http = new HttpClient(_httpClientHandler, false))
            {
                var formatter = new MessageFormatter(Log, GetBaseUri(), MessageTemplate);
                var message = formatter.GenerateMessageText(evt);
                var telegram = new TelegramBotClient(BotToken, http);
                telegram.SendTextMessageAsync(ChatId, message).Wait();
            }
        }

        bool CanSend(Event<LogEventData> evt)
        {
            if (SuppressionMinutes < 1)
                return true;
            var canSend = false;
            _lastSeen.AddOrUpdate(
                key: evt.EventType,
                addValueFactory: type =>
                {
                    canSend = true;
                    return DateTime.Now;
                },
                updateValueFactory: (type, lastTime) =>
                {
                    var now = DateTime.Now;
                    if ((now - lastTime).TotalMinutes > SuppressionMinutes)
                    {
                        canSend = true;
                        return now;
                    }
                    return lastTime;
                });
            return canSend;
        }

        string GetBaseUri() => BaseUrl ?? Host.ListenUris.FirstOrDefault();
    }


}
