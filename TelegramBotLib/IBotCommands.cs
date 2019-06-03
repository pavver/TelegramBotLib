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

        public readonly Telegram.Bot.Types.Chat Chat;

        public BotCommands(Telegram.Bot.Types.Chat chat)
        {
            Chat = chat;
        }

        public Dictionary<string, (string method, string param)> KeyboardData;

        // ReSharper disable once UnusedMember.Global
        public virtual Task Start()
        {
            return null;
        }

        public virtual void OnUnknownCommand(string command)
        {
            Console.WriteLine($"UnknownCommand: {command}");
        }

        private static string GenerateHash(string input)
        {
            var code = "";
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

        private InlineKeyboardMarkup CreateInlineKeyboardMarkup(
            IEnumerable<IEnumerable<(string text, string data)>> keyboard)
        {
            if (keyboard == null) return null;

            KeyboardData = new Dictionary<string, (string method, string param)>();

            // ReSharper disable once PossibleMultipleEnumeration
            return new InlineKeyboardMarkup(keyboard.Select(line =>
                line.Select(button =>
                {
                    KeyboardData.Add(GenerateHash(button.text + button.data), button);
                    return InlineKeyboardButton.WithCallbackData(button.text, button.data);
                })));
        }

        private ReplyKeyboardMarkup CreateReplyKeyboardMarkup(
            IEnumerable<IEnumerable<(string text, string data)>> keyboard)
        {
            if (keyboard == null) return null;

            KeyboardData = new Dictionary<string, (string method, string param)>();

            // ReSharper disable once PossibleMultipleEnumeration
            return new ReplyKeyboardMarkup(keyboard.Select(line =>
                line.Select(button =>
                {
                    KeyboardData.Add(GenerateHash(button.text + button.data), button);
                    return new KeyboardButton(button.text);
                })));
        }

        protected async Task SendMessage(string text,
            IEnumerable<IEnumerable<(string text, string data)>> keyboard = null, bool inline = true)
        {
            await OnSendMessage.Invoke(this, text,
                inline ? CreateInlineKeyboardMarkup(keyboard) : (IReplyMarkup) CreateReplyKeyboardMarkup(keyboard));
        }

        public event SendMessageDelegate OnSendMessage = (commands, text, keyboard) => Task.CompletedTask;

        public delegate Task SendMessageDelegate(BotCommands commands, string text, IReplyMarkup keyboard = null);

    }
}