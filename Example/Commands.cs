using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Example
{
    public class Commands : TelegramBotLib.BotCommands
    {
        public Commands(User user) : base(user)
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
