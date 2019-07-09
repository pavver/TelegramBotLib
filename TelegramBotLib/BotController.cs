using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotLib
{
    public class BotController
    {
        private readonly Telegram.Bot.TelegramBotClient _bot;

        /// <summary>
        /// Список "юзеров"
        /// </summary>
        protected Dictionary<long, BotCommands> UserCommands = new Dictionary<long, BotCommands>();

        /// <summary>
        /// Список команд в классе с командами (для економии времени на сканировании класса через рефлексию каждый раз)
        /// </summary>
        protected Dictionary<long, MethodInfo[]> CommandsMethodsList = new Dictionary<long, MethodInfo[]>();

        public BotController(Config config)
        {
            if (string.IsNullOrEmpty((config.ProxyHost)))
                _bot = new Telegram.Bot.TelegramBotClient(config.Token);
            else
            {
                IWebProxy webProxy;
                if (string.IsNullOrEmpty(config.ProxyUserName))
                    webProxy = new WebProxy()
                    {
                        Address = new Uri(config.ProxyHost)
                    };
                else
                    webProxy = new WebProxy()
                    {
                        Address = new Uri(config.ProxyHost),
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(config.ProxyUserName, config.ProxyPass)
                    };

                _bot = new Telegram.Bot.TelegramBotClient(config.Token, webProxy);
            }
        }

        public delegate (BotCommands, MethodInfo[]) NewUserDelegate(Chat chat, User user);

        public event NewUserDelegate OnNewUser = (chat, user) => (null, null);

        private readonly object _getCommandsLocker = new object();

        public BotCommands GetCommands(Chat chat, User user)
        {
            lock (_getCommandsLocker)
            {
                if (UserCommands.ContainsKey(chat.Id))
                    return UserCommands[chat.Id];

                var data = OnNewUser(chat, user);
                CommandsMethodsList.Add(chat.Id, data.Item2);
                data.Item1.OnSendMessage += Send;
                UserCommands.Add(chat.Id, data.Item1);
                return data.Item1;
            }
        }

        private async Task Send(BotCommands commands, string text, IReplyMarkup keyboard = null)
        {
            await _bot.SendTextMessageAsync(commands.Chat.Id, text, replyMarkup: keyboard);
        }

        private User _me;

        public User Me
        {
            get
            {
                if (_me == null)
                    _me = _bot.GetMeAsync().Result;
                return _me;
            }
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        public static bool CheckParams(ParameterInfo[] param)
        {
            // параметров нету? збс
            if (param.Length == 0) return true;
            // один параметр? строка? ну ок
            if (param.Length == 1 && param[0].ParameterType == typeof(string)) return true;
            // чего? нет нам не по пути
            return false;
        }

        public async Task RunCommand(BotCommands commands, string command)
        {
            // ожидаем ли мы сейчас такую команду
            if (!commands.KeyboardData.ContainsKey(command))
            {
                // неизвестная команда
                throw new InvalidOperationException();
            }

            // загружаем данные про вызываемый метод и передаваемый параметр
            var data = commands.KeyboardData[command];

            // ищем именно тот метод который нас интересует
            // ReSharper disable once InconsistentlySynchronizedField
            var method = CommandsMethodsList[commands.Chat.Id].Single(d => d.Name == data.method);

            // о да именно то ради чего все это и было написано, дергаем метод команды
            await (Task) method.Invoke(commands, new object[] {data.param});
        }

        public void StartReceiving()
        {
            _bot.StartReceiving(new[] {UpdateType.Message, UpdateType.CallbackQuery});
            _bot.OnMessage += Bot_OnMessage;
            _bot.OnCallbackQuery += Bot_OnCallbackQuery;
        }

        public void StopReceiving()
        {
            _bot.StopReceiving();
            _bot.OnMessage -= Bot_OnMessage;
            _bot.OnCallbackQuery -= Bot_OnCallbackQuery;
        }

        private async void Bot_OnCallbackQuery(object sender, Telegram.Bot.Args.CallbackQueryEventArgs e)
        {
            var user = GetCommands(e.CallbackQuery.Message.Chat, e.CallbackQuery.From);

            try
            {
                await RunCommand(user, e.CallbackQuery.Data);
            }
            catch (InvalidOperationException)
            {
                try
                {
                    await _bot.EditMessageTextAsync(e.CallbackQuery.Message.Chat.Id,
                        e.CallbackQuery.Message.MessageId,
                        e.CallbackQuery.Message.Text);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        private async void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            if (e.Message.Type != MessageType.Text) return;

            string command;
            if (e.Message.Chat.Type == ChatType.Private)
                command = e.Message.Text;
            else
            {
                var myName = $"@{Me.Username}";
                if (!e.Message.Text.EndsWith(myName))
                    return;
                command = e.Message.Text.Replace(myName, "");
            }

            var user = GetCommands(e.Message.Chat, e.Message.From);

            try
            {
                await RunCommand(user, command);
            }
            catch (InvalidOperationException)
            {
                await user.OnUnknownCommand(command);
            }
        }
    }
}