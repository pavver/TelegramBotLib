using System;
using System.Collections.Generic;
using System.Linq;
using File = System.IO.File;

namespace TelegramBotLib
{
    public class DataDictFile : DataDict
    {
        public DataDictFile(string path) : base(LoadData(path))
        {
            _path = path;
        }

        private static Dictionary<string, string> LoadData(string path)
        {
            if (File.Exists(path))
                return File.ReadAllLines(path)
                    .Select(ParseLine)
                    .Where(dataLine => dataLine != null)
                    .ToDictionary(dataLine => dataLine.Value.key, dataLine => dataLine.Value.value);

            return new Dictionary<string, string> { { "LastMsgId", "0" } };

        }

        private readonly string _path;

        private static (string key, string value)? ParseLine(string text)
        {
            int i = text.IndexOf('|');
            if (i == -1) return null;
            return new ValueTuple<string, string>()
            {
                Item1 = text.Substring(0, i),
                Item2 = text.Substring(i + 1)
            };
        }

        public override void Add(string key, string value)
        {
            if (Data.ContainsKey(key))
                Data[key] = value;
            else
                Data.Add(key, value);

            SaveDataToFile();
        }

        public override void Clear()
        {
            Data.Clear();
            SaveDataToFile();
        }

        public override void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public override bool Remove(KeyValuePair<string, string> item)
        {
            throw new NotImplementedException();
        }

        public override bool Remove(string key)
        {
            if (!Data.Remove(key)) return false;
            SaveDataToFile();
            return true;
        }

        private void SaveDataToFile()
        {
            File.WriteAllLines(_path, Data.Select(d => d.Key + "|" + d.Value + "\n"));
        }
    }
}