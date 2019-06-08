using System;
using System.Linq;
using System.Threading.Tasks;
using TelegramBotLib;

namespace Example
{
    class Program
    {
        static BotController _bot;

        static void Main()
        {
            // Инициализируем телеграм бота
            _bot = new BotController(new Config()
            {
                Token = "825252605:AAHsSexEPBEQUQUMFzWGeX_fDITUM9wOrkk"
            });

            _bot.OnNewUser += Bot_OnNewUser;

            // Слушаем сообщения от бота/реагируем/обрабатываем и т д...
            _bot.StartReceiving();

            // Ждем нажатия любой кнопки
            Console.ReadKey();

            // Перестаем слушать сообщения
            _bot.StopReceiving();
        }

        private static (BotCommands, System.Reflection.MethodInfo[]) Bot_OnNewUser(Telegram.Bot.Types.Chat chat, Telegram.Bot.Types.User user)
        {
            Commands c = new Commands(chat);
            System.Reflection.MethodInfo[] m = c.GetType().GetMethods()
                .Where(d => d.ReturnType == typeof(Task) && BotController.CheckParams(d.GetParameters()))
                .ToArray();
            return (c, m);
        }
    }
}