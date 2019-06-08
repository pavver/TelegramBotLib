using System;
using TelegramBotLib;

namespace Example
{
    class Program
    {
        static TelegramBotLib.BotController<Commands> Bot;

        static void Main()
        {
            // Инициализируем телеграм бота
            Bot = new BotController<Commands>(new Config()
            {
                Token = "825252605:AAHsSexEPBEQUQUMFzWGeX_fDITUM9wOrkk"
            });

            // Слушаем сообщения от бота/реагируем/обрабатываем и т д...
            Bot.StartReceiving();

            // Ждем нажатия любой кнопки
            Console.ReadKey();

            // Перестаем слушать сообщения
            Bot.StopReceiving();
        }
    }
}