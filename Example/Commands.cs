using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Example
{
    public class Commands : TelegramBotLib.BotCommands
    {
        private string[] data = new string[]
        {
            ""
        };

        public Commands(Chat chat) : base(chat)
        {
        }

        public override async Task Start()
        {
            await SendMessage("123",
                new[] {
                    new[]{ ("123",nameof())}
                });
        }

        public 

    }
}
