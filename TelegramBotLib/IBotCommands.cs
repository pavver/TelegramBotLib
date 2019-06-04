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
            IEnumerable<IEnumerable<(string text, string method, string param)>> keyboard)
        {
            if (keyboard == null) return null;

            KeyboardData = new Dictionary<string, (string method, string param)>();

            return new InlineKeyboardMarkup(keyboard.Select(line =>
                line.Select(button =>
                {
                    var (text, method, parameter) = button;
                    var hash = GenerateHash(text + method + parameter);
                    KeyboardData.Add(hash, (method, parameter));
                    return InlineKeyboardButton.WithCallbackData(text, hash);
                })));
        }

        private ReplyKeyboardMarkup CreateReplyKeyboardMarkup(
            IEnumerable<IEnumerable<(string text, string method, string param)>> keyboard)
        {
            if (keyboard == null) return null;

            KeyboardData = new Dictionary<string, (string method, string param)>();

            return new ReplyKeyboardMarkup(keyboard.Select(line =>
                line.Select(button =>
                {
                    var (text, method, parameter) = button;
                    KeyboardData.Add(text, (method, parameter));
                    return new KeyboardButton(text);
                })));
        }

        protected async Task SendMessage(string text,
            IEnumerable<IEnumerable<(string text, string method, string param)>> keyboard = null, bool inline = true)
        {
            await OnSendMessage.Invoke(this, text,
                inline ? CreateInlineKeyboardMarkup(keyboard) : (IReplyMarkup) CreateReplyKeyboardMarkup(keyboard));
        }

        public event SendMessageDelegate OnSendMessage = (commands, text, keyboard) => Task.CompletedTask;

        public delegate Task SendMessageDelegate(BotCommands commands, string text, IReplyMarkup keyboard = null);

    }
}