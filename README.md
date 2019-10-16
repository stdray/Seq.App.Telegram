# Seq.App.Telegram
An app for Seq (http://getseq.net) that forwards messages to Telegram group chat. Socks5 proxy supported.

[![NuGet](https://img.shields.io/nuget/v/Seq.App.Telegram.svg?style=flat-square)](https://www.nuget.org/packages/Seq.App.Telegram/)

### In order to use Seq.App.Telegram you will need:
* **Bot authentication token**. You can use existing bot's token or create a new one. Refer to docs at https://core.telegram.org/bots/api#authorizing-your-bot.
* **Chat id**. Invite [@ShowJsonBot](https://telegram.me/ShowJsonBot) or [@RawDataBot](https://telegram.me/RawDataBot) into your chat and it will send you a message. Copy `id` value from `chat` section including leading minus.
	`chat` section example:
	```json
	 "chat": {
	   "title": "Some chat",
	   "type": "group",
	   "all_members_are_administrators": true,
	   "id": -221908654
	  },
	```