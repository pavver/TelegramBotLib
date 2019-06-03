using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TelegramBotLib
{
    public class Config
    {

        public string Token { get; set; }

        public string ProxyHost { get; set; }

        public string ProxyUserName { get; set; }

        public string ProxyPass { get; set; }

    }
}