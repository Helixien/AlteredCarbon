using System.Collections.Generic;
using Verse;

namespace AlteredCarbon
{
    public class SaveDataHandler<K, T>
    {
        public string saveKey;
        public LookMode lookMode = LookMode.Value;
        private Dictionary<K, T> data = new();

        public SaveDataHandler(string saveKey, LookMode lookMode = LookMode.Value)
        {
            this.saveKey = saveKey;
            this.lookMode = lookMode;
        }

        public T Get(K key)
        {
            if (data.TryGetValue(key, out var value))
            {
                return value;
            }
            return default;
        }

        public bool TryGet(K key, out T value)
        {
            return data.TryGetValue(key, out value);
        }

        public void Set(K key, T data)
        {
            this.data[key] = data;
        }

        public void ExposeData(K key)
        {
            var data = Get(key);
            if (lookMode == LookMode.Value)
            {
                Scribe_Values.Look(ref data, saveKey);
            }
            else if (lookMode == LookMode.Deep)
            {
                Scribe_Deep.Look(ref data, saveKey);
            }
            else if (lookMode == LookMode.Reference)
            {
                if (data is ILoadReferenceable referee)
                {
                    Scribe_References.Look(ref referee, saveKey);
                    if (referee is T castedReferee)
                    {
                        data = castedReferee;
                    }
                }
            }
            Set(key, data);
        }
    }
}

