namespace TelegramBotLib
{
    public class ButtonClass
    {
        public ButtonClass(string text, string method, string parameter)
        {
            Text = text;
            Method = method;
            Param = parameter;
        }

        public string Text { get; }
        public string Method { get; }
        public string Param { get; }
    }
}