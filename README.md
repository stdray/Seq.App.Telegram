# Seq.App.Telegram
An app for Seq (http://getseq.net) that forwards messages to Telegram group chat. 


### In order to use Seq.App.Telegram you will need:
* **Bot authentication token**. You can use existing bot's token or create a new one. Refer to docs at https://core.telegram.org/bots/api#authorizing-your-bot.
* **Chat id**. Invite [@ShowJsonBot](https://telegram.me/ShowJsonBot) into your chat and it will send you a message. Copy `id` value from `chat` section including leading minus.
	`chat` section example:
	```json
	 "chat": {
	   "title": "Some chat",
	   "type": "group",
	   "all_members_are_administrators": true,
	   "id": -221908654
	  },
	```