using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TelegramBotLib
{

    public abstract class DataDict : IDictionary<string, string>, ISerializable
    {

        public readonly Dictionary<string, string> Data;

        protected DataDict(Dictionary<string, string> data)
        {
            Data = data;
        }

        public void Add(KeyValuePair<string, string> item)
        {
            Add(item.Key, item.Value);
        }

        public abstract void Clear();

        public abstract void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex);

        public abstract bool Remove(KeyValuePair<string, string> item);

        public abstract bool Remove(string key);

        public int Count => Data.Count;
        public bool IsReadOnly => false;

        public bool Contains(KeyValuePair<string, string> item)
        {
            try
            {
                var (key, value) = item;
                return Data[key] == value;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public abstract void Add(string key, string value);

        public bool ContainsKey(string key)
        {
            return Keys.Contains(key);
        }

        public bool TryGetValue(string key, out string value)
        {
            return Data.TryGetValue(key, out value);
        }

        public string this[string key]
        {
            get => Data[key];
            set => Add(key, value);
        }

        public ICollection<string> Keys => Data.Keys;
        public ICollection<string> Values => Data.Values;

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Data.GetObjectData(info, context);
        }
    }
}