using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Seq.Apps;
using Seq.Apps.LogEvents;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Seq.App.Telegram
{
    [SeqApp("Telegram notifier", Description = "Sends messages matching a view to Telegram.")]
    public class TelegramReactor : Reactor, ISubscribeTo<LogEventData>
    {
        [SeqAppSetting(
            DisplayName = "Bot authentication token",
            HelpText = "Refer to Telegram api documentation https://core.telegram.org/bots/api#authorizing-your-bot")]
        public string BotToken { get; set; }

        [SeqAppSetting(
            DisplayName = "Group chat identifier",
            HelpText = "Unique identifier for your group chat (include minus)")]
        public long ChatId { get; set; }

        [SeqAppSetting(
            DisplayName = "Seq Base URL",
            HelpText = "Used for generating perma links to events in Telegram messages.",
            IsOptional = true)]
        public string BaseUrl { get; set; }

        [SeqAppSetting(
            HelpText = "The message template to use when writing the message to Telegram. Refer to https://tlgrm.ru/docs/bots/api#formatting-options for Markdown style formatting options. Event property values can be added in the format [PropertyKey]. The default is \"[RenderedMessage]\"",
            IsOptional = true)]
        public string MessageTemplate { get; set; }

        [SeqAppSetting(
            DisplayName = "Suppression time (minutes)",
            IsOptional = true,
            HelpText = "Once an event type has been sent to Telegram, the time to wait before sending again. The default is zero.")]
        public int SuppressionMinutes { get; set; } = 0;

        readonly HttpClient _http = new HttpClient();

        readonly Throttling<uint> _throttling = new Throttling<uint>();

        public void On(Event<LogEventData> evt)
        {
            if (!_throttling.TryBegin(evt.EventType, TimeSpan.FromMinutes(SuppressionMinutes)))
                return;
            var formatter = new MessageFormatter(Log, GetBaseUri(), MessageTemplate);
            var message = formatter.GenerateMessageText(evt);
            var telegram = new TelegramBotClient(BotToken, _http);
            Task.Run(() => telegram.SendTextMessageAsync(ChatId, message, ParseMode.Markdown));
        }

        string GetBaseUri() => BaseUrl ?? Host.ListenUris.FirstOrDefault();
    }
}
