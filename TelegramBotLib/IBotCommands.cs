using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using System.Linq;
using System.Security.Cryptography;

namespace TelegramBotLib
{
    public class BotCommands
    {

        public readonly Telegram.Bot.Types.User User;

        public BotCommands(Telegram.Bot.Types.User user)
        {
            User = user;
        }

        public Dictionary<string, (string method, string param)> KeyboardData;

        public virtual Task Start()
        {
            return null;
        }
        public virtual void OnUnknownCommand(string command)
        {
            Console.WriteLine($"UnknownCommand: {command}");
        }

        private string GenerateHash(string input)
        {
            string code = "";
            using (var sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                for (var index = 0; index < 10; index++)
                {
                    var b = hash[index];
                    code += b.ToString("X2");
                }
            }
            return code;
        }

        private InlineKeyboardMarkup CreateInlineKeyboardMarkup(IEnumerable<IEnumerable<(string text, string data)>> keyboard)
        {
            if (keyboard == null || keyboard.Count() == 0) return null;

            KeyboardData = new Dictionary<string, (string method, string param)>();

            return new InlineKeyboardMarkup(keyboard.Select(line =>
                line.Select(button => {
                    KeyboardData.Add(GenerateHash(button.text + button.data), button);
                    return InlineKeyboardButton.WithCallbackData(button.text, button.data);
                })));
        }

        private ReplyKeyboardMarkup CreateReplyKeyboardMarkup(IEnumerable<IEnumerable<(string text, string data)>> keyboard)
        {
            if (keyboard == null || keyboard.Count() == 0) return null;

            KeyboardData = new Dictionary<string, (string method, string param)>();

            return new ReplyKeyboardMarkup(keyboard.Select(line =>
                line.Select(button => {
                    KeyboardData.Add(GenerateHash(button.text + button.data), button);
                    return new KeyboardButton(button.text);
                })));
        }

        protected async Task SendMessage(string text, IEnumerable<IEnumerable<(string text, string data)>> keyboard = null, bool inline = true)
        {
            await OnSendMessage.Invoke(this, text, inline ? (IReplyMarkup)CreateInlineKeyboardMarkup(keyboard) : (IReplyMarkup)CreateReplyKeyboardMarkup(keyboard));
        }

        public event SendMessageDelegate OnSendMessage;

        public delegate Task SendMessageDelegate(BotCommands commands, string text, IReplyMarkup keyboard = null);

    }
}