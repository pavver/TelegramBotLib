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
    public class BotController<TCommandsType> where TCommandsType : BotCommands
    {
        private readonly Telegram.Bot.TelegramBotClient _bot;

        /// <summary>
        /// Список "типов" классов с командами
        /// </summary>
        protected Dictionary<long, TCommandsType> UserCommandsTypes = new Dictionary<long, TCommandsType>();

        /// <summary>
        /// Список команд в классе с командами (для економии времени на сканировании класса через рефлексию каждый раз)
        /// </summary>
        protected Dictionary<Type, MethodInfo[]> CommandsMethodsList = new Dictionary<Type, MethodInfo[]>();

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

        private readonly object _getCommandsLocker = new object();

        public TCommandsType GetCommands(Chat chat)
        {
            lock (_getCommandsLocker)
            {
                if (UserCommandsTypes.ContainsKey(chat.Id))
                    return UserCommandsTypes[chat.Id];

                var t = typeof(TCommandsType);
                var constructorInfo = t.GetConstructor(new[] {typeof(Chat)});
                var ret = (TCommandsType)constructorInfo.Invoke( new object[] {chat});
                ret.OnSendMessage += Send;
                UserCommandsTypes.Add(chat.Id, ret);
                return ret;
            }
        }

        private async Task Send(BotCommands commands, string text, IReplyMarkup keyboard = null)
        {
            await _bot.SendTextMessageAsync(commands.Chat.Id, text, replyMarkup: keyboard);
        }

        private readonly object _getMethodsLocker = new object();

        private void CheckLoadedMethods(Type t)
        {
            lock (_getMethodsLocker)
                if (!CommandsMethodsList.ContainsKey(t))
                {
                    // выбираем только "подходящие" методы
                    var m = t.GetMethods()
                        .Where(d => d.ReturnType == typeof(Task) && CheckParams(d.GetParameters()))
                        .ToArray();

                    // сохраняем чтобы в будущем не делать лишних телодвижений
                    CommandsMethodsList.Add(t, m);
                }
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private static bool CheckParams(ParameterInfo[] param)
        {
            // параметров нету? збс
            if (param.Length == 0) return true;
            // один параметр? строка? ну ок
            if (param.Length == 1 && param[0].ParameterType == typeof(string)) return true;
            // чего? нет нам не по пути
            return false;
        }

        public async Task RunCommand(TCommandsType commands, string command)
        {
            // ожидаем ли мы сейчас такую команду
            if (!commands.KeyboardData.ContainsKey(command))
            {
                // неизвестная команда
                await commands.OnUnknownCommand(command);
                return;
            }

            // загружаем данные про вызываемый метод и передаваемый параметр
            var data = commands.KeyboardData[command];

            // тип класса с командами
            Type t = commands.GetType();

            // проверяем загрузили ли мы список методов с этого типа данных
            CheckLoadedMethods(t);

            // ищем именно тот метод который нас интересует
            // ReSharper disable once InconsistentlySynchronizedField
            var method = CommandsMethodsList[t].Single(d => d.Name == data.method);

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
            var user = GetCommands(e.CallbackQuery.Message.Chat);

            await RunCommand(user, e.CallbackQuery.Data);
        }

        private async void Bot_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {
            if (e.Message.Type != MessageType.Text) return;

            var user = GetCommands(e.Message.Chat);

            await RunCommand(user, e.Message.Text);
        }
    }
}