using System;
using TelegramBotLib;

namespace Example
{
    class Program
    {
        static TelegramBotLib.BotController<Commands> Bot;

        static void Main()
        {
            Bot = new BotController<Commands>(new Config()
            {
                Token = "825252605:AAHsSexEPBEQUQUMFzWGeX_fDITUM9wOrkk"
            });
            Bot.StartReceiving();

            Console.ReadKey();

            Bot.StopReceiving();
        }
    }
}