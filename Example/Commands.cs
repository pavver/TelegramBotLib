using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Example
{
    public class Commands : TelegramBotLib.BotCommands
    {
        public Commands(User chat) : base(chat)
        {
        }

        public override async Task Start()
        {
            await SendMessage("123",
                new[] {
                    new[]{ ("123","123")}
                });
        }
    }
}
